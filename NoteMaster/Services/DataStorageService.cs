using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NoteMaster.Models;
using System.Data.SQLite;
using System.Linq;

namespace NoteMaster.Services
{
    public class DataStorageService
    {
        private readonly string _storagePath; // Notes JSON path
        private readonly string _backupPath; // Notes backup path
        private readonly string _foldersPath; // Folders JSON path
        private readonly string _foldersBackupPath; // Folders backup path
        private readonly string _dbPath; // Database path
        private readonly string _todosPath; // Todos JSON path
        private readonly string _todosBackupPath; // Todos backup path

        private readonly JsonSerializerSettings _jsonSettings; // JSON serialization settings

        public DataStorageService()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string baseDir = Path.Combine(appData, "NoteMaster");
                Directory.CreateDirectory(baseDir);

                _storagePath = Path.Combine(baseDir, "notes.json");
                _backupPath = Path.Combine(baseDir, "notes_backup.json");
                _foldersPath = Path.Combine(baseDir, "folders.json");
                _foldersBackupPath = Path.Combine(baseDir, "folders_backup.json");
                _dbPath = Path.Combine(baseDir, "notes.db");
                _todosPath = Path.Combine(baseDir, "todos.json");
                _todosBackupPath = Path.Combine(baseDir, "todos_backup.json");

                Directory.CreateDirectory(Path.GetDirectoryName(_storagePath));

                // Configure JSON serialization settings
                _jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.None,
                    Error = (sender, args) =>
                    {
                        Console.WriteLine($"JSON 序列化/反序列化错误: {args.ErrorContext.Error.Message}\n{args.ErrorContext.Error.StackTrace}");
                        args.ErrorContext.Handled = true; // Continue despite errors
                    }
                };

                InitializeDatabase();
                Console.WriteLine("DataStorageService 初始化成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DataStorageService 初始化失败: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Notes (
                        Id TEXT PRIMARY KEY,
                        FolderId TEXT,
                        Title TEXT,
                        CreatedAt TEXT,
                        UpdatedAt TEXT
                    );
                    CREATE TABLE IF NOT EXISTS Tags (
                        NoteId TEXT,
                        Tag TEXT,
                        PRIMARY KEY (NoteId, Tag)
                    );";
                cmd.ExecuteNonQuery();
                Console.WriteLine("SQLite 数据库初始化成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQLite 数据库初始化失败: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public void SaveNotes(List<Note> notes)
        {
            try
            {
                // Save to JSON
                string json = JsonConvert.SerializeObject(notes, _jsonSettings);
                File.WriteAllText(_storagePath, json);
                Console.WriteLine($"保存 {notes.Count} 条笔记到 {_storagePath}");

                // Backup
                try
                {
                    File.Copy(_storagePath, _backupPath, true);
                    Console.WriteLine("笔记备份成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"笔记备份失败: {ex.Message}\n{ex.StackTrace}");
                }

                // Save to database
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();

                foreach (var note in notes ?? new List<Note>())
                {
                    if (note == null) continue;

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                        INSERT OR REPLACE INTO Notes (Id, FolderId, Title, CreatedAt, UpdatedAt)
                        VALUES ($id, $folderId, $title, $createdAt, $updatedAt)";
                    cmd.Parameters.AddWithValue("$id", note.Id ?? "");
                    cmd.Parameters.AddWithValue("$folderId", note.FolderId.HasValue ? note.FolderId.Value.ToString() : "");
                    cmd.Parameters.AddWithValue("$title", note.Title ?? "");
                    cmd.Parameters.AddWithValue("$createdAt", note.CreatedAt.ToString("o"));
                    cmd.Parameters.AddWithValue("$updatedAt", note.UpdatedAt.ToString("o"));
                    cmd.ExecuteNonQuery();

                    // Update Tags
                    cmd.CommandText = "DELETE FROM Tags WHERE NoteId = $noteId";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$noteId", note.Id ?? "");
                    cmd.ExecuteNonQuery();

                    foreach (var tag in note.Tags ?? new List<string>())
                    {
                        if (!string.IsNullOrWhiteSpace(tag))
                        {
                            cmd.CommandText = "INSERT INTO Tags (NoteId, Tag) VALUES ($noteId, $tag)";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("$noteId", note.Id ?? "");
                            cmd.Parameters.AddWithValue("$tag", tag);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存笔记失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public List<Note> LoadNotes()
        {
            List<Note> notes = new();
            try
            {
                if (File.Exists(_storagePath))
                {
                    string json = File.ReadAllText(_storagePath);
                    notes = JsonConvert.DeserializeObject<List<Note>>(json, _jsonSettings) ?? new List<Note>();
                    Console.WriteLine($"从 JSON 加载 {notes.Count} 条笔记");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从 JSON 加载笔记失败: {ex.Message}\n{ex.StackTrace}");
            }

            try
            {
                // Load tags and metadata from database
                using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                conn.Open();

                // Load all tags
                var tagDict = new Dictionary<string, List<string>>();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT NoteId, Tag FROM Tags";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var noteId = reader.GetString(0);
                        var tag = reader.GetString(1);
                        if (!tagDict.ContainsKey(noteId))
                            tagDict[noteId] = new List<string>();
                        tagDict[noteId].Add(tag);
                    }
                }

                // Update notes with tags and metadata
                foreach (var note in notes.Where(n => n != null))
                {
                    note.Tags = tagDict.ContainsKey(note.Id) ? tagDict[note.Id] : new List<string>();

                    cmd.CommandText = "SELECT FolderId, Title, CreatedAt, UpdatedAt FROM Notes WHERE Id = $id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$id", note.Id ?? "");

                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        note.FolderId = reader.IsDBNull(0) ? note.FolderId : int.TryParse(reader.GetString(0), out var folderId) ? folderId : (int?)null;
                        note.Title = reader.IsDBNull(1) ? note.Title : reader.GetString(1);
                        note.CreatedAt = reader.IsDBNull(2) ? note.CreatedAt : DateTime.TryParse(reader.GetString(2), out var createdAt) ? createdAt : note.CreatedAt;
                        note.UpdatedAt = reader.IsDBNull(3) ? note.UpdatedAt : DateTime.TryParse(reader.GetString(3), out var updatedAt) ? updatedAt : note.UpdatedAt;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从数据库加载笔记元数据失败: {ex.Message}\n{ex.StackTrace}");
            }

            return notes;
        }

        public void SaveTodoItems(List<TodoItem> todoItems)
        {
            try
            {
                // Save to JSON
                string json = JsonConvert.SerializeObject(todoItems ?? new List<TodoItem>(), _jsonSettings);
                File.WriteAllText(_todosPath, json);
                Console.WriteLine($"保存 {todoItems?.Count ?? 0} 条待办事项到 {_todosPath}");

                // Backup
                try
                {
                    File.Copy(_todosPath, _todosBackupPath, true);
                    Console.WriteLine("待办事项备份成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"待办事项备份失败: {ex.Message}\n{ex.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存待办事项失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public List<TodoItem> LoadTodoItems()
        {
            try
            {
                if (!File.Exists(_todosPath))
                {
                    Console.WriteLine($"待办文件 {_todosPath} 不存在，返回空列表");
                    return new List<TodoItem>();
                }

                string json = File.ReadAllText(_todosPath);
                var items = JsonConvert.DeserializeObject<List<TodoItem>>(json, _jsonSettings) ?? new List<TodoItem>();

                // Validate TodoItems to ensure DueDate is DateTime?
                var validItems = items
                    .Where(item => item != null && (item.DueDate == null || item.DueDate is DateTime))
                    .ToList();

                if (validItems.Count < items.Count)
                {
                    Console.WriteLine($"过滤了 {items.Count - validItems.Count} 条无效待办事项（DueDate 类型错误）");
                }

                Console.WriteLine($"成功加载 {validItems.Count} 条待办事项");
                return validItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从 JSON 加载待办事项失败: {ex.Message}\n{ex.StackTrace}");
                return new List<TodoItem>();
            }
        }

        public void SaveFolders(List<Folder> folders)
        {
            try
            {
                string json = JsonConvert.SerializeObject(folders ?? new List<Folder>(), _jsonSettings);
                File.WriteAllText(_foldersPath, json);
                Console.WriteLine($"保存 {folders?.Count ?? 0} 个文件夹到 {_foldersPath}");

                try
                {
                    File.Copy(_foldersPath, _foldersBackupPath, true);
                    Console.WriteLine("文件夹备份成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"文件夹备份失败: {ex.Message}\n{ex.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存文件夹失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public List<Folder> LoadFolders()
        {
            try
            {
                if (!File.Exists(_foldersPath))
                {
                    Console.WriteLine($"文件夹文件 {_foldersPath} 不存在，返回空列表");
                    return new List<Folder>();
                }

                string json = File.ReadAllText(_foldersPath);
                var folders = JsonConvert.DeserializeObject<List<Folder>>(json, _jsonSettings) ?? new List<Folder>();
                Console.WriteLine($"成功加载 {folders.Count} 个文件夹");
                return folders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从 JSON 加载文件夹失败: {ex.Message}\n{ex.StackTrace}");
                return new List<Folder>();
            }
        }
    }
}
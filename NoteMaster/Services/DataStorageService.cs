using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Newtonsoft.Json;
using NoteMaster.Models;

namespace NoteMaster.Services
{
    public class DataStorageService
    {
        private readonly string _jsonPath; // JSON 文件路径
        private readonly string _backupPath; // JSON 备份路径
        private readonly string _dbPath; // SQLite 数据库路径

        public DataStorageService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(appData))
            {
                // 回退到项目目录
                appData = Directory.GetCurrentDirectory();
                Console.WriteLine("Warning: LocalApplicationData is empty, using project directory.");
            }

            _jsonPath = Path.Combine(appData, "NoteMaster", "notes.json");
            _backupPath = Path.Combine(appData, "NoteMaster", "notes_backup.json");
            _dbPath = Path.Combine(appData, "NoteMaster", "notes.db");

            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath));
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                if (string.IsNullOrEmpty(_dbPath))
                {
                    throw new InvalidOperationException("Database path is empty.");
                }

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
                    )";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize database: {ex.Message}");
                throw;
            }
        }

        public void SaveNotes(List<Note> notes)
        {
            // 保存到 JSON
            string json = JsonConvert.SerializeObject(notes, Formatting.Indented);
            File.WriteAllText(_jsonPath, json);

            // 生成备份
            try
            {
                File.Copy(_jsonPath, _backupPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create backup: {ex.Message}");
            }

            // 保存到 SQLite
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            foreach (var note in notes)
            {
                // 保存笔记元数据
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT OR REPLACE INTO Notes (Id, FolderId, Title, CreatedAt, UpdatedAt)
                    VALUES ($id, $folderId, $title, $createdAt, $updatedAt)";
                cmd.Parameters.AddWithValue("$id", note.Id);
                cmd.Parameters.AddWithValue("$folderId", note.FolderId ?? "");
                cmd.Parameters.AddWithValue("$title", note.Title ?? "");
                cmd.Parameters.AddWithValue("$createdAt", note.CreatedAt.ToString("o"));
                cmd.Parameters.AddWithValue("$updatedAt", note.UpdatedAt.ToString("o"));
                cmd.ExecuteNonQuery();

                // 保存标签
                cmd.CommandText = "DELETE FROM Tags WHERE NoteId = $noteId";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("$noteId", note.Id);
                cmd.ExecuteNonQuery();

                foreach (var tag in note.Tags ?? new List<string>())
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        cmd.CommandText = "INSERT INTO Tags (NoteId, Tag) VALUES ($noteId, $tag)";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("$noteId", note.Id);
                        cmd.Parameters.AddWithValue("$tag", tag);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<Note> LoadNotes()
        {
            // 从 JSON 加载笔记
            List<Note> notes = new List<Note>();
            try
            {
                if (File.Exists(_jsonPath))
                {
                    string json = File.ReadAllText(_jsonPath);
                    notes = JsonConvert.DeserializeObject<List<Note>>(json) ?? new List<Note>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load JSON notes: {ex.Message}");
            }

            // 从 SQLite 加载标签和 FolderId
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();

            // 加载标签
            var tags = new Dictionary<string, List<string>>();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT NoteId, Tag FROM Tags";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var noteId = reader.GetString(0);
                    var tag = reader.GetString(1);
                    if (!tags.ContainsKey(noteId)) tags[noteId] = new List<string>();
                    tags[noteId].Add(tag);
                }
            }

            // 加载 FolderId 和元数据
            foreach (var note in notes)
            {
                note.Tags = tags.ContainsKey(note.Id) ? tags[note.Id] : new List<string>();

                cmd.CommandText = "SELECT FolderId, Title, CreatedAt, UpdatedAt FROM Notes WHERE Id = $id";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("$id", note.Id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        note.FolderId = reader.IsDBNull(0) ? null : reader.GetString(0);
                        // 可选：同步 Title 和时间（如果 JSON 和 SQLite 不一致）
                        note.Title = reader.IsDBNull(1) ? note.Title : reader.GetString(1);
                        note.CreatedAt = reader.IsDBNull(2) ? note.CreatedAt : DateTime.Parse(reader.GetString(2));
                        note.UpdatedAt = reader.IsDBNull(3) ? note.UpdatedAt : DateTime.Parse(reader.GetString(3));
                    }
                }
            }

            return notes;
        }
    }
}
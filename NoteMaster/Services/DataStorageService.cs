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
        private readonly string _jsonPath; // JSON �ļ�·��
        private readonly string _backupPath; // JSON ����·��
        private readonly string _dbPath; // SQLite ���ݿ�·��

        public DataStorageService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(appData))
            {
                // ���˵���ĿĿ¼
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
            // ���浽 JSON
            string json = JsonConvert.SerializeObject(notes, Formatting.Indented);
            File.WriteAllText(_jsonPath, json);

            // ���ɱ���
            try
            {
                File.Copy(_jsonPath, _backupPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create backup: {ex.Message}");
            }

            // ���浽 SQLite
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();
            foreach (var note in notes)
            {
                // ����ʼ�Ԫ����
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

                // �����ǩ
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
            // �� JSON ���رʼ�
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

            // �� SQLite ���ر�ǩ�� FolderId
            using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            conn.Open();

            // ���ر�ǩ
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

            // ���� FolderId ��Ԫ����
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
                        // ��ѡ��ͬ�� Title ��ʱ�䣨��� JSON �� SQLite ��һ�£�
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
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NoteMaster.Models;
using System.Data.SQLite;

namespace NoteMaster.Services
{
	public class DataStorageService
	{
		private readonly string _storagePath;//json·��
		private readonly string _backupPath;// ����·��
		private readonly string _foldersPath;// �ļ���·��
		private readonly string _foldersBackupPath;// ����·��
        private readonly string _dbPath;                // ���ݿ�·��

		public DataStorageService()
		{
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string baseDir = Path.Combine(appData, "NoteMaster");
			Directory.CreateDirectory(baseDir);

			_storagePath = Path.Combine(baseDir, "notes.json");
			_backupPath = Path.Combine(baseDir, "notes_backup.json");
			_foldersPath = Path.Combine(baseDir, "folders.json");
			_foldersBackupPath = Path.Combine(baseDir, "folders_backup.json");
            _dbPath = Path.Combine(baseDir, "notes.db");
            Directory.CreateDirectory(Path.GetDirectoryName(_storagePath));
            InitializeDatabase(); // ��ʼ�����ݿ�
            }
        
  private void InitializeDatabase()// ��ʼ�����ݿ�
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
        File.WriteAllText(_storagePath, json);

        try { File.Copy(_storagePath, _backupPath, true); }
        catch (Exception ex) { Console.WriteLine($"Failed to backup notes: {ex.Message}"); }

        // ���浽���ݿ�
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        conn.Open();

        foreach (var note in notes)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT OR REPLACE INTO Notes (Id, FolderId, Title, CreatedAt, UpdatedAt)
                VALUES ($id, $folderId, $title, $createdAt, $updatedAt)";
            cmd.Parameters.AddWithValue("$id", note.Id);
            cmd.Parameters.AddWithValue("$folderId", note.FolderId.HasValue ? note.FolderId.Value.ToString() : "");
            cmd.Parameters.AddWithValue("$title", note.Title ?? "");
            cmd.Parameters.AddWithValue("$createdAt", note.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("$updatedAt", note.UpdatedAt.ToString("o"));
            cmd.ExecuteNonQuery();

            // ���� Tags
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
        List<Note> notes = new();

        try
        {
            if (File.Exists(_storagePath))
            {
                string json = File.ReadAllText(_storagePath);
                notes = JsonConvert.DeserializeObject<List<Note>>(json) ?? new List<Note>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load notes from JSON: {ex.Message}");
        }

        // �����ݿ���ر�ǩ��Ԫ��Ϣ
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        conn.Open();

        // �������б�ǩ
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

        // ���� notes �еı�ǩ��Ԫ����
        foreach (var note in notes)
        {
            note.Tags = tagDict.ContainsKey(note.Id) ? tagDict[note.Id] : new List<string>();

            cmd.CommandText = "SELECT FolderId, Title, CreatedAt, UpdatedAt FROM Notes WHERE Id = $id";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("$id", note.Id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                note.FolderId = reader.IsDBNull(0) ? note.FolderId : int.TryParse(reader.GetString(0), out var folderId) ? folderId : (int?)null;
                note.Title = reader.IsDBNull(1) ? note.Title : reader.GetString(1);
                note.CreatedAt = reader.IsDBNull(2) ? note.CreatedAt : DateTime.Parse(reader.GetString(2));
                note.UpdatedAt = reader.IsDBNull(3) ? note.UpdatedAt : DateTime.Parse(reader.GetString(3));
            }
        }

        return notes;
    }

    // ? ���Ķ� Folder ��ز��� ?
    public void SaveFolders(List<Folder> folders)
    {
        string json = JsonConvert.SerializeObject(folders, Formatting.Indented);
        File.WriteAllText(_foldersPath, json);
        File.Copy(_foldersPath, _foldersBackupPath, true);
    }

    public List<Folder> LoadFolders()
    {
        if (!File.Exists(_foldersPath))
            return new List<Folder>();

        string json = File.ReadAllText(_foldersPath);
        return JsonConvert.DeserializeObject<List<Folder>>(json) ?? new List<Folder>();
    }
    }
}
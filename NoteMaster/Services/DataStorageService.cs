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
		private readonly string _storagePath;
		private readonly string _backupPath;
        private readonly string _jsonPath;//json�ļ�·��
        private readonly string _dbPath;//���ݿ��ļ�·��
		public DataStorageService()
		{
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			_storagePath = Path.Combine(appData, "NoteMaster", "notes.json");
			_backupPath = Path.Combine(appData, "NoteMaster", "notes_backup.json");

			Directory.CreateDirectory(Path.GetDirectoryName(_storagePath));
			InitializeDatabase();
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
                    )";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // ��¼���󣨿��滻Ϊ��־��ܣ�
                Console.WriteLine($"Failed to initialize database: {ex.Message}");
                throw;
            }
        }
		public void SaveNotes(List<Note> notes)
		{
			string json = JsonConvert.SerializeObject(notes, Formatting.Indented);
			File.WriteAllText(_storagePath, json);

			// ���ɱ���
			File.Copy(_storagePath, _backupPath, true);
		}

		public List<Note> LoadNotes()
		{
			if (!File.Exists(_storagePath))
				return new List<Note>();

			string json = File.ReadAllText(_storagePath);
			return JsonConvert.DeserializeObject<List<Note>>(json) ?? new List<Note>();
		}
	}
}
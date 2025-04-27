using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NoteMaster.Models;
namespace NoteMaster.Services
{
	public class DataStorageService
	{
		private readonly string _storagePath;
		private readonly string _backupPath;

		public DataStorageService()
		{
			// 参考 NoteBot 的路径设置
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			_storagePath = Path.Combine(appData, "NoteMaster", "notes.json");
			_backupPath = Path.Combine(appData, "NoteMaster", "notes_backup.json");

			Directory.CreateDirectory(Path.GetDirectoryName(_storagePath));
		}

		public void SaveNotes(List<Note> notes)
		{
			string json = JsonConvert.SerializeObject(notes, Formatting.Indented);
			File.WriteAllText(_storagePath, json);

			// 生成备份
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
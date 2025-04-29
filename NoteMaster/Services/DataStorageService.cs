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
		private readonly string _foldersPath;
		private readonly string _foldersBackupPath;

		public DataStorageService()
		{
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string baseDir = Path.Combine(appData, "NoteMaster");
			Directory.CreateDirectory(baseDir);

			_storagePath = Path.Combine(baseDir, "notes.json");
			_backupPath = Path.Combine(baseDir, "notes_backup.json");
			_foldersPath = Path.Combine(baseDir, "folders.json");
			_foldersBackupPath = Path.Combine(baseDir, "folders_backup.json");

			Directory.CreateDirectory(Path.GetDirectoryName(_storagePath));
		}

		public void SaveNotes(List<Note> notes)
		{
			string json = JsonConvert.SerializeObject(notes, Formatting.Indented);
			File.WriteAllText(_storagePath, json);
			File.Copy(_storagePath, _backupPath, true);
		}

		public List<Note> LoadNotes()
		{
			if (!File.Exists(_storagePath))
				return new List<Note>();

			string json = File.ReadAllText(_storagePath);
			return JsonConvert.DeserializeObject<List<Note>>(json) ?? new List<Note>();
		}

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
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
			// �ο� NoteBot ��·������
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			_storagePath = Path.Combine(appData, "NoteMaster", "notes.json");
			_backupPath = Path.Combine(appData, "NoteMaster", "notes_backup.json");

			Directory.CreateDirectory(Path.GetDirectoryName(_storagePath));
		}

		public void SaveNotes(List<Note> notes)
		{
			string json = JsonConvert.SerializeObject(notes, Formatting.Indented);
			File.WriteAllText(_storagePath, json);

			// ���ɱ���
			File.Copy(_storagePath, _backupPath, true);
		}

		public IEnumerable<Note> LoadNotes()
		{
			// 临时返回空列表，实际应用中这里应该从数据库或文件加载数据
			return new List<Note>();
		}
	}
}
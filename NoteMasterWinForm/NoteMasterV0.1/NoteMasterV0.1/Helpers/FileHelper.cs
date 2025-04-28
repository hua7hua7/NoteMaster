using System.Text.Json;
using NoteMasterV0._1.Models;

namespace NoteMasterV0._1.Helpers
{
    public static class FileHelper
    {
        private static readonly string BaseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "NoteMaster");

        public static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
            }
        }

        public static async Task SaveNoteToFileAsync(Note note)
        {
            EnsureDirectoryExists();
            var filePath = Path.Combine(BaseDirectory, $"{note.Id}.json");
            var json = JsonSerializer.Serialize(note);
            await File.WriteAllTextAsync(filePath, json);
        }

        public static async Task<Note?> LoadNoteFromFileAsync(int noteId)
        {
            var filePath = Path.Combine(BaseDirectory, $"{noteId}.json");
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<Note>(json);
            }
            return null;
        }

        public static void CreateBackup(Note note)
        {
            EnsureDirectoryExists();
            var backupDir = Path.Combine(BaseDirectory, "Backups");
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            var backupPath = Path.Combine(backupDir, $"{note.Id}_{DateTime.Now:yyyyMMddHHmmss}.json");
            var json = JsonSerializer.Serialize(note);
            File.WriteAllText(backupPath, json);
        }

        public static void DeleteNoteFile(int noteId)
        {
            var filePath = Path.Combine(BaseDirectory, $"{noteId}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
} 
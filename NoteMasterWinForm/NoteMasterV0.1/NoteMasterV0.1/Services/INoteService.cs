using NoteMasterV0._1.Models;

namespace NoteMasterV0._1.Services
{
    public interface INoteService
    {
        Task<List<Note>> GetAllNotesAsync();
        Task<Note?> GetNoteByIdAsync(int id);
        Task<Note> CreateNoteAsync(Note note);
        Task<Note> UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(int id);
        Task<List<Note>> SearchNotesAsync(string keyword);
        Task<List<Note>> GetNotesByTagAsync(string tag);
        Task ArchiveNoteAsync(int id);
        Task UnarchiveNoteAsync(int id);
    }
} 
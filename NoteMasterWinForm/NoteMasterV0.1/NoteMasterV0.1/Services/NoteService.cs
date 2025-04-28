using Microsoft.EntityFrameworkCore;
using NoteMasterV0._1.Data;
using NoteMasterV0._1.Models;

namespace NoteMasterV0._1.Services
{
    public class NoteService : INoteService
    {
        private readonly NoteDbContext _context;

        public NoteService(NoteDbContext context)
        {
            _context = context;
        }

        public async Task<List<Note>> GetAllNotesAsync()
        {
            return await _context.Notes.ToListAsync();
        }

        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            return await _context.Notes.FindAsync(id);
        }

        public async Task<Note> CreateNoteAsync(Note note)
        {
            note.CreatedAt = DateTime.Now;
            note.UpdatedAt = DateTime.Now;
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<Note> UpdateNoteAsync(Note note)
        {
            note.UpdatedAt = DateTime.Now;
            _context.Entry(note).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task DeleteNoteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Note>> SearchNotesAsync(string keyword)
        {
            return await _context.Notes
                .Where(n => n.Title.Contains(keyword) || n.Content.Contains(keyword))
                .ToListAsync();
        }

        public async Task<List<Note>> GetNotesByTagAsync(string tag)
        {
            return await _context.Notes
                .Where(n => n.Tags.Contains(tag))
                .ToListAsync();
        }

        public async Task ArchiveNoteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                note.IsArchived = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnarchiveNoteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                note.IsArchived = false;
                await _context.SaveChangesAsync();
            }
        }
    }
} 
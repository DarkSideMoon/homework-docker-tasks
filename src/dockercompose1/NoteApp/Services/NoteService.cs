using NoteApp.Cache;
using NoteApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoteApp.Services
{
    public interface INoteService
    {
        Task<IEnumerable<Note>> GetAllNotes();

        Task<bool> SetNote(Note note);
    }

    public class NoteService : INoteService
    {
        public readonly IStorage<Note> _storage;

        public NoteService(IStorage<Note> storage)
        {
            _storage = storage;
        }

        public async Task<IEnumerable<Note>> GetAllNotes()
        {
            return await _storage.GetAllItems();
        }

        public async Task<bool> SetNote(Note note)
        {
            var id = new Random().Next(1, 1000);
            note.Id = id.ToString();
            note.CreateTime = DateTime.Now;
            await _storage.SetItem(id.ToString(), note);
            return true;
        }
    }
}

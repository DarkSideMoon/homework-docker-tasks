using NoteApp.Cache;
using System;

namespace NoteApp.Model
{
    public class Note : IStorageId
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public DateTime CreateTime { get; set; }

        string IStorageId.Id => Id;
    }
}

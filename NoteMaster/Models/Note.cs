using System;
using System.Collections.Generic;

namespace NoteMaster.Models
{
    public class Note
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public List<string> Tags { get; set; } = new List<string>();
        public int? FolderId { get; set; }
    }
}
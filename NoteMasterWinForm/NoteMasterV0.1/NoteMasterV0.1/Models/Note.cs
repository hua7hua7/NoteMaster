using System;
using System.Collections.Generic;

namespace NoteMasterV0._1.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public bool IsArchived { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
} 
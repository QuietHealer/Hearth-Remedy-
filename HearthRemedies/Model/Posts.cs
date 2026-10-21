using System;
using System.Collections.Generic;
using System.Text;

namespace HearthRemedies.Model
{
    public class Posts
    {

        public required string Name { get; set; }
        public string Description { get; set; } = "";
        public required List<string> Ingredients { get; set; }
        public required List<string> Steps {  get; set; }
        public required string Category { get; set; }
        public string? UsageMethod { get; set; }
        public int UpVoteNum { get; set; } = 0;
        public int DownVoteNum { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public bool IsHidden { get; set; } = false;
        public int ReportCount { get; set; } = 0;
        public string? IngUrl { get; set; }


    }
}

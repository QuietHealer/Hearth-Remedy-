using System;
using System.Collections.Generic;
using System.Text;

namespace HearthRemedies.Model
{
    public class Posts
    {
        private int nextId = 0;
        public int Id { get; private set; }
        public required string Name { get; set; }
        public string Description { get; set; } = "";
        public required List<string> Ingredients { get; set; }
        public required List<string> Steps {  get; set; }
        public required RemedyType Category { get; set; }
        public required RemedyMethod UsageMethod { get; set; }
        public int UpVoteNum { get; set; } = 0;
        public int DownVoteNum { get; set; } = 0;
        public DateTime CreatedAt { get; private set; }
        public bool IsHidden { get; set; } = false;
        public int ReportCount { get; set; } = 0;
        public string? ImgUrl { get; set; }

        public Posts()
        {
            Id = ++nextId;
            CreatedAt = DateTime.Now;
        }

        public void changeUpVote(int numChange)
        {
            UpVoteNum += numChange;
        }

        public void changeReport(int numChange)
        {
            ReportCount += numChange;
        }

        public void changeDownVote(int numChange)
        {
            DownVoteNum += numChange;
        }

    }
}

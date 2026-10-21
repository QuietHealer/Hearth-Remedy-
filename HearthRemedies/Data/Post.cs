using Microsoft.EntityFrameworkCore;
using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(HearthRemedies.Models.Post))]

namespace HearthRemedies.Data
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Ingredients { get; set; } = "";
        public string Steps { get; set; } = "";
        public string Category { get; set; } = "";
        public string UsageMethod { get; set; } = ""; // skin, eat, breathe
        public int Score { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsHidden { get; set; }
        public int ReportCount { get; set; }
        //public void Configuration(IAppBuilder app)
        //{
        //    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
        //}
    }
}



// Data/AppDbContext.cs


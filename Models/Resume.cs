using System;
using System.ComponentModel.DataAnnotations;

namespace AIResumeAnalyzer.Models
{
    public class Resume
    {
        [Key]
        public int Id { get; set; }

        public required string FileName { get; set; }

        public required string ResumeText { get; set; }

        public required string JobDescription { get; set; }

        public int Score { get; set; }

        public required string AIResult { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

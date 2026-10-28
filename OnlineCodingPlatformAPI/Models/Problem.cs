using OnlineCodingAPI.Enums;
using OnlineCodingPlatformAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineCodingAPI.Models
{
    public class Problem
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;

        public string InputFormat { get; set; } = string.Empty;
        public string OutputFormat { get; set; } = string.Empty;
        public string Constraints { get; set; } = string.Empty;

        public string SampleInput { get; set; } = string.Empty;
        public string SampleOutput { get; set; } = string.Empty;

        public int TimeLimitMs { get; set; } = 2000;
        public int MemoryLimitKb { get; set; } = 256000;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public ICollection<Progress> ProgressRecords { get; set; } = new List<Progress>();
    }
}

using OnlineCodingAPI.Enums;

namespace OnlineCodingAPI.Models
{
    public class Submission
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int ProblemId { get; set; }
        public Problem? Problem { get; set; }

        public string Code { get; set; } = string.Empty;

        // Judge0 language id (e.g. 71 = Python3, 62 = Java, 54 = C++17)
        public int LanguageId { get; set; }

        public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;

        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }

        public double? ExecutionTimeMs { get; set; }
        public int? MemoryUsedKb { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}

using OnlineCodingAPI.Enums;

namespace OnlineCodingAPI.Models
{
    public class Progress
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int ProblemId { get; set; }
        public Problem? Problem { get; set; }

        public ProgressStatus Status { get; set; } = ProgressStatus.NotStarted;

        public int AttemptsCount { get; set; } = 0;

        public DateTime? SolvedAt { get; set; }
        public DateTime LastAttemptAt { get; set; } = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;

namespace OnlineCodingAPI.Dtos
{
    public class CreateSubmissionDto
    {
        [Required]
        public int ProblemId { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public int LanguageId { get; set; }
    }


    // Used when returning complete submission details
    public class SubmissionResultDto
    {
        public int Id { get; set; }

        public int ProblemId { get; set; }

        public string ProblemTitle { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? Output { get; set; }

        public string? ErrorMessage { get; set; }

        public double? ExecutionTimeMs { get; set; }

        public int? MemoryUsedKb { get; set; }

        public DateTime SubmittedAt { get; set; }
    }


    // Used for student's submission history
    public class SubmissionSummaryDto
    {
        public int Id { get; set; }

        public int ProblemId { get; set; }

        public string ProblemTitle { get; set; } = string.Empty;

        public int LanguageId { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; }
    }


    // Used when Admin views all student submissions
    public class AdminSubmissionDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public int ProblemId { get; set; }

        public string ProblemTitle { get; set; } = string.Empty;

        public int LanguageId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? Output { get; set; }

        public string? ErrorMessage { get; set; }

        public double? ExecutionTimeMs { get; set; }

        public int? MemoryUsedKb { get; set; }

        public DateTime SubmittedAt { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using OnlineCodingAPI.Enums;

namespace OnlineCodingAPI.Dtos
{
    public class CreateProblemDto
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public DifficultyLevel Difficulty { get; set; }
            = DifficultyLevel.Easy;

        public string InputFormat { get; set; } = string.Empty;

        public string OutputFormat { get; set; } = string.Empty;

        public string Constraints { get; set; } = string.Empty;

        public string SampleInput { get; set; } = string.Empty;

        public string SampleOutput { get; set; } = string.Empty;

        public int TimeLimitMs { get; set; } = 2000;

        public int MemoryLimitKb { get; set; } = 256000;

        public List<CreateTestCaseDto> TestCases { get; set; }
            = new();
    }


    public class UpdateProblemDto : CreateProblemDto
    {
    }


    public class CreateTestCaseDto
    {
        [Required]
        public string Input { get; set; } = string.Empty;

        [Required]
        public string ExpectedOutput { get; set; } = string.Empty;

        public bool IsSample { get; set; } = false;

        public bool IsHidden { get; set; } = true;
    }


    // ============================================================
    // PROBLEM SUMMARY
    // ============================================================

    public class ProblemSummaryDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Difficulty { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        // Not Started / Pending / Completed
        public string Status { get; set; } = "Not Started";
    }


    // ============================================================
    // PROBLEM DETAIL
    // ============================================================

    public class ProblemDetailDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Difficulty { get; set; } = string.Empty;

        public string InputFormat { get; set; } = string.Empty;

        public string OutputFormat { get; set; } = string.Empty;

        public string Constraints { get; set; } = string.Empty;

        public string SampleInput { get; set; } = string.Empty;

        public string SampleOutput { get; set; } = string.Empty;

        public int TimeLimitMs { get; set; }

        public int MemoryLimitKb { get; set; }

        // Not Started / Pending / Completed
        public string Status { get; set; } = "Not Started";

        public List<SampleTestCaseDto> SampleTestCases { get; set; }
            = new();
    }


    public class SampleTestCaseDto
    {
        public string Input { get; set; } = string.Empty;

        public string ExpectedOutput { get; set; } = string.Empty;
    }
}
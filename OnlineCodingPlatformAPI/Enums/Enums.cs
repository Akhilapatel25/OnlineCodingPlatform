
namespace OnlineCodingAPI.Enums
{
    public enum UserRole
    {
        User = 0,
        Admin = 1
    }

    public enum DifficultyLevel
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    public enum SubmissionStatus
    {
        Pending = 0,
        Judging = 1,
        Accepted = 2,
        WrongAnswer = 3,
        TimeLimitExceeded = 4,
        MemoryLimitExceeded = 5,
        RuntimeError = 6,
        CompileError = 7,
        InternalError = 8
    }

    public enum ProgressStatus
    {
        NotStarted = 0,
        Attempted = 1,
        Solved = 2
    }
}

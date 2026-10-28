namespace OnlineCodingAPI.Dtos
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ProblemsSolved { get; set; }
        public int TotalSubmissions { get; set; }
    }

    public class UpdateUserDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class UserProgressDto
    {
        public int ProblemId { get; set; }
        public string ProblemTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int AttemptsCount { get; set; }
        public DateTime? SolvedAt { get; set; }
    }
}

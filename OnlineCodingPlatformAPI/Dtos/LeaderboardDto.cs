namespace OnlineCodingAPI.Dtos
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int ProblemsSolved { get; set; }
        public int TotalSubmissions { get; set; }
    }
}

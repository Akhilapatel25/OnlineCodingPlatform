namespace OnlineCodingAPI.Models
{
    public class TestCase
    {
        public int Id { get; set; }

        public int ProblemId { get; set; }
        public Problem? Problem { get; set; }

        public string Input { get; set; } = string.Empty;
        public string ExpectedOutput { get; set; } = string.Empty;

        // Sample test cases are shown to the user; hidden ones are used only for judging
        public bool IsSample { get; set; } = false;
        public bool IsHidden { get; set; } = true;
    }
}

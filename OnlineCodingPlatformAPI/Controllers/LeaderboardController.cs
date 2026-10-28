
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCodingAPI.Data;
using OnlineCodingAPI.Dtos;
using OnlineCodingAPI.Enums;

namespace OnlineCodingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class LeaderboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaderboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/leaderboard?top=50
        [HttpGet]
        public async Task<ActionResult<List<LeaderboardEntryDto>>> GetLeaderboard(
            [FromQuery] int top = 50)
        {
            var raw = await _context.Users
                // Only normal users should appear on the leaderboard
                .Where(u => u.Role == UserRole.User)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    ProblemsSolved = u.ProgressRecords
                        .Count(p => p.Status == ProgressStatus.Solved),

                    TotalSubmissions = u.Submissions.Count
                })
                .OrderByDescending(u => u.ProblemsSolved)
                .ThenBy(u => u.TotalSubmissions)
                .Take(top)
                .ToListAsync();
    
            var leaderboard = raw
                .Select((entry, index) => new LeaderboardEntryDto
                {
                    Rank = index + 1,
                    UserId = entry.Id,
                    Username = entry.Username,
                    ProblemsSolved = entry.ProblemsSolved,
                    TotalSubmissions = entry.TotalSubmissions
                })
                .ToList();

            return Ok(leaderboard);
        }
    }
}

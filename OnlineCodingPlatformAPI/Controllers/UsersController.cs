/*using System.Security.Claims;
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
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/users/me
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userId = GetUserId();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return NotFound();

            var solvedCount = await _context.ProgressRecords
                .CountAsync(p => p.UserId == userId && p.Status == ProgressStatus.Solved);

            var submissionCount = await _context.Submissions
                .CountAsync(s => s.UserId == userId);

            return Ok(new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                ProblemsSolved = solvedCount,
                TotalSubmissions = submissionCount
            });
        }

        // PUT api/users/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(UpdateUserDto dto)
        {
            var userId = GetUserId();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                var taken = await _context.Users.AnyAsync(u => u.Username == dto.Username && u.Id != userId);
                if (taken) return Conflict(new { message = "Username already taken." });
                user.Username = dto.Username;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET api/users/me/progress
        [HttpGet("me/progress")]
        public async Task<ActionResult<List<UserProgressDto>>> GetMyProgress()
        {
            var userId = GetUserId();

            var progress = await _context.ProgressRecords
                .Include(p => p.Problem)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.LastAttemptAt)
                .Select(p => new UserProgressDto
                {
                    ProblemId = p.ProblemId,
                    ProblemTitle = p.Problem!.Title,
                    Status = p.Status.ToString(),
                    AttemptsCount = p.AttemptsCount,
                    SolvedAt = p.SolvedAt
                })
                .ToListAsync();

            return Ok(progress);
        }

        // GET api/users/5  (Admin only - view any user's public profile)
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserProfileDto>> GetById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null) return NotFound();

            var solvedCount = await _context.ProgressRecords
                .CountAsync(p => p.UserId == id && p.Status == ProgressStatus.Solved);

            var submissionCount = await _context.Submissions.CountAsync(s => s.UserId == id);

            return Ok(new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                ProblemsSolved = solvedCount,
                TotalSubmissions = submissionCount
            });
        }

        private int GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idClaim!);
        }
    }
}
  */
using System.Security.Claims;
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
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/users/me
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userId = GetUserId();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
                return NotFound();

            var solvedCount = await _context.ProgressRecords
                .CountAsync(p =>
                    p.UserId == userId &&
                    p.Status == ProgressStatus.Solved);

            var submissionCount = await _context.Submissions
                .CountAsync(s => s.UserId == userId);

            return Ok(new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                ProblemsSolved = solvedCount,
                TotalSubmissions = submissionCount
            });
        }

        // PUT api/users/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(UpdateUserDto dto)
        {
            var userId = GetUserId();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                var taken = await _context.Users
                    .AnyAsync(u =>
                        u.Username == dto.Username &&
                        u.Id != userId);

                if (taken)
                    return Conflict(new
                    {
                        message = "Username already taken."
                    });

                user.Username = dto.Username;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET api/users/me/progress
        [HttpGet("me/progress")]
        public async Task<ActionResult<List<UserProgressDto>>> GetMyProgress()
        {
            var userId = GetUserId();

            var progress = await _context.ProgressRecords
                .Include(p => p.Problem)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.LastAttemptAt)
                .Select(p => new UserProgressDto
                {
                    ProblemId = p.ProblemId,
                    ProblemTitle = p.Problem!.Title,
                    Status = p.Status.ToString(),
                    AttemptsCount = p.AttemptsCount,
                    SolvedAt = p.SolvedAt
                })
                .ToListAsync();

            return Ok(progress);
        }

        // GET api/users/all
        // Admin only - view all users
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserProfileDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .Select(user => new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role.ToString(),

                    CreatedAt = user.CreatedAt,

                    ProblemsSolved = _context.ProgressRecords
                        .Count(p =>
                            p.UserId == user.Id &&
                            p.Status == ProgressStatus.Solved),

                    TotalSubmissions = _context.Submissions
                        .Count(s => s.UserId == user.Id)
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET api/users/5
        // Admin only - view any user's public profile
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserProfileDto>> GetById(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
                return NotFound();

            var solvedCount = await _context.ProgressRecords
                .CountAsync(p =>
                    p.UserId == id &&
                    p.Status == ProgressStatus.Solved);

            var submissionCount = await _context.Submissions
                .CountAsync(s => s.UserId == id);

            return Ok(new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                ProblemsSolved = solvedCount,
                TotalSubmissions = submissionCount
            });
        }

        private int GetUserId()
        {
            var idClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.Parse(idClaim!);
        }
    }
}
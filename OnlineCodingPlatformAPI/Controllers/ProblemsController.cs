using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCodingAPI.Dtos;
using OnlineCodingAPI.Services;

namespace OnlineCodingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProblemsController : ControllerBase
    {
        private readonly ProblemService _problemService;

        public ProblemsController(ProblemService problemService)
        {
            _problemService = problemService;
        }

        // GET api/problems?difficulty=Easy
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProblemSummaryDto>>> GetAll(
            [FromQuery] string? difficulty)
        {
            var userId = GetUserId();

            return Ok(
                await _problemService.GetAllAsync(
                    difficulty,
                    userId
                )
            );
        }

        // GET api/problems/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProblemDetailDto>> GetById(int id)
        {
            var userId = GetUserId();

            var problem =
                await _problemService.GetByIdAsync(
                    id,
                    userId
                );

            if (problem is null)
            {
                return NotFound(new
                {
                    message = $"Problem {id} not found."
                });
            }

            return Ok(problem);
        }

        // POST api/problems
        // Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProblemDetailDto>> Create(
            CreateProblemDto dto)
        {
            var problem =
                await _problemService.CreateAsync(dto);

            var created =
                await _problemService.GetByIdAsync(
                    problem.Id,
                    GetUserId()
                );

            return CreatedAtAction(
                nameof(GetById),
                new { id = problem.Id },
                created
            );
        }

        // PUT api/problems/5
        // Admin only
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            int id,
            UpdateProblemDto dto)
        {
            var updated =
                await _problemService.UpdateAsync(
                    id,
                    dto
                );

            if (!updated)
            {
                return NotFound(new
                {
                    message = $"Problem {id} not found."
                });
            }

            return NoContent();
        }

        // DELETE api/problems/5
        // Admin only
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted =
                await _problemService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = $"Problem {id} not found."
                });
            }

            return NoContent();
        }

        // Get logged-in user's ID from JWT
        private int? GetUserId()
        {
            var idClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (string.IsNullOrEmpty(idClaim))
            {
                return null;
            }

            if (int.TryParse(idClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
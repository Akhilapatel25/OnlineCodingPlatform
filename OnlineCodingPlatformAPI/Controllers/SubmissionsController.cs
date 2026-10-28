using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCodingAPI.Dtos;
using OnlineCodingAPI.Enums;
using OnlineCodingAPI.Services;
using System.Security.Claims;

namespace OnlineCodingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubmissionsController : ControllerBase
    {
        private readonly SubmissionService _submissionService;

        public SubmissionsController(SubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        // POST api/submissions
        // Student submits code
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<SubmissionResultDto>> Submit(
            CreateSubmissionDto dto)
        {
            var result = await _submissionService
                .SubmitAsync(GetUserId(), dto);

            if (result is null)
            {
                return NotFound(new
                {
                    message = $"Problem {dto.ProblemId} not found."
                });
            }

            return Ok(result);
        }


        // GET api/submissions/mine
        // Student views their own submissions
        [HttpGet("mine")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<List<SubmissionSummaryDto>>> GetMine()
        {
            return Ok(
                await _submissionService
                    .GetUserSubmissionsAsync(GetUserId())
            );
        }


        // GET api/submissions/all
        // Admin views all student submissions
        /*[HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<AdminSubmissionDto>>> GetAll()
        {
            return Ok(
                await _submissionService
                    .GetAllSubmissionsForAdminAsync()
            );
        }   */
        [HttpGet("all")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<List<AdminSubmissionDto>>> GetAll()
        {
            return Ok(
                await _submissionService.GetAllSubmissionsForAdminAsync()
            );
        }


        // GET api/submissions/5
        // User can view own submission.
        // Admin can view any submission.
        [HttpGet("{id:int}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<SubmissionResultDto>> GetById(int id)
        {
            var isAdmin = User.IsInRole("Admin");

            var result = await _submissionService.GetByIdAsync(
                id,
                GetUserId(),
                isAdmin
            );

            if (result is null)
            {
                return NotFound(new
                {
                    message = $"Submission {id} not found."
                });
            }

            return Ok(result);
        }


        private int GetUserId()
        {
            var idClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.Parse(idClaim!);
        }
    }
}
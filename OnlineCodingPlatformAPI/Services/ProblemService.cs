using Microsoft.EntityFrameworkCore;
using OnlineCodingAPI.Data;
using OnlineCodingAPI.Dtos;
using OnlineCodingAPI.Enums;
using OnlineCodingAPI.Models;

namespace OnlineCodingAPI.Services
{
    public class ProblemService
    {
        private readonly ApplicationDbContext _context;

        public ProblemService(ApplicationDbContext context)
        {
            _context = context;
        }


        // ============================================================
        // GET ALL PROBLEMS
        // ============================================================

        public async Task<List<ProblemSummaryDto>> GetAllAsync(
            string? difficulty,
            int? userId)
        {
            var query =
                _context.Problems.AsQueryable();

            // Difficulty filter
            if (!string.IsNullOrWhiteSpace(difficulty) &&
                Enum.TryParse<DifficultyLevel>(
                    difficulty,
                    true,
                    out var parsedDifficulty))
            {
                query = query.Where(
                    p => p.Difficulty == parsedDifficulty
                );
            }


            // First get problems
            var problems =
                await query
                    .OrderBy(p => p.Id)
                    .Select(p => new ProblemSummaryDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Difficulty =
                            p.Difficulty.ToString(),
                        CreatedAt = p.CreatedAt,

                        // Default
                        Status = "Not Started"
                    })
                    .ToListAsync();


            // If nobody is logged in,
            // all problems remain Not Started
            if (!userId.HasValue)
            {
                return problems;
            }


            // Get progress records for this student
            var progressRecords =
                await _context.ProgressRecords
                    .Where(p =>
                        p.UserId == userId.Value)
                    .Select(p => new
                    {
                        p.ProblemId,
                        p.Status
                    })
                    .ToListAsync();


            // Convert progress into lookup
            var progressLookup =
                progressRecords

                    .GroupBy(p => p.ProblemId)
                    .ToDictionary(
                        g => g.Key,
                        g => g
                            .OrderByDescending(x =>
                                x.Status ==
                                ProgressStatus.Solved)
                            .First()
                            .Status
                    );

            // Set status for every problem
            foreach (var problem in problems)
            {
                if (!progressLookup.TryGetValue(
                        problem.Id,
                        out var progressStatus))
                {
                    problem.Status = "Not Started";
                }
                else if (
                    progressStatus ==
                    ProgressStatus.Solved)
                {
                    problem.Status = "Completed";
                }
                else
                {
                    problem.Status = "Pending";
                }
            }


            return problems;
        }


        // ============================================================
        // GET PROBLEM BY ID
        // ============================================================

        public async Task<ProblemDetailDto?> GetByIdAsync(
            int id,
            int? userId)
        {
            var problem =
                await _context.Problems
                    .Include(p => p.TestCases)
                    .FirstOrDefaultAsync(
                        p => p.Id == id
                    );

            if (problem is null)
            {
                return null;
            }


            // Default status
            var status = "Not Started";


            // If logged-in user exists,
            // check their progress
            if (userId.HasValue)
            {
                var progress =
                    await _context.ProgressRecords
                        .FirstOrDefaultAsync(p =>
                            p.UserId == userId.Value &&
                            p.ProblemId == problem.Id
                        );

                if (progress is not null)
                {
                    if (progress.Status ==
                        ProgressStatus.Solved)
                    {
                        status = "Completed";
                    }
                    else
                    {
                        status = "Pending";
                    }
                }
            }


            return new ProblemDetailDto
            {
                Id = problem.Id,

                Title = problem.Title,

                Description =
                    problem.Description,

                Difficulty =
                    problem.Difficulty.ToString(),

                InputFormat =
                    problem.InputFormat,

                OutputFormat =
                    problem.OutputFormat,

                Constraints =
                    problem.Constraints,

                SampleInput =
                    problem.SampleInput,

                SampleOutput =
                    problem.SampleOutput,

                TimeLimitMs =
                    problem.TimeLimitMs,

                MemoryLimitKb =
                    problem.MemoryLimitKb,

                Status = status,

                // Only expose sample test cases
                // Hidden test cases stay server-side
                SampleTestCases =
                    problem.TestCases
                        .Where(tc => tc.IsSample)
                        .Select(tc =>
                            new SampleTestCaseDto
                            {
                                Input = tc.Input,
                                ExpectedOutput =
                                    tc.ExpectedOutput
                            })
                        .ToList()
            };
        }


        // ============================================================
        // GET ENTITY WITH TEST CASES
        // Used internally by SubmissionService
        // ============================================================

        public async Task<Problem?>
            GetEntityWithTestCasesAsync(int id)
        {
            return await _context.Problems
                .Include(p => p.TestCases)
                .FirstOrDefaultAsync(
                    p => p.Id == id
                );
        }


        // ============================================================
        // CREATE PROBLEM
        // ============================================================

        public async Task<Problem> CreateAsync(
            CreateProblemDto dto)
        {
            var problem = new Problem
            {
                Title = dto.Title,

                Description =
                    dto.Description,

                Difficulty =
                    dto.Difficulty,

                InputFormat =
                    dto.InputFormat,

                OutputFormat =
                    dto.OutputFormat,

                Constraints =
                    dto.Constraints,

                SampleInput =
                    dto.SampleInput,

                SampleOutput =
                    dto.SampleOutput,

                TimeLimitMs =
                    dto.TimeLimitMs,

                MemoryLimitKb =
                    dto.MemoryLimitKb,

                CreatedAt =
                    DateTime.UtcNow,

                TestCases =
                    dto.TestCases
                        .Select(tc => new TestCase
                        {
                            Input = tc.Input,

                            ExpectedOutput =
                                tc.ExpectedOutput,

                            IsSample =
                                tc.IsSample,

                            IsHidden =
                                tc.IsHidden
                        })
                        .ToList()
            };


            _context.Problems.Add(problem);

            await _context.SaveChangesAsync();

            return problem;
        }


        // ============================================================
        // UPDATE PROBLEM
        // ============================================================

        public async Task<bool> UpdateAsync(
            int id,
            UpdateProblemDto dto)
        {
            var problem =
                await _context.Problems
                    .Include(p => p.TestCases)
                    .FirstOrDefaultAsync(
                        p => p.Id == id
                    );

            if (problem is null)
            {
                return false;
            }


            problem.Title =
                dto.Title;

            problem.Description =
                dto.Description;

            problem.Difficulty =
                dto.Difficulty;

            problem.InputFormat =
                dto.InputFormat;

            problem.OutputFormat =
                dto.OutputFormat;

            problem.Constraints =
                dto.Constraints;

            problem.SampleInput =
                dto.SampleInput;

            problem.SampleOutput =
                dto.SampleOutput;

            problem.TimeLimitMs =
                dto.TimeLimitMs;

            problem.MemoryLimitKb =
                dto.MemoryLimitKb;


            // Replace test cases
            _context.TestCases
                .RemoveRange(problem.TestCases);


            problem.TestCases =
                dto.TestCases
                    .Select(tc => new TestCase
                    {
                        Input = tc.Input,

                        ExpectedOutput =
                            tc.ExpectedOutput,

                        IsSample =
                            tc.IsSample,

                        IsHidden =
                            tc.IsHidden
                    })
                    .ToList();


            await _context.SaveChangesAsync();

            return true;
        }


        // ============================================================
        // DELETE PROBLEM
        // ============================================================

        public async Task<bool> DeleteAsync(
            int id)
        {
            var problem =
                await _context.Problems
                    .FindAsync(id);

            if (problem is null)
            {
                return false;
            }


            _context.Problems.Remove(problem);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}





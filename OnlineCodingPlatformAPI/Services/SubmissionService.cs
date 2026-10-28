using Microsoft.EntityFrameworkCore;
using OnlineCodingAPI.Data;
using OnlineCodingAPI.Dtos;
using OnlineCodingAPI.Enums;
using OnlineCodingAPI.Models;




namespace OnlineCodingAPI.Services
{
    public class SubmissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ProblemService _problemService;
        private readonly IJudge0Service _judge0Service;
        private readonly ILogger<SubmissionService> _logger;

        public SubmissionService(
            ApplicationDbContext context,
            ProblemService problemService,
            IJudge0Service judge0Service,
            ILogger<SubmissionService> logger)
        {
            _context = context;
            _problemService = problemService;
            _judge0Service = judge0Service;
            _logger = logger;
        }


        // =========================================================
        // STUDENT SUBMITS CODE
        // =========================================================

        public async Task<SubmissionResultDto?> SubmitAsync(
            int userId,
            CreateSubmissionDto dto)
        {
            var problem =
                await _problemService
                    .GetEntityWithTestCasesAsync(
                        dto.ProblemId);

            if (problem is null)
                return null;

            var submission = new Submission
            {
                UserId = userId,
                ProblemId = problem.Id,
                Code = dto.Code,
                LanguageId = dto.LanguageId,
                Status = SubmissionStatus.Judging,
                SubmittedAt = DateTime.UtcNow
            };

            _context.Submissions.Add(submission);

            await _context.SaveChangesAsync();


            /*
             * Use TestCases if available.
             *
             * Otherwise use SampleInput and SampleOutput
             * as a fallback test case.
             */
            var testCases = problem.TestCases.Any()
                ? problem.TestCases
                : new List<TestCase>
                {
                    new TestCase
                    {
                        Input = problem.SampleInput,
                        ExpectedOutput = problem.SampleOutput,
                        IsSample = true
                    }
                };


            var finalStatus =
                SubmissionStatus.Accepted;

            string? lastOutput = null;
            string? errorMessage = null;

            double maxTime = 0;
            int maxMemory = 0;


            /*
             * Execute each test case.
             */
            foreach (var testCase in testCases)
            {
                Judge0SubmissionResult result;

                try
                {
                    result =
                        await _judge0Service.ExecuteAsync(
                            submission.Code,
                            submission.LanguageId,
                            testCase.Input,
                            testCase.ExpectedOutput,
                            problem.TimeLimitMs,
                            problem.MemoryLimitKb);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Code execution failed for submission {SubmissionId}",
                        submission.Id);

                    finalStatus =
                        SubmissionStatus.InternalError;

                    errorMessage =
                        "Code execution service unavailable. Please try again later.";

                    break;
                }


                lastOutput = result.Stdout;


                if (result.Time.HasValue)
                {
                    maxTime =
                        Math.Max(
                            maxTime,
                            result.Time.Value);
                }


                if (result.Memory.HasValue)
                {
                    maxMemory =
                        Math.Max(
                            maxMemory,
                            result.Memory.Value);
                }


                /*
                 * First check whether the code itself
                 * compiled/executed successfully.
                 */
                if (result.StatusId != 3)
                {
                    finalStatus =
                        MapJudge0Status(
                            result.StatusId);

                    errorMessage =
                        result.Stderr
                        ?? result.CompileOutput
                        ?? result.Message;

                    break;
                }


                /*
                 * Code executed successfully.
                 *
                 * Now compare actual output with
                 * expected output.
                 */
                if (!OutputsMatch(
                        result.Stdout,
                        testCase.ExpectedOutput))
                {
                    finalStatus =
                        SubmissionStatus.WrongAnswer;

                    errorMessage =
                        "Output does not match the expected output.";

                    break;
                }


                /*
                 * Current test case passed.
                 */
                finalStatus =
                    SubmissionStatus.Accepted;
            }


            /*
             * Save final submission result.
             */
            submission.Status =
                finalStatus;

            submission.Output =
                lastOutput;

            submission.ErrorMessage =
                errorMessage;

            submission.ExecutionTimeMs =
                maxTime;

            submission.MemoryUsedKb =
                maxMemory;

            await _context.SaveChangesAsync();


            /*
             * Update user progress.
             */
            await UpdateProgressAsync(
                userId,
                problem.Id,
                finalStatus);


            /*
             * Return complete submission details.
             *
             * IMPORTANT:
             * Code is now returned to the frontend.
             */
            return new SubmissionResultDto
            {
                Id =
                    submission.Id,

                ProblemId =
                    problem.Id,

                ProblemTitle =
                    problem.Title,

                Code =
                    submission.Code,

                Status =
                    submission.Status.ToString(),

                Output =
                    submission.Output,

                ErrorMessage =
                    submission.ErrorMessage,

                ExecutionTimeMs =
                    submission.ExecutionTimeMs,

                MemoryUsedKb =
                    submission.MemoryUsedKb,

                SubmittedAt =
                    submission.SubmittedAt
            };
        }


        // =========================================================
        // STUDENT - VIEW OWN SUBMISSION HISTORY
        // =========================================================

        public async Task<List<SubmissionSummaryDto>>
            GetUserSubmissionsAsync(
                int userId)
        {
            return await _context.Submissions
                .Include(s => s.Problem)
                .Where(s => s.UserId == userId)
                .OrderByDescending(
                    s => s.SubmittedAt)
                .Select(s =>
                    new SubmissionSummaryDto
                    {
                        Id =
                            s.Id,

                        ProblemId =
                            s.ProblemId,

                        ProblemTitle =
                            s.Problem!.Title,

                        LanguageId =
                            s.LanguageId,

                        Status =
                            s.Status.ToString(),

                        SubmittedAt =
                            s.SubmittedAt
                    })
                .ToListAsync();
        }


        // =========================================================
        // ADMIN - VIEW ALL STUDENT SUBMISSIONS
        // =========================================================

        public async Task<List<AdminSubmissionDto>>
            GetAllSubmissionsForAdminAsync()
        {
            return await _context.Submissions
                .Include(s => s.User)
                .Include(s => s.Problem)

                // Only students
                .Where(s =>
                    s.User!.Role == UserRole.User)

                // Latest submissions first
                .OrderByDescending(
                    s => s.SubmittedAt)

                .Select(s =>
                    new AdminSubmissionDto
                    {
                        Id =
                            s.Id,

                        UserId =
                            s.UserId,

                        Username =
                            s.User!.Username,

                        ProblemId =
                            s.ProblemId,

                        ProblemTitle =
                            s.Problem!.Title,

                        LanguageId =
                            s.LanguageId,

                        Code =
                            s.Code,

                        Status =
                            s.Status.ToString(),

                        Output =
                            s.Output,

                        ErrorMessage =
                            s.ErrorMessage,

                        ExecutionTimeMs =
                            s.ExecutionTimeMs,

                        MemoryUsedKb =
                            s.MemoryUsedKb,

                        SubmittedAt =
                            s.SubmittedAt
                    })
                .ToListAsync();
        }


        // =========================================================
        // VIEW ONE SUBMISSION
        //
        // Student -> only own submission
        // Admin   -> any student's submission
        // =========================================================

        public async Task<SubmissionResultDto?>
            GetByIdAsync(
                int submissionId,
                int userId,
                bool isAdmin)
        {
            var submission =
                await _context.Submissions
                    .Include(s => s.Problem)
                    .FirstOrDefaultAsync(
                        s =>
                            s.Id ==
                            submissionId);


            if (submission is null)
                return null;


            /*
             * Normal users can see only their own
             * submissions.
             *
             * Admin can see any submission.
             */
            if (!isAdmin &&
                submission.UserId != userId)
            {
                return null;
            }


            return new SubmissionResultDto
            {
                Id =
                    submission.Id,

                ProblemId =
                    submission.ProblemId,

                ProblemTitle =
                    submission.Problem!.Title,

                // IMPORTANT:
                // Return saved code
                Code =
                    submission.Code,

                Status =
                    submission.Status.ToString(),

                Output =
                    submission.Output,

                ErrorMessage =
                    submission.ErrorMessage,

                ExecutionTimeMs =
                    submission.ExecutionTimeMs,

                MemoryUsedKb =
                    submission.MemoryUsedKb,

                SubmittedAt =
                    submission.SubmittedAt
            };
        }


        // =========================================================
        // UPDATE PROGRESS
        // =========================================================

        private async Task UpdateProgressAsync(
            int userId,
            int problemId,
            SubmissionStatus status)
        {
            var progress =
                await _context.ProgressRecords
                    .FirstOrDefaultAsync(
                        p =>
                            p.UserId == userId &&
                            p.ProblemId == problemId);


            if (progress is null)
            {
                progress = new Progress
                {
                    UserId =
                        userId,

                    ProblemId =
                        problemId,

                    Status =
                        ProgressStatus.Attempted,

                    AttemptsCount =
                        0
                };

                _context.ProgressRecords.Add(
                    progress);
            }


            progress.AttemptsCount += 1;

            progress.LastAttemptAt =
                DateTime.UtcNow;


            /*
             * Mark problem as solved only when
             * all test cases pass.
             */
            if (status ==
                    SubmissionStatus.Accepted &&
                progress.Status !=
                    ProgressStatus.Solved)
            {
                progress.Status =
                    ProgressStatus.Solved;

                progress.SolvedAt =
                    DateTime.UtcNow;
            }
            else if (
                progress.Status !=
                ProgressStatus.Solved)
            {
                progress.Status =
                    ProgressStatus.Attempted;
            }


            await _context.SaveChangesAsync();
        }


        // =========================================================
        // OUTPUT COMPARISON
        // =========================================================

        private static bool OutputsMatch(
            string? actual,
            string? expected)
        {
            if (actual is null ||
                expected is null)
            {
                return false;
            }


            var actualNormalized =
                NormalizeOutput(actual);

            var expectedNormalized =
                NormalizeOutput(expected);


            return actualNormalized ==
                   expectedNormalized;
        }


        private static string NormalizeOutput(
            string output)
        {
            return string.Join(
                " ",
                output
                    .Trim()
                    .Split(
                        new[]
                        {
                            ' ',
                            '\t',
                            '\r',
                            '\n'
                        },
                        StringSplitOptions
                            .RemoveEmptyEntries));
        }


        // =========================================================
        // STATUS MAPPING
        // =========================================================

        /*
         * 3 = execution successful
         * 5 = time limit exceeded
         * 6 = compile error
         * 7 = runtime error
         */
        private static SubmissionStatus
            MapJudge0Status(int statusId)
        {
            return statusId switch
            {
                3 =>
                    SubmissionStatus.Accepted,

                5 =>
                    SubmissionStatus.TimeLimitExceeded,

                6 =>
                    SubmissionStatus.CompileError,

                7 =>
                    SubmissionStatus.RuntimeError,

                _ =>
                    SubmissionStatus.InternalError
            };
        }
    }
}
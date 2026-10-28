using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OnlineCodingAPI.Services
{
    public class Judge0SubmissionResult
    {
        public string? Stdout { get; set; }
        public string? Stderr { get; set; }
        public string? CompileOutput { get; set; }
        public string? Message { get; set; }

        public double? Time { get; set; }
        public int? Memory { get; set; }

        // Internal status mapping:
        // 3 = Execution successful
        // 5 = Time limit exceeded
        // 6 = Compile error
        // 7 = Runtime error
        public int StatusId { get; set; }

        public string StatusDescription { get; set; } = string.Empty;
    }

    public interface IJudge0Service
    {
        Task<Judge0SubmissionResult> ExecuteAsync(
            string sourceCode,
            int languageId,
            string stdin,
            string expectedOutput,
            int timeLimitMs,
            int memoryLimitKb,
            CancellationToken cancellationToken = default);
    }

    public class Judge0Service : IJudge0Service
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Judge0Service> _logger;

        public Judge0Service(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<Judge0Service> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var baseUrl =
                _configuration["CodeExecution:BaseUrl"]
                ?? "https://api.onecompiler.com";

            _httpClient.BaseAddress = new Uri(baseUrl);

            var apiKey =
                _configuration["CodeExecution:ApiKey"];

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add(
                    "X-API-Key",
                    apiKey);
            }

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));
        }

        public async Task<Judge0SubmissionResult> ExecuteAsync(
            string sourceCode,
            int languageId,
            string stdin,
            string expectedOutput,
            int timeLimitMs,
            int memoryLimitKb,
            CancellationToken cancellationToken = default)
        {
            /*
             * Convert our application's languageId
             * into OneCompiler's language name
             * and appropriate file name.
             */
            var languageConfig =
                GetLanguageConfiguration(languageId);

            var language = languageConfig.Language;
            var fileName = languageConfig.FileName;

            _logger.LogInformation(
                "Executing code. LanguageId: {LanguageId}, Language: {Language}",
                languageId,
                language);

            /*
             * OneCompiler request
             */
            var requestBody = new
            {
                language = language,
                stdin = stdin,
                files = new[]
                {
                    new
                    {
                        name = fileName,
                        content = sourceCode
                    }
                }
            };

            var json =
                JsonSerializer.Serialize(requestBody);

            using var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.PostAsync(
                    "/v1/run",
                    content,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "OneCompiler request was cancelled.");

                return new Judge0SubmissionResult
                {
                    StatusId = 5,
                    StatusDescription = "Time Limit Exceeded",
                    Message = "Execution timed out."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while calling OneCompiler.");

                throw;
            }

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            /*
             * HTTP-level failure
             */
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "OneCompiler returned {StatusCode}: {Response}",
                    response.StatusCode,
                    responseBody);

                throw new HttpRequestException(
                    $"OneCompiler returned {(int)response.StatusCode}: {responseBody}");
            }

            try
            {
                using var document =
                    JsonDocument.Parse(responseBody);

                var root =
                    document.RootElement;

                var stdout =
                    GetString(root, "stdout");

                var stderr =
                    GetString(root, "stderr");

                var exception =
                    GetString(root, "exception");

                var executionTime =
                    GetDouble(root, "executionTime");

                var memoryUsed =
                    GetInt(root, "memoryUsed");

                /*
                 * ------------------------------------------------
                 * 1. Check exception returned by OneCompiler
                 * ------------------------------------------------
                 */
                if (!string.IsNullOrWhiteSpace(exception))
                {
                    var exceptionText =
                        exception.Trim();

                    /*
                     * Timeout
                     */
                    if (LooksLikeTimeout(exceptionText))
                    {
                        return new Judge0SubmissionResult
                        {
                            Stdout = stdout,
                            Stderr = stderr,
                            Message = exceptionText,
                            Time = executionTime,
                            Memory = memoryUsed,
                            StatusId = 5,
                            StatusDescription =
                                "Time Limit Exceeded"
                        };
                    }

                    /*
                     * Compile error
                     */
                    if (LooksLikeCompileError(exceptionText))
                    {
                        return new Judge0SubmissionResult
                        {
                            Stdout = stdout,
                            Stderr = stderr,
                            CompileOutput = exceptionText,
                            Message = exceptionText,
                            Time = executionTime,
                            Memory = memoryUsed,
                            StatusId = 6,
                            StatusDescription =
                                "Compile Error"
                        };
                    }

                    /*
                     * Otherwise runtime error
                     */
                    return new Judge0SubmissionResult
                    {
                        Stdout = stdout,
                        Stderr = stderr,
                        CompileOutput = null,
                        Message = exceptionText,
                        Time = executionTime,
                        Memory = memoryUsed,
                        StatusId = 7,
                        StatusDescription =
                            "Runtime Error"
                    };
                }

                /*
                 * ------------------------------------------------
                 * 2. Check stderr
                 * ------------------------------------------------
                 *
                 * Some compiler errors from OneCompiler
                 * can appear in stderr instead of exception.
                 */
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    var stderrText =
                        stderr.Trim();

                    /*
                     * Timeout
                     */
                    if (LooksLikeTimeout(stderrText))
                    {
                        return new Judge0SubmissionResult
                        {
                            Stdout = stdout,
                            Stderr = stderr,
                            Message = stderrText,
                            Time = executionTime,
                            Memory = memoryUsed,
                            StatusId = 5,
                            StatusDescription =
                                "Time Limit Exceeded"
                        };
                    }

                    /*
                     * Compile error
                     */
                    if (LooksLikeCompileError(stderrText))
                    {
                        return new Judge0SubmissionResult
                        {
                            Stdout = stdout,
                            Stderr = stderr,
                            CompileOutput = stderrText,
                            Message = stderrText,
                            Time = executionTime,
                            Memory = memoryUsed,
                            StatusId = 6,
                            StatusDescription =
                                "Compile Error"
                        };
                    }

                    /*
                     * Runtime error
                     */
                    return new Judge0SubmissionResult
                    {
                        Stdout = stdout,
                        Stderr = stderr,
                        CompileOutput = null,
                        Message = stderrText,
                        Time = executionTime,
                        Memory = memoryUsed,
                        StatusId = 7,
                        StatusDescription =
                            "Runtime Error"
                    };
                }

                /*
                 * ------------------------------------------------
                 * 3. Successful execution
                 * ------------------------------------------------
                 *
                 * SubmissionService will compare stdout
                 * with ExpectedOutput.
                 *
                 * If equal     -> Accepted
                 * If different -> Wrong Answer
                 */
                return new Judge0SubmissionResult
                {
                    Stdout = stdout,
                    Stderr = null,
                    CompileOutput = null,
                    Message = null,
                    Time = executionTime,
                    Memory = memoryUsed,
                    StatusId = 3,
                    StatusDescription =
                        "Execution Successful"
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Invalid response received from OneCompiler: {Response}",
                    responseBody);

                throw new InvalidOperationException(
                    "Invalid response received from code execution service.");
            }
        }


        // ============================================================
        // LANGUAGE CONFIGURATION
        // ============================================================

        private static LanguageConfiguration
            GetLanguageConfiguration(int languageId)
        {
            return languageId switch
            {
                // Java
                1 => new LanguageConfiguration
                {
                    Language = "java",
                    FileName = "Main.java"
                },

                // Python
                2 => new LanguageConfiguration
                {
                    Language = "python",
                    FileName = "main.py"
                },

                // C
                3 => new LanguageConfiguration
                {
                    Language = "c",
                    FileName = "main.c"
                },

                // C++
                4 => new LanguageConfiguration
                {
                    Language = "cpp",
                    FileName = "main.cpp"
                },

                // C#
                5 => new LanguageConfiguration
                {
                    Language = "csharp",
                    FileName = "Program.cs"
                },

                // JavaScript
                6 => new LanguageConfiguration
                {
                    Language = "javascript",
                    FileName = "main.js"
                },

                // Go
                7 => new LanguageConfiguration
                {
                    Language = "go",
                    FileName = "main.go"
                },

                // Kotlin
                8 => new LanguageConfiguration
                {
                    Language = "kotlin",
                    FileName = "Main.kt"
                },

                _ => throw new ArgumentException(
                    $"Unsupported languageId: {languageId}")
            };
        }


        // ============================================================
        // COMPILE ERROR DETECTION
        // ============================================================

        private static bool LooksLikeCompileError(
            string text)
        {
            var lowerText =
                text.ToLowerInvariant();

            return
                lowerText.Contains("error:")
                || lowerText.Contains("1 error")
                || lowerText.Contains("errors")
                || lowerText.Contains("cannot find symbol")
                || lowerText.Contains("';' expected")
                || lowerText.Contains("illegal start")
                || lowerText.Contains(
                    "reached end of file while parsing")
                || lowerText.Contains(
                    "class, interface, enum, or record expected")
                || lowerText.Contains("not a statement")
                || lowerText.Contains("expected")
                || lowerText.Contains("syntax error")
                || lowerText.Contains("compilation failed")
                || lowerText.Contains("compilation error");
        }


        // ============================================================
        // TIMEOUT DETECTION
        // ============================================================

        private static bool LooksLikeTimeout(
            string text)
        {
            var lowerText =
                text.ToLowerInvariant();

            return
                lowerText.Contains("timeout")
                || lowerText.Contains("timed out")
                || lowerText.Contains(
                    "time limit exceeded")
                || lowerText.Contains(
                    "execution timed out");
        }


        // ============================================================
        // JSON HELPERS
        // ============================================================

        private static string? GetString(
            JsonElement root,
            string propertyName)
        {
            if (!root.TryGetProperty(
                    propertyName,
                    out var element))
            {
                return null;
            }

            if (element.ValueKind ==
                JsonValueKind.Null)
            {
                return null;
            }

            if (element.ValueKind ==
                JsonValueKind.String)
            {
                return element.GetString();
            }

            return element.ToString();
        }


        private static double? GetDouble(
            JsonElement root,
            string propertyName)
        {
            if (!root.TryGetProperty(
                    propertyName,
                    out var element))
            {
                return null;
            }

            if (element.ValueKind ==
                JsonValueKind.Null)
            {
                return null;
            }

            if (element.ValueKind ==
                JsonValueKind.Number)
            {
                return element.GetDouble();
            }

            if (element.ValueKind ==
                    JsonValueKind.String &&
                double.TryParse(
                    element.GetString(),
                    out var value))
            {
                return value;
            }

            return null;
        }


        private static int? GetInt(
            JsonElement root,
            string propertyName)
        {
            if (!root.TryGetProperty(
                    propertyName,
                    out var element))
            {
                return null;
            }

            if (element.ValueKind ==
                JsonValueKind.Null)
            {
                return null;
            }

            if (element.ValueKind ==
                JsonValueKind.Number)
            {
                return element.GetInt32();
            }

            if (element.ValueKind ==
                    JsonValueKind.String &&
                int.TryParse(
                    element.GetString(),
                    out var value))
            {
                return value;
            }

            return null;
        }


        // ============================================================
        // LANGUAGE CONFIGURATION CLASS
        // ============================================================

        private class LanguageConfiguration
        {
            public string Language { get; set; } = string.Empty;

            public string FileName { get; set; } = string.Empty;
        }
    }
}


using System.Net;
using System.Text.Json;

namespace ECommerceBackend.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        // 這是 Middleware 的進入點
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // 1. 嘗試讓請求通過 (去執行 Controller 的邏輯)
                await _next(context);
            }
            catch (Exception ex)
            {
                // 2. 如果途中發生任何錯誤 (Exception)，就會掉到這裡
                _logger.LogError(ex, ex.Message); // 📝 寫日記：把錯誤記下來 (Console 或檔案)

                // 3. 處理錯誤並回傳 JSON
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // 判斷環境：如果是 "開發模式 (Development)"，就顯示詳細錯誤讓我們除錯
            // 如果是 "正式環境 (Production)"，就只顯示 "Internal Server Error" 保護系統
            var response = _env.IsDevelopment()
                ? new ErrorDetails(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                : new ErrorDetails(context.Response.StatusCode, "Internal Server Error");

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }

    // 定義回傳給前端的錯誤格式
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string? Details { get; set; }

        public ErrorDetails(int statusCode, string message, string? details = null)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }
    }
}
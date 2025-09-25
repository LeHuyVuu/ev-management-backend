using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using BrandService.Models; // nơi chứa ApiResponse<T>

namespace Application.ExceptionHandler
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception tại {Path}. Error: {Error}",
                    context.Request.Path,
                    ex.Message
                );

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var (statusCode, response) = exception switch
            {
                ValidationException ex => (
                    StatusCodes.Status400BadRequest,
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message, "VALIDATION_ERROR")
                ),

                NotFoundException ex => (
                    StatusCodes.Status404NotFound,
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message, "NOT_FOUND")
                ),

                ArgumentNullException ex => (
                    StatusCodes.Status400BadRequest,
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message, "ARGUMENT_NULL")
                ),

                InvalidOperationException ex => (
                    StatusCodes.Status400BadRequest,
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message, "INVALID_OPERATION")
                ),

                BadRequestException ex => (
                    StatusCodes.Status400BadRequest,
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message, "BAD_REQUEST")
                ),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    CreateInternalServerError(exception)
                )
            };

            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }

        private ApiResponse<object> CreateInternalServerError(Exception ex)
        {
            var message = _env.IsDevelopment()
                ? $"Internal Server Error: {ex.Message}"
                : "Đã xảy ra lỗi trong quá trình xử lý";

            return ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, message, "INTERNAL_ERROR");
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CustomerAppsApi.Middleware
{
	public class ResponseLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger _logger;
		private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

		public ResponseLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
		{
			_next = next;
			_logger = loggerFactory.CreateLogger<ResponseLoggingMiddleware>();
			_recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
		}

		public async Task Invoke(HttpContext context)
		{
			var watcher = Stopwatch.StartNew();
			await LogResponse(context, watcher);
		}

		private async Task LogResponse(HttpContext context, Stopwatch watcher)
		{
			var originalBodyStream = context.Response.Body;

			await using var responseBody = _recyclableMemoryStreamManager.GetStream();
			context.Response.Body = responseBody;

			await _next(context);

			context.Response.Body.Seek(0, SeekOrigin.Begin);
			var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
			context.Response.Body.Seek(0, SeekOrigin.Begin);

			watcher.Stop();

			_logger.LogInformation("Http Response Information: " +
								   "Schema: {RequestScheme} " +
								   "Code: {Code}" +
								   "Host: {RequestHost} " +
								   "Path: {RequestPath} " +
								   "QueryString: {RequestQueryString} " +
								   "Response Body: {RequestBody} | " +
								   "Elapsed: {RequestTotalMilliseconds}ms",
								   context.Request.Scheme,
								   context.Response.StatusCode,
								   context.Request.Host,
								   context.Request.Path,
								   context.Request.QueryString,
								   text,
								   watcher.Elapsed.TotalMilliseconds);

			await responseBody.CopyToAsync(originalBodyStream);
		}
	}
}

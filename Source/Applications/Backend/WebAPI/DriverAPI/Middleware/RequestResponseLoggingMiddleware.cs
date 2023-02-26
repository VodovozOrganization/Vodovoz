using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DriverAPI.Middleware
{
	public class RequestResponseLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger _logger;
		private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

		public RequestResponseLoggingMiddleware(RequestDelegate next,
												ILoggerFactory loggerFactory)
		{
			_next = next;
			_logger = loggerFactory
					  .CreateLogger<RequestResponseLoggingMiddleware>();
			_recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
		}

		public async Task Invoke(HttpContext context)
		{
			Stopwatch _watcher = Stopwatch.StartNew();
			await LogRequest(context);
			await LogResponse(context, _watcher);
		}

		private async Task LogRequest(HttpContext context)
		{
			context.Request.EnableBuffering();
			await using var requestStream = _recyclableMemoryStreamManager.GetStream();
			await context.Request.Body.CopyToAsync(requestStream);
			_logger.LogInformation("Http Request Information: " +
								   "Schema: {RequestScheme} " +
								   "Host: {RequestHost} " +
								   "Path: {RequestPath} " +
								   "QueryString: {RequestQueryString} " +
								   "Request Body: {RequestBody}",
								   context.Request.Scheme,
								   context.Request.Host,
								   context.Request.Path,
								   context.Request.QueryString,
								   ReadStreamInChunks(requestStream));

			context.Request.Body.Position = 0;
		}

		private static string ReadStreamInChunks(Stream stream)
		{
			const int readChunkBufferLength = 4096;

			stream.Seek(0, SeekOrigin.Begin);

			using var textWriter = new StringWriter();
			using var reader = new StreamReader(stream);

			var readChunk = new char[readChunkBufferLength];
			int readChunkLength;

			do
			{
				readChunkLength = reader.ReadBlock(readChunk,
												   0,
												   readChunkBufferLength);
				textWriter.Write(readChunk, 0, readChunkLength);
			}
			while(readChunkLength > 0);

			return textWriter.ToString();
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
								   "Host: {RequestHost} " +
								   "Path: {RequestPath} " +
								   "QueryString: {RequestQueryString} " +
								   "Response Body: {RequestBody} | " +
								   "Elapsed: {RequestTotalMilliseconds}ms",
								   context.Request.Scheme,
								   context.Request.Host,
								   context.Request.Path,
								   context.Request.QueryString,
								   text,
								   watcher.Elapsed.TotalMilliseconds);

			await responseBody.CopyToAsync(originalBodyStream);
		}
	}
}

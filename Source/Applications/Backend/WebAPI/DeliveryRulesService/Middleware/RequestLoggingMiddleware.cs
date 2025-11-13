using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DeliveryRulesService.Middleware
{
	internal class RequestLoggingMiddleware
	{
		private readonly ILogger _logger;
		private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
		private readonly RequestDelegate _next;

		public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
			_recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			await LogRequest(context);
		}

		private async Task LogRequest(HttpContext context)
		{
			context.Request.EnableBuffering();

			await using var requestStream = _recyclableMemoryStreamManager.GetStream();

			var userAgent = string.Empty;

			if(context.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var userAgentHeader))
			{
				userAgent = userAgentHeader;
			}

			await context.Request.Body.CopyToAsync(requestStream);
			_logger.LogInformation("Http Request Information: " +
								   "Schema: {RequestScheme} " +
								   "User-Agent: {UserAgent} " +
								   "Host: {RequestHost} " +
								   "Path: {RequestPath} " +
								   "QueryString: {RequestQueryString} " +
								   "Request Body: {RequestBody}",
								   context.Request.Scheme,
								   userAgent,
								   context.Request.Host,
								   context.Request.Path,
								   context.Request.QueryString,
								   ReadStreamInChunks(requestStream));

			context.Request.Body.Position = 0;

			await _next?.Invoke(context);
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
				readChunkLength = reader.ReadBlock(readChunk, 0, readChunkBufferLength);
				textWriter.Write(readChunk, 0, readChunkLength);
			}
			while(readChunkLength > 0);

			return textWriter.ToString();
		}		
	}
}

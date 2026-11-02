using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryRulesService.Middleware
{
	internal class RequestLoggingMiddleware
	{
		private readonly IOptionsMonitor<ResponseLoggingOptions> _responseLoggingOptions;
		private readonly ILogger _logger;
		private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
		private readonly RequestDelegate _next;

		public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory,
			IOptionsMonitor<ResponseLoggingOptions> responseLoggingOptions)
		{
			_logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
			_recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
			_next = next;
			_responseLoggingOptions = responseLoggingOptions;
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
								   "RequestId: {RequestId} " +
								   "Request Body: {RequestBody}",
								   context.Request.Scheme,
								   userAgent,
								   context.Request.Host,
								   context.Request.Path,
								   context.Request.QueryString,
								   context.TraceIdentifier,
								   ReadStreamInChunks(requestStream));

			context.Request.Body.Position = 0;

			if(_responseLoggingOptions.CurrentValue.Paths?.Any(path =>
				!string.IsNullOrWhiteSpace(path)
				&& string.Equals(context.Request.Path.Value?.TrimEnd('/'), path.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)) == true)
			{
				await LogResponse(context);
				return;
			}

			await _next.Invoke(context);
		}

		private async Task LogResponse(HttpContext context)
		{
			var originalBody = context.Response.Body;
			await using var responseStream = _recyclableMemoryStreamManager.GetStream();
			context.Response.Body = responseStream;

			try
			{
				await _next.Invoke(context);
				var responseBody = ReadStreamInChunks(responseStream);
				responseStream.Position = 0;
				await responseStream.CopyToAsync(originalBody);

				_logger.LogInformation(
					"Http Response Information: RequestId: {RequestId} Path: {RequestPath} " +
					"StatusCode: {StatusCode} Response Body: {ResponseBody}",
					context.TraceIdentifier,
					context.Request.Path,
					context.Response.StatusCode,
					responseBody);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка обработки или передачи ответа: RequestId: {RequestId} Path: {RequestPath}",
					context.TraceIdentifier, context.Request.Path);
				throw;
			}
			finally
			{
				context.Response.Body = originalBody;
			}
		}

		private static string ReadStreamInChunks(Stream stream)
		{
			const int readChunkBufferLength = 4096;

			stream.Seek(0, SeekOrigin.Begin);

			using var textWriter = new StringWriter();
			using var reader = new StreamReader(stream, Encoding.UTF8, true, readChunkBufferLength, leaveOpen: true);

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

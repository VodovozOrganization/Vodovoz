using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mango.Service
{
	public class PerformanceMiddleware
	{
		private readonly ILogger _logger;
		private readonly RequestDelegate _next;

		public PerformanceMiddleware(ILoggerFactory loggerFactory, RequestDelegate requestDelegate)
		{
			if(loggerFactory is null)
			{
				throw new ArgumentNullException(nameof(loggerFactory));
			}

			_logger = loggerFactory.CreateLogger("Requests");
			_next = requestDelegate;
		}

		public Task Invoke(HttpContext httpContext)
		{
			var watch = new Stopwatch();
			watch.Start();

			var httpRequest = httpContext.Request;
			var requestString = $"[{httpRequest.Method}]{httpRequest.Path}?{httpRequest.QueryString}";
			string json = null;
			StringValues values;
			if(httpRequest.Form.TryGetValue("json", out values))
				json = values.ToString();

			_logger.LogTrace("REQUEST: {RequestString} JSON:{Json}", requestString, json);

			var nextTask = _next.Invoke(httpContext);
			nextTask.ContinueWith(t =>
			{
				var time = watch.ElapsedMilliseconds;
				var httpRequest = httpContext.Request;
				var requestString = $"[{httpRequest.Method}]{httpRequest.Path}?{httpRequest.QueryString}";
				string json = null;
				StringValues values;
				if(httpRequest.Form.TryGetValue("json", out values))
					json = values.ToString();

				if(t.Status == TaskStatus.RanToCompletion)
				{
					_logger.LogInformation("Time: {Time}ms {RequestString} JSON:{Json}", time, requestString, json);
				}
				else
				{
					_logger.LogError(t.Exception?.InnerException, "Time: {Time}ms [{Status}] - {RequestString} JSON:{Json}",
						time, t.Status, requestString, json);
				}
			});
			return nextTask;
		}
	}
}

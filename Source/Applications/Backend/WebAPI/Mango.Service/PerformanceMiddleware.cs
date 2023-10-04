using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NLog;

namespace Mango.Service
{
	public class PerformanceMiddleware
	{
		private static Logger logger = LogManager.GetLogger("Requests");
		private readonly RequestDelegate _next;

		public PerformanceMiddleware(RequestDelegate requestDelegate)
		{
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

			logger.Trace($"REQUEST: {requestString} JSON:{json}");

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
					logger.Info($"Time: {time}ms {requestString} JSON:{json}");
				}
				else
				{
					logger.Error(t.Exception?.InnerException, $"Time: {time}ms [{t.Status}] - {requestString} JSON:{json}");
				}
			});
			return nextTask;
		}
	}
}
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace VodovozHealthCheck.ResponseWriter
{
	public class JsonResponseWriter : IResponseWriter
	{
		private readonly ILogger<JsonResponseWriter> _logger;

		public JsonResponseWriter(ILogger<JsonResponseWriter> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task WriteResponse(HttpContext context, HealthReport healthReport)
		{
			context.Response.ContentType = "application/json; charset=utf-8";

			var options = new JsonWriterOptions
			{
				Indented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};

			using var memoryStream = new MemoryStream();
			using(var jsonWriter = new Utf8JsonWriter(memoryStream, options))
			{
				jsonWriter.WriteStartObject();
				jsonWriter.WriteString("status", healthReport.Status.ToString());

				var firstEntry = healthReport.Entries.FirstOrDefault();
				jsonWriter.WriteString("description", firstEntry.Value.Description ?? "Нет описания");

				jsonWriter.WriteStartObject("data");

				foreach(var entry in healthReport.Entries)
				{
					var dataDict = entry.Value.Data;

					foreach(var item in dataDict)
					{
						jsonWriter.WritePropertyName(item.Key);
						JsonSerializer.Serialize(jsonWriter, item.Value, item.Value?.GetType() ?? typeof(object));
					}

					if(entry.Value.Exception is { } exception)
					{
						jsonWriter.WritePropertyName("exception");
						jsonWriter.WriteStringValue(exception.ToString());
					}
				}

				jsonWriter.WriteEndObject(); // конец data
				jsonWriter.WriteEndObject(); // конец корневого объекта
			}

			var responseBody = Encoding.UTF8.GetString(memoryStream.ToArray());

			if(healthReport.Status == HealthStatus.Healthy)
			{
				_logger.LogInformation(
					"Health check пройден успешно. Status: {Status}. Полный ответ:\n{ResponseBody}",
					healthReport.Status,
					responseBody
				);
			}
			else
			{
				_logger.LogWarning(
					"Health check НЕ ПРОЙДЕН! Status: {Status}. Полный ответ:\n{ResponseBody}",
					healthReport.Status,
					responseBody
				);
			}

			await context.Response.WriteAsync(responseBody);
		}
	}
}

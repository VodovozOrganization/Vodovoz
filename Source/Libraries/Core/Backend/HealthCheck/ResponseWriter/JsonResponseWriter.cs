using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace VodovozHealthCheck.ResponseWriter
{
	public class JsonResponseWriter : IResponseWriter
	{
		public Task WriteResponse(HttpContext context, HealthReport healthReport)
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
				jsonWriter.WriteString("description", healthReport.Entries.FirstOrDefault().Value.Description);

				jsonWriter.WriteStartObject("data");

				foreach(var healthReportEntry in healthReport.Entries)
				{
					var data = healthReportEntry.Value.Data;

					foreach(var item in data)
					{
						jsonWriter.WritePropertyName(item.Key);

						JsonSerializer.Serialize(jsonWriter, item.Value, item.Value?.GetType() ?? typeof(object));
					}

					if(healthReportEntry.Value.Exception is { } exception)
					{
						jsonWriter.WritePropertyName("exception");
						jsonWriter.WriteStringValue(exception.ToString());
					}
				}

				jsonWriter.WriteEndObject();

				jsonWriter.WriteEndObject();
			}

			return context.Response.WriteAsync(Encoding.UTF8.GetString(memoryStream.ToArray()));
		}
	}
}

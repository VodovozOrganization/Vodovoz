using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace VodovozHealthCheck.ResponseWriter
{
	public interface IResponseWriter
	{
		Task WriteResponse(HttpContext context, HealthReport healthReport);
	}
}

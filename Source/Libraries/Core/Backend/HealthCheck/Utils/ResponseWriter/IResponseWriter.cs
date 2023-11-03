using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;

namespace VodovozHealthCheck.Utils.ResponseWriter
{
	public interface IResponseWriter
	{
		Task WriteResponse(HttpContext context, HealthReport healthReport);
	}
}

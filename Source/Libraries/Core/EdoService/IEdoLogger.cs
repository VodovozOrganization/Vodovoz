using System.Net.Http;

namespace EdoService.Library
{
	public interface IEdoLogger
	{
		void LogError(HttpResponseMessage response);
	}
}

using System.Net.Http;

namespace EdoService
{
	public interface IEdoLogger
	{
		void LogError(HttpResponseMessage response);
	}
}

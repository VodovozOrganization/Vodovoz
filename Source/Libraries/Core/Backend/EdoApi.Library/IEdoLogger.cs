using System.Net.Http;

namespace EdoApi.Library
{
	public interface IEdoLogger
	{
		void LogError(HttpResponseMessage response);
	}
}

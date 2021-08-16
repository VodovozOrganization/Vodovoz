using System.Net.Http;

namespace ApiHelper
{
	public interface IApiClientProvider
	{
		HttpClient Client { get; }
	}
}
using System.Net.Http;

namespace ApiClientProvider
{
	public interface IApiClientProvider
	{
		HttpClient Client { get; }
	}
}
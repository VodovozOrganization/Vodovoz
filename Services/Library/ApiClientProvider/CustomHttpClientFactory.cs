using System.Net.Http;

namespace ApiClientProvider
{
	public class CustomHttpClientFactory : ICustomHttpClientFactory
	{
		private HttpClient _client;

		public HttpClient CreateClient()
		{
			if(_client == null)
			{
				_client = new HttpClient();
			}
			
			return _client;
		}
	}
}

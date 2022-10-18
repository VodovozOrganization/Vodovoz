using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ApiClientProvider
{
	public static class HttpClientExtension
	{
		public static void ConfigureHttpClient(this HttpClient client, Uri baseUri)
		{
			client.BaseAddress = baseUri;
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}
	}
}

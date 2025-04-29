using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GeoCoderApi.Client.Extensions
{
	public static class HttpClientExtensions
	{
		public static Task<HttpResponseMessage> GetAsync(
			this HttpClient httpClient,
			string requestUri,
			string parameterName,
			object parameterValue,
			CancellationToken cancellationToken) =>
				httpClient.GetAsync(requestUri.WithParameter(parameterName, parameterValue), cancellationToken);

		public static Task<HttpResponseMessage> GetAsync(
			this HttpClient httpClient,
			string requestUri,
			IDictionary<string, object> parameters,
			CancellationToken cancellationToken) =>
				httpClient.GetAsync(requestUri.WithParameters(parameters), cancellationToken);
	}

}

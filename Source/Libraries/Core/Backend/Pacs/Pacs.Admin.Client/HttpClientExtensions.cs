using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Pacs.Admin.Client
{
	public static class HttpClientExtensions
	{
		public static Task<HttpResponseMessage> PostAsJsonAsync<TValue>(
			this HttpClient client,
			string requestUri,
			TValue value,
			IReadOnlyDictionary<string, string> headers,
			JsonSerializerOptions options = null,
			CancellationToken cancellationToken = default)
		{
			if(client is null)
			{
				throw new ArgumentNullException(nameof(client));
			}

			var content = JsonContent.Create(
				value,
				mediaType: new MediaTypeHeaderValue("application/json"),
				options);

			foreach(var header in headers)
			{
				content.Headers.Remove(header.Key);
				content.Headers.Add(header.Key, header.Value);
			}

			return client.PostAsync(requestUri, content, cancellationToken);
		}

		public static Task<HttpResponseMessage> GetAsync(
			this HttpClient client,
			string requestUri,
			IReadOnlyDictionary<string, string> headers,
			CancellationToken cancellationToken = default)
		{
			if(client is null)
			{
				throw new ArgumentNullException(nameof(client));
			}

			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Get,
				RequestUri = new Uri(requestUri),
			};

			foreach(var header in headers)
			{
				request.Headers.Remove(header.Key);
				request.Headers.Add(header.Key, header.Value);
			}

			return client.SendAsync(request, cancellationToken);
		}
	}
}

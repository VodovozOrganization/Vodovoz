using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;

namespace TrueMarkApi.Client
{
	public class TrueMarkApiClient : ITrueMarkApiClient
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _trueMarkApiBaseUrl;
		private readonly string _trueMarkApiToken;

		public TrueMarkApiClient(IHttpClientFactory httpClientFactory, string trueMarkApiBaseUrl, string trueMarkApiToken)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_trueMarkApiBaseUrl = trueMarkApiBaseUrl ?? throw new ArgumentNullException(nameof(trueMarkApiBaseUrl));
			_trueMarkApiToken = trueMarkApiToken ?? throw new ArgumentNullException(nameof(trueMarkApiToken));
		}

		private HttpClient GetHttpClient()
		{
			var httpClient = _httpClientFactory.CreateClient(nameof(TrueMarkApiClient));
			httpClient.BaseAddress = new Uri(_trueMarkApiBaseUrl);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _trueMarkApiToken);

			return httpClient;
		}

		public async Task<TrueMarkRegistrationResultDto> GetParticipantRegistrationForWaterStatusAsync(string url, string inn,
			CancellationToken cancellationToken)
		{
			var urlWithParams = $"{url}?inn={inn}";

			using(var httpClient = GetHttpClient())
			{
				var response = await httpClient.GetAsync(urlWithParams, cancellationToken);
				var responseBody = await response.Content.ReadAsStreamAsync();
				var responseResult =
					await JsonSerializer.DeserializeAsync<TrueMarkRegistrationResultDto>(responseBody,
						cancellationToken: cancellationToken);

				return responseResult;
			}
		}

		public async Task<ProductInstancesInfoResponse> GetProductInstanceInfoAsync(IEnumerable<string> identificationCodes,
			CancellationToken cancellationToken)
		{
			var content = JsonSerializer.Serialize(identificationCodes.ToArray());
			var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

			var retryPolicy = Policy
				.Handle<HttpRequestException>()
				.Or<TimeoutException>()
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

			using(var httpClient = GetHttpClient())
			{
				var result = await retryPolicy.ExecuteAndCaptureAsync(
					async (innerCancellationToken) =>
					{
						var response = await httpClient.PostAsync("api/RequestProductInstanceInfo", httpContent, innerCancellationToken);
						var responseBody = await response.Content.ReadAsStreamAsync();
						var responseResult =
							await JsonSerializer.DeserializeAsync<ProductInstancesInfoResponse>(responseBody,
								cancellationToken: innerCancellationToken);
						
						return responseResult;
					},
					cancellationToken);

				return result.Result;
			}
		}
	}
}

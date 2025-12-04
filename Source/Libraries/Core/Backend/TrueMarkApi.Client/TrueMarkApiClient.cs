using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;

namespace TrueMarkApi.Client
{
	public class TrueMarkApiClient : ITrueMarkApiClient
	{
		private readonly HttpClient _httpClient;

		public TrueMarkApiClient(HttpClient httpClient)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		public async Task<TrueMarkRegistrationResultDto> GetParticipantRegistrationForWaterStatusAsync(string url, string inn, CancellationToken cancellationToken)
		{
			var urlWithParams = $"{url}?inn={inn}";
			var response = await _httpClient.GetAsync(urlWithParams, cancellationToken);
			var responseBody = await response.Content.ReadAsStreamAsync();
			var responseResult = await JsonSerializer.DeserializeAsync<TrueMarkRegistrationResultDto>(responseBody, cancellationToken: cancellationToken);

			return responseResult;
		}

		public async Task<ProductInstancesInfoResponse> GetProductInstanceInfoAsync(IEnumerable<string> identificationCodes, CancellationToken cancellationToken)
		{
			string content = JsonSerializer.Serialize(identificationCodes.ToArray());
			HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

			var retryPolicy = Policy
				.Handle<HttpRequestException>()
				.Or<TimeoutException>()
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

			var result = await retryPolicy.ExecuteAndCaptureAsync(
				async (innerCancellationToken) =>
				{
					var response = await _httpClient.PostAsync("api/RequestProductInstanceInfo", httpContent, innerCancellationToken);
					var responseBody = await response.Content.ReadAsStreamAsync();
					var responseResult = await JsonSerializer.DeserializeAsync<ProductInstancesInfoResponse>(responseBody, cancellationToken: innerCancellationToken);
					return responseResult;
				},
				cancellationToken);

			if(result.Outcome == OutcomeType.Failure)
			{
				throw result.FinalException;
			}

			return result.Result;
		}

		public async Task<string> SendIndividualAccountingWithdrawalDocument(string document, string inn, CancellationToken cancellationToken)
		{
			var sendDocumentRequest = new
			{
				Document = document,
				Inn = inn
			};

			string content = JsonSerializer.Serialize(sendDocumentRequest);
			HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync("api/SendIndividualAccountingWithdrawalDocument", httpContent, cancellationToken);
			response.EnsureSuccessStatusCode();

			var documentId = await response.Content.ReadAsStringAsync();

			return documentId;
		}
	}
}

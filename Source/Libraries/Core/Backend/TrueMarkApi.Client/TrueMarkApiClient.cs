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
using TrueMark.Contracts.Documents;
using TrueMark.Contracts.Responses;

namespace TrueMarkApi.Client
{
	public class TrueMarkApiClient : ITrueMarkApiClient
	{
		private const int _participantsCheckMaxCount = 100;

		private readonly HttpClient _httpClient;

		public TrueMarkApiClient(HttpClient httpClient)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		public int ParticipantsCheckMaxCount => _participantsCheckMaxCount;

		public async Task<TrueMarkRegistrationResultDto> GetParticipantRegistrationForWaterStatusAsync(string url, string inn, CancellationToken cancellationToken)
		{
			var urlWithParams = $"{url}?inn={inn}";
			var response = await _httpClient.GetAsync(urlWithParams, cancellationToken);
			var responseBody = await response.Content.ReadAsStreamAsync();
			var responseResult = await JsonSerializer.DeserializeAsync<TrueMarkRegistrationResultDto>(responseBody, cancellationToken: cancellationToken);

			return responseResult;
		}

		public async Task<IEnumerable<ParticipantRegistrationDto>> GetParticipantsRegistrations(IEnumerable<string> inns, CancellationToken cancellationToken)
		{
			var uniqueInns = inns.Distinct();
			if(uniqueInns.Count() > ParticipantsCheckMaxCount)
			{
				throw new ArgumentException($"The number of INNs cannot exceed {ParticipantsCheckMaxCount}.", nameof(inns));
			}

			string content = JsonSerializer.Serialize(uniqueInns);
			HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync("api/participants", httpContent, cancellationToken);
			response.EnsureSuccessStatusCode();

			var responseBody = await response.Content.ReadAsStreamAsync();
			var responseResult = await JsonSerializer.DeserializeAsync<IEnumerable<ParticipantRegistrationDto>>(responseBody, cancellationToken: cancellationToken);
			
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

		public async Task<TrueMarkDocumentInfo> GetDocumentInfo(Guid documentId, string inn, CancellationToken cancellationToken)
		{
			var urlWithParams = $"api/GetDocumentInfo?documentId={documentId}&inn={inn}";
			var response = await _httpClient.GetAsync(urlWithParams, cancellationToken);

			if(!response.IsSuccessStatusCode)
			{
				if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return new TrueMarkDocumentInfo
					{
						DocumentId = documentId,
						Status = TrueMarkDocumentStatus.NotFound,
						ErrorMessage = "Document not found"
					};
				}

				return new TrueMarkDocumentInfo
				{
					DocumentId = documentId,
					Status = TrueMarkDocumentStatus.Error,
					ErrorMessage = $"Error retrieving document info. Message: {response.ReasonPhrase} Status code: {response.StatusCode}"
				};
			}

			var responseBody = await response.Content.ReadAsStreamAsync();
			var createdDocumentInfo = await JsonSerializer.DeserializeAsync<CreatedDocumentInfoDto>(responseBody, cancellationToken: cancellationToken);

			if(createdDocumentInfo is null)
			{
				return new TrueMarkDocumentInfo
				{
					DocumentId = documentId,
					Status = TrueMarkDocumentStatus.Error,
					ErrorMessage = "Error deserializing document info response"
				};
			}
			
			return new TrueMarkDocumentInfo
			{
				DocumentId = documentId,
				Status =
					createdDocumentInfo.HasErrors
					? TrueMarkDocumentStatus.Error
					: TrueMarkDocumentStatus.Ok,
				ErrorMessage =
					createdDocumentInfo.HasErrors
					? string.Join("; ", createdDocumentInfo.Errors)
					: null
			};
		}
	}
}

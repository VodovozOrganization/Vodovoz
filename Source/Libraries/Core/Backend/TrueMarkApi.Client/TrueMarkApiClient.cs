using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

			return result.Result;
		}

		public async Task<string> SendIndividualAccountingWithdrawalDocument(string document, string inn, CancellationToken cancellationToken)
		{
			var sendDocumentRequest = new SendDocumentDataRequest
			{
				Document = document,
				Inn = inn
			};

			//(string Document, string Inn) documentData = (document, inn);
			string content = JsonSerializer.Serialize(sendDocumentRequest);

			HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync("api/SendIndividualAccountingWithdrawalDocument", httpContent, cancellationToken);

			var documentId = await response.Content.ReadAsStringAsync();

			if(!response.IsSuccessStatusCode)
			{
				throw new Exception(
					$"Ошибка при отправке документа вывода из оборота в Честный Знак. " +
					$"Документ: {document}. " +
					$"ИНН: {inn}. " +
					$"Код ошибки: {response.StatusCode}. " +
					$"Ответ: {documentId}");
			}

			return documentId;
		}

		public async Task<CreatedDocumentInfoDto> ReceiveDocument(string documentId, string inn, CancellationToken cancellationToken)
		{
			var endPoint = $"api/RecieveDocument?documentId={documentId}&&inn={inn}";

			var response = await _httpClient.GetAsync(endPoint, cancellationToken);

			if(!response.IsSuccessStatusCode)
			{
				throw new Exception(
					$"Ошибка при получении статуса документа из Честного Знака. " +
					$"Документ: {documentId}. " +
					$"ИНН: {inn}. " +
					$"Код ошибки: {response.StatusCode}. " +
					$"Ошибка: {response.ReasonPhrase}");
			}

			var responseBody = await response.Content.ReadAsStreamAsync();
			var createdDocumentInfo = (await JsonSerializer.DeserializeAsync<IEnumerable<CreatedDocumentInfoDto>>(responseBody))
				.FirstOrDefault();

			return createdDocumentInfo;
		}
	}

	public class SendDocumentDataRequest
	{
		/// <summary>
		/// Документ
		/// </summary>
		public string Document { get; set; }
		/// <summary>
		/// Инн организации
		/// </summary>
		public string Inn { get; set; }
	}
}

using BitrixNotificationsSend.Contracts.Dto;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace BitrixNotificationsSend.Client
{
	public class BitrixNotificationsSendClient : IBitrixNotificationsSendClient
	{
		private readonly HttpClient _httpClient;

		public BitrixNotificationsSendClient(HttpClient httpClient)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		public async Task<Result> SendCounterpartiesCashlessDebtsNotification(IEnumerable<CounterpartyCashlessDebtDto> counterpartiesCashlessDebts, CancellationToken cancellationToken)
		{
			var content = JsonSerializer.Serialize(counterpartiesCashlessDebts.ToArray());
			HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

			var retryPolicy = Policy
				.Handle<HttpRequestException>()
				.Or<TimeoutException>()
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

			var result = await retryPolicy.ExecuteAndCaptureAsync(
				async (innerCancellationToken) =>
				{
					var response = await _httpClient.PostAsync("bitrix", httpContent, innerCancellationToken);
					var responseResult =
						response.IsSuccessStatusCode
						? Result.Success()
						: Result.Failure(Errors.BitrixNotificationsSendErrors.CreateSendCounterpartiesCashlessDebtsNotificationError(response.ReasonPhrase));
					return responseResult;
				},
				cancellationToken);

			return result.Result
				?? Errors.BitrixNotificationsSendErrors.CreateSendCounterpartiesCashlessDebtsNotificationError(result.FinalException.Message);
		}
	}
}

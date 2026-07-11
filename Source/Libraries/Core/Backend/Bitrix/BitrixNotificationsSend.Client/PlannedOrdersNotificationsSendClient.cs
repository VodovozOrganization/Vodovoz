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
using Vodovoz.Settings.Notifications;

namespace BitrixNotificationsSend.Client
{
	public class PlannedOrdersNotificationsSendClient : IPlannedOrdersNotificationsSendClient
	{
		private readonly HttpClient _httpClient;
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;

		public PlannedOrdersNotificationsSendClient(
			HttpClient httpClient,
			IBitrixNotificationsSendSettings bitrixNotificationsSendSettings)
		{
			_httpClient = httpClient
				?? throw new ArgumentNullException(nameof(httpClient));
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings
				?? throw new ArgumentNullException(nameof(bitrixNotificationsSendSettings));
		}

		public async Task<Result> SendPlannedOrdersNotification(IEnumerable<PlannedOrderDto> plannedOrders, CancellationToken cancellationToken)
		{
			var content = JsonSerializer.Serialize(plannedOrders.ToArray());
			HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

			var retryPolicy = Policy
				.Handle<HttpRequestException>()
				.Or<TimeoutException>()
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

			var result = await retryPolicy.ExecuteAndCaptureAsync(
				async (innerCancellationToken) =>
				{
					var response = await _httpClient.PostAsync($"handler.php?token={_bitrixNotificationsSendSettings.PlannedOrdersBitrixToken}", httpContent, innerCancellationToken);
					var responseResult =
						response.IsSuccessStatusCode
						? Result.Success()
						: Result.Failure(Errors.BitrixNotificationsSendErrors.CreateSendPlannedOrdersNotificationError(response.ReasonPhrase));
					return responseResult;
				},
				cancellationToken);

			return result.Result
				?? Errors.BitrixNotificationsSendErrors.CreateSendPlannedOrdersNotificationError(result.FinalException.Message);
		}
	}
}

using DriverApi.Notifications.Client.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DriverApi.Contracts.V6.Requests;
using Vodovoz.Core.Domain.Results;
using Vodovoz.NotificationSenders;
using Vodovoz.Settings.Logistics;
using VodovozBusiness.NotificationSenders;
using CommonErrors = Vodovoz.Errors.Common;

namespace DriverApi.Notifications.Client.Clients
{
	public class DriverApiNotificationsClient :
		ISmsPaymentStatusNotificationSender,
		IFastDeliveryOrderAddedNotificationSender,
		IWaitingTimeChangedNotificationSender,
		ICashRequestForDriverIsGivenForTakeNotificationSender,
		IRouteListChangesNotificationSender
	{
		private readonly ILogger<DriverApiNotificationsClient> _logger;
		private readonly IDriverApiSettings _driverApiSettings;
		private readonly HttpClient _httpClient;

		public DriverApiNotificationsClient(
			ILogger<DriverApiNotificationsClient> logger,
			IHttpClientFactory httpClientFactory,
			IDriverApiSettings driverApiSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_driverApiSettings = driverApiSettings ?? throw new ArgumentNullException(nameof(driverApiSettings));

			_httpClient = (httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory)))
				.CreateClient(nameof(DriverApiNotificationsClient));
		}

		public async Task NotifyOfSmsPaymentStatusChanged(int orderId)
		{
			using(var response = await _httpClient.PostAsJsonAsync(_driverApiSettings.NotifyOfSmsPaymentStatusChangedUri, orderId))
			{
				if(response.IsSuccessStatusCode)
				{
					return;
				}

				throw new DriverApiNotificationsClientException(response.ReasonPhrase);
			}
		}

		public async Task NotifyOfFastDeliveryOrderAdded(int orderId)
		{
			try
			{
				using(var response = await _httpClient.PostAsJsonAsync(_driverApiSettings.NotifyOfFastDeliveryOrderAddedUri, orderId))
				{
					if(response.IsSuccessStatusCode)
					{
						return;
					}

					throw new DriverApiNotificationsClientException(response.ReasonPhrase);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, $"Не удалось уведомить водителя о добавлении заказа {orderId} с быстрой доставкой в МЛ");
			}
		}

		public async Task NotifyOfWaitingTimeChanged(int orderId)
		{
			using(var response = await _httpClient.PostAsJsonAsync(_driverApiSettings.NotifyOfWaitingTimeChangedURI, orderId))
			{
				if(response.IsSuccessStatusCode)
				{
					return;
				}

				throw new DriverApiNotificationsClientException(response.ReasonPhrase);
			}
		}

		public async Task NotifyOfCashRequestForDriverIsGivenForTake(int cashRequestId)
		{
			using(var response = await _httpClient.PostAsJsonAsync(_driverApiSettings.NotifyOfCashRequestForDriverIsGivenForTakeUri, cashRequestId))
			{
				if(response.IsSuccessStatusCode)
				{
					return;
				}

				throw new DriverApiNotificationsClientException(response.ReasonPhrase);
			}
		}

		public async Task<Result> NotifyOfRouteListChanged(NotificationRouteListChangesRequest changesRequest)
		{
			using(var response = await _httpClient.PostAsJsonAsync(_driverApiSettings.NotifyOfRouteListChangedUri, changesRequest))
			{
				var responseBody = await response.Content.ReadAsStringAsync();

				if(response.IsSuccessStatusCode && string.IsNullOrWhiteSpace(responseBody))
				{
					return Result.Success();
				}

				if(string.IsNullOrWhiteSpace(responseBody))
				{
					return Result.Failure(CommonErrors.DriverApiClientErrors.ApiError(response.ReasonPhrase));
				}

				try
				{
					var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);
					return Result.Failure(CommonErrors.DriverApiClientErrors.OrderWithGoodsTransferingIsTransferedNotNotified(problemDetails.Detail));
				}
				catch(Exception ex)
				{
					return Result.Failure(CommonErrors.DriverApiClientErrors.ApiError(ex.Message));
				}
			}
		}
	}
}

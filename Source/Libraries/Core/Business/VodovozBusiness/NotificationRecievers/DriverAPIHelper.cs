using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VodovozBusiness.NotificationRecievers;

namespace Vodovoz.NotificationRecievers
{
	public class DriverAPIHelper : 
		ISmsPaymentStatusNotificationReciever,
		IFastDeliveryOrderAddedNotificationReciever,
		IWaitingTimeChangedNotificationReciever,
		ICashRequestForDriverIsGivenForTakeNotificationReciever,
		IRouteListTransferhandByHandReciever,
		IDisposable
	{
		private readonly ILogger<DriverAPIHelper> _logger;
		private string _notifyOfSmsPaymentStatusChangedUri;
		private string _notifyOfFastDeliveryOrderAddedUri;
		private string _notifyOfWaitingTimeChangedUri;
		private string _notifyOfOrderWithGoodsTransferingIsTransferedUri;
		private string _notifyOfCashRequestForDriverIsGivenForTakeUri;
		private HttpClient _apiClient;

		public DriverAPIHelper(
			ILogger<DriverAPIHelper> logger,
			DriverApiHelperConfiguration configuration)
		{
			_logger = logger;
			InitializeClient(configuration);
		}

		private void InitializeClient(DriverApiHelperConfiguration configuration)
		{
			_apiClient = new HttpClient();
			_apiClient.BaseAddress = configuration.ApiBase;
			_apiClient.DefaultRequestHeaders.Accept.Clear();
			_apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			_notifyOfSmsPaymentStatusChangedUri = configuration.NotifyOfSmsPaymentStatusChangedURI;
			_notifyOfFastDeliveryOrderAddedUri = configuration.NotifyOfFastDeliveryOrderAddedURI;
			_notifyOfWaitingTimeChangedUri = configuration.NotifyOfWaitingTimeChangedURI;
			_notifyOfOrderWithGoodsTransferingIsTransferedUri = configuration.NotifyOfOrderWithGoodsTransferingIsTransferedUri;
			_notifyOfCashRequestForDriverIsGivenForTakeUri = configuration.NotifyOfCashRequestForDriverIsGivenForTakeUri;
		}

		public async Task NotifyOfSmsPaymentStatusChanged(int orderId)
		{
			using(var response = await _apiClient.PostAsJsonAsync(_notifyOfSmsPaymentStatusChangedUri, orderId))
			{
				if(response.IsSuccessStatusCode)
				{
					return;
				}
				throw new DriverAPIHelperException(response.ReasonPhrase);
			}
		}

		public async Task NotifyOfFastDeliveryOrderAdded(int orderId)
		{
			try
			{
				using(var response = await _apiClient.PostAsJsonAsync(_notifyOfFastDeliveryOrderAddedUri, orderId))
				{
					if(response.IsSuccessStatusCode)
					{
						return;
					}

					throw new DriverAPIHelperException(response.ReasonPhrase);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, $"Не удалось уведомить водителя о добавлении заказа {orderId} с быстрой доставкой в МЛ");
			}
		}

		public async Task NotifyOfWaitingTimeChanged(int orderId)
		{
			using(var response = await _apiClient.PostAsJsonAsync(_notifyOfWaitingTimeChangedUri, orderId))
			{
				if(response.IsSuccessStatusCode)
				{
					return;
				}

				throw new DriverAPIHelperException(response.ReasonPhrase);
			}
		}

		public async Task NotifyOfCashRequestForDriverIsGivenForTake(int cashRequestId)
		{
			using(var response = await _apiClient.PostAsJsonAsync(_notifyOfCashRequestForDriverIsGivenForTakeUri, cashRequestId))
			{
				if(response.IsSuccessStatusCode)
				{
					return;
				}
				throw new DriverAPIHelperException(response.ReasonPhrase);
			}
		}

		public void Dispose()
		{
			_apiClient?.Dispose();
		}

		public async Task NotifyOfOrderWithGoodsTransferingIsTransfered(int orderId)
		{
			using(var response = await _apiClient.PostAsJsonAsync(_notifyOfOrderWithGoodsTransferingIsTransferedUri, orderId))
			{
				var responseBody = await response.Content.ReadAsAsync<ProblemDetails>();

				if(response.IsSuccessStatusCode && responseBody is null)
				{
					return;
				}

				if(responseBody is null)
				{
					throw new DriverAPIHelperException(response.ReasonPhrase);
				}
				else
				{
					throw new DriverAPIHelperException(responseBody.Detail);
				}
			}
		}
	}
}

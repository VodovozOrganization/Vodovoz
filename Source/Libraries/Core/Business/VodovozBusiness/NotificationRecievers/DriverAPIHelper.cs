using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Vodovoz.NotificationRecievers
{
	public class DriverAPIHelper :
		ISmsPaymentStatusNotificationReciever,
		IFastDeliveryOrderAddedNotificationReciever,
		IWaitingTimeChangedNotificationReciever,
		IDisposable
	{
		private readonly ILogger _logger;
		private string _notifyOfSmsPaymentStatusChangedUri;
		private string _notifyOfFastDeliveryOrderAddedUri;
		private string _notifyOfWaitingTimeChangedUri;
		private HttpClient _apiClient;

		public DriverAPIHelper(
			ILogger logger,
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
			_notifyOfFastDeliveryOrderAddedUri = configuration.NotifyOfFastDeliveryOrderAddedUri;
			_notifyOfWaitingTimeChangedUri = configuration.NotifyOfWaitingTimeChangedUri;
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
			try
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
			catch(Exception e)
			{
				_logger.LogError(e, $"Не удалось уведомить водителя изменении времени ожидания заказа {orderId}");
			}
		}

		public void Dispose()
		{
			_apiClient?.Dispose();
		}
	}

	public class DriverAPIHelperException : Exception
	{
		public DriverAPIHelperException(string message) : base(message)
		{ }
	}

	public class DriverApiHelperConfiguration
	{
		public Uri ApiBase { get; set; }
		public string NotifyOfSmsPaymentStatusChangedURI { get; set; }
		public string NotifyOfFastDeliveryOrderAddedUri { get; set; }
		public string NotifyOfWaitingTimeChangedUri { get; set; }
	}
}

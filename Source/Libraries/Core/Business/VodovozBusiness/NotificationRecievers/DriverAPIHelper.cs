﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Vodovoz.NotificationRecievers
{
	public class DriverAPIHelper : 
		ISmsPaymentStatusNotificationReciever,
		IFastDeliveryOrderAddedNotificationReciever,
		IWaitingTimeChangedNotificationReciever,
		ICashRequestForDriverIsGivenForTakeNotificationReciever,
		IDisposable
	{
		private string _notifyOfSmsPaymentStatusChangedUri;
		private string _notifyOfFastDeliveryOrderAddedUri;
		private string _notifyOfWaitingTimeChangedUri;
		private string _notifyOfCashRequestForDriverIsGivenForTakeUri;
		private HttpClient _apiClient;

		public DriverAPIHelper(DriverApiHelperConfiguration configuration)
		{
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
			using(var response = await _apiClient.PostAsJsonAsync(_notifyOfFastDeliveryOrderAddedUri, orderId))
			{
				if(response.IsSuccessStatusCode)
				{
					return;
				}
				throw new DriverAPIHelperException(response.ReasonPhrase);
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
		public string NotifyOfFastDeliveryOrderAddedURI { get; set; }
		public string NotifyOfWaitingTimeChangedURI { get; set; }
		public string NotifyOfCashRequestForDriverIsGivenForTakeUri { get; set; }
	}
}

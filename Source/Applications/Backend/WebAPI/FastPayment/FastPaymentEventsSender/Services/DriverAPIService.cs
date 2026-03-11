using System;
using System.Net.Http;
using System.Threading.Tasks;
using FastPaymentEventsSender.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FastPaymentEventsSender.Services
{
	public class DriverAPIService : IDriverAPIService
	{
		private readonly ILogger<DriverAPIService> _logger;
		private readonly HttpClient _httpClient;
		private readonly DriverApiOptions _driverApiOptions;

		public DriverAPIService(
			ILogger<DriverAPIService> logger,
			HttpClient client,
			IOptionsSnapshot<DriverApiOptions> driverApiOptions)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_driverApiOptions = (driverApiOptions ?? throw new ArgumentNullException(nameof(driverApiOptions))).Value;
		}

		public async Task NotifyOfFastPaymentStatusChangedAsync(int orderId)
		{
			_logger.LogInformation("Уведомляем водителя об изменении статуса оплаты заказа: {OrderNumber}", orderId);
			var response = await _httpClient.PostAsJsonAsync(_driverApiOptions.FastPaymentStatusChangedEndpoint, orderId);

			if(response.IsSuccessStatusCode)
			{
				return;
			}
			throw new Exception(response.ReasonPhrase);
		}
	}
}

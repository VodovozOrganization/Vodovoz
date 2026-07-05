using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mango.Vpbx.Client.Services
{
	/// <inheritdoc/>
	public class MangoCallsService : IMangoCallsService
	{
		private readonly ILogger<MangoCallsService> _logger;
		private readonly HttpClient _httpClient;

		public MangoCallsService(
			ILogger<MangoCallsService> logger,
			HttpClient httpClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		/// <inheritdoc/>
		public async Task MakeWebhookCall(string extension, string toNumber, CancellationToken cancellationToken)
		{
			var requestUri = new Uri($"{_httpClient.BaseAddress}&EmployeeNUM={extension}&TelNumbr={toNumber}");

			_logger.LogInformation(
				"Отправка команды на звонок с использованием вебхука с добавочного номера {Extension} на номер {ToNumber}",
				extension,
				toNumber);

			var response = await _httpClient.GetAsync(requestUri, cancellationToken);

			response.EnsureSuccessStatusCode();
		}
	}
}

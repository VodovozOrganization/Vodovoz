using Microsoft.Extensions.Logging;
using Taxcom.Client.Api;

namespace TaxcomEdoApi.ErrorHandlers
{
	public class TaxcomApiErrorHandler : ITaxcomApiErrorHandler
	{
		private readonly ILogger<TaxcomApiErrorHandler> _logger;

		public TaxcomApiErrorHandler(ILogger<TaxcomApiErrorHandler> logger)
		{
			_logger = logger;
		}

		public bool OnError(TaxcomApiErrorDescription description)
		{
			_logger.LogError(
				description.InnerException,
				"Ошибка регламента ЭДО при AutoSendReceive: {Message}",
				description.InnerException?.Message ?? description.ToString());

			// не останавливать остальные команды
			return false;
		}
	}
}

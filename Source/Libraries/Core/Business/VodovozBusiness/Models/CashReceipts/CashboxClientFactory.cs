using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Models.CashReceipts.DTO;

namespace Vodovoz.Models.CashReceipts
{
	public class CashboxClientFactory
	{
		private readonly ILogger<CashboxClient> _logger;
		private readonly string _baseUrl;

		public CashboxClientFactory(ILogger<CashboxClient> logger, string baseUrl)
		{
			if(string.IsNullOrWhiteSpace(baseUrl))
			{
				throw new ArgumentException($"'{nameof(baseUrl)}' cannot be null or whitespace.", nameof(baseUrl));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_baseUrl = baseUrl;
		}

		public ICashboxClient CreateClient(CashboxSetting cashBox)
		{
			if(cashBox is null)
			{
				throw new ArgumentNullException(nameof(cashBox));
			}

			var result = new CashboxClient(_logger, cashBox, _baseUrl);
			return result;
		}
	}
}

using Microsoft.Extensions.Options;
using System;
using Vodovoz.Settings.Counterparty;

namespace EmailDebtNotificationWorker.Options
{
	public class ConfigureChainStoreDelayLettersOptions : IConfigureOptions<ChainStoreDelayLettersOptions>
	{
		private readonly IChainStoreDelayLettersSettings _chainStoreDelayLettersSettings;

		public ConfigureChainStoreDelayLettersOptions(IChainStoreDelayLettersSettings chainStoreDelayLettersSettings)
		{
			_chainStoreDelayLettersSettings =
				chainStoreDelayLettersSettings ?? throw new ArgumentNullException(nameof(chainStoreDelayLettersSettings));
		}

		public void Configure(ChainStoreDelayLettersOptions options)
		{
			options.Interval = _chainStoreDelayLettersSettings.SendInterval;
			options.SendTimeFrom = _chainStoreDelayLettersSettings.SendTimeFrom;
			options.SendTimeTo = _chainStoreDelayLettersSettings.SendTimeTo;
		}
	}
}

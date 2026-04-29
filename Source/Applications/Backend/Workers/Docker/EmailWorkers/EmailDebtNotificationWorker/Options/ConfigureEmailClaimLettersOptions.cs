using Microsoft.Extensions.Options;
using System;
using Vodovoz.Settings.Counterparty;

namespace EmailDebtNotificationWorker.Options
{
	public class ConfigureEmailClaimLettersOptions : IConfigureOptions<EmailClaimLettersOptions>
	{
		private readonly IDebtorsSettings _debtorsSettings;

		public ConfigureEmailClaimLettersOptions(IDebtorsSettings debtorsSettings)
		{
			_debtorsSettings = debtorsSettings ?? throw new ArgumentNullException(nameof(debtorsSettings));
		}

		public void Configure(EmailClaimLettersOptions options)
		{
			options.OverdueDebtorDebtExpiredDaysAgo = _debtorsSettings.OverdueDebtorDebtExpiredDaysAgo;
			options.OverdueDebtorDebtInterval = _debtorsSettings.OverdueDebtorDebtInterval;
			options.OverdueDebtorDebtLettersCountPerInterval = _debtorsSettings.OverdueDebtorDebtLettersCountPerInterval;
		}
	}
}

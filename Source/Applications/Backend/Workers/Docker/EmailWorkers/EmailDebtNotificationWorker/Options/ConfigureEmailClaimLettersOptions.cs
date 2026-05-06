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
			options.LettersOfClaimTimeoutDays = _debtorsSettings.LettersOfClaimTimeoutDays;
			options.WorkerInterval = _debtorsSettings.LettersOfClaimWorkerInterval;
			options.MaxCountPerInterval = _debtorsSettings.LettersOfClaimMaxCountPerInterval;
			options.MaxCountPerDay = _debtorsSettings.LettersOfClaimMaxCountPerDay;
			options.ResendIntervalDays = _debtorsSettings.LettersOfClaimResendIntervalDays;
		}
	}
}

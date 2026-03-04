namespace Vodovoz.Settings.Counterparty
{
	public interface IDebtorsSettings
	{
		/// <summary>
		/// Скрывать приостановленных контрагентов
		/// </summary>
		int GetSuspendedCounterpartyId { get; }

		/// <summary>
		/// Скрывать аннулированных контрагентов
		/// </summary>

		int GetCancellationCounterpartyId { get; }

		/// <summary>
		/// Воркер по рассылке писем о задолженности отключен
		/// </summary>
		bool DebtNotificationWorkerIsDisabled { get; set; }

		/// <summary>
		/// Интервал срабатывания воркера по рассылке писем о задолженности в секундах
		/// </summary>
		int DebtNotificationWorkerIntervalSeconds { get; }
	}
}

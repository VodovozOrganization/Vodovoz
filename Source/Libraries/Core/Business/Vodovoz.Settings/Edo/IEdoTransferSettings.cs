namespace Vodovoz.Settings.Edo
{
	public interface IEdoTransferSettings
	{
		/// <summary>
		/// Время через которое будет запущен принудительный старт трансфера.
		/// Означает что время на ожидание дополнительных запросов на трансфер истекло.
		/// </summary>
		int TransferTaskRequestsWaitingTimeoutMinute { get; }

		/// <summary>
		/// Интервал проверки таймаута ожидания запросов на трансфер.
		/// </summary>
		int TransferTaskRequestsWaitingTimeoutCheckIntervalSecond { get; }

		/// <summary>
		/// Минимальное количество кодов для начала трансфера
		/// </summary>
		int MinCodesCountForStartTransfer { get; }
	}
}

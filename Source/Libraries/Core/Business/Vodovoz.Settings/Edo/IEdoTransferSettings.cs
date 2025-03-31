using System;

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
		/// Интервал обновления ожидающих задач на трансфер с завершенными документами
		/// </summary>
		TimeSpan WaitingTransfersUpdateInterval { get; }

		/// <summary>
		/// Интервал отправки УПД по заказам Закр.Док
		/// </summary>
		TimeSpan ClosingDocumentsOrdersUpdSendInterval { get; }

		/// <summary>
		/// Минимальное количество кодов для начала трансфера
		/// </summary>
		int MinCodesCountForStartTransfer { get; }

		/// <summary>
		/// Процент добавочной цены к себестоимости для трансфера
		/// </summary>
		int AdditionalPurchasePricePrecentForTransfer { get; }
	}
}

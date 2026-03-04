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
		/// Количество обновляемых ожидающих задач на трансфер с завершенными документами за один прогон
		/// </summary>
		int WaitingTransfersCountToProcess { get; }

		/// <summary>
		/// Интервал отправки УПД по заказам Закр.Док
		/// </summary>
		TimeSpan ClosingDocumentsOrdersUpdSendInterval { get; }

		/// <summary>
		/// Максимальное количество дней с даты доставки заказа для отправки УПД по Закр.Док
		/// </summary>
		int ClosingDocumentsOrdersUpdSendMaxDaysFromDeliveryDate { get; }

		/// <summary>
		/// Минимальное количество кодов для начала трансфера
		/// </summary>
		int MinCodesCountForStartTransfer { get; }

		/// <summary>
		/// Процент добавочной цены к себестоимости для трансфера
		/// </summary>
		int AdditionalPurchasePricePrecentForTransfer { get; }

		/// <summary>
		/// Время после которого трансфер можно считать зависшим
		/// По истечении этого времени задачу можно отменить 
		/// а по трансферу отправить предложение об аннулировании.
		/// Время трансфера вычисляется с момента отправки его в систему ЭДО
		/// записывается в TransferStartTime
		/// </summary>
		TimeSpan TransferTimeoutInterval { get; }
	}
}

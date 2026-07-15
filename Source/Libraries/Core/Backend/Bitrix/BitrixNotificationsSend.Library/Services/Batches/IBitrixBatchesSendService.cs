using BitrixNotificationsSend.Contracts.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace BitrixNotificationsSend.Library.Services.Batches
{
	/// <summary>
	/// Отправка команд в Битрикс24 пакетами с учётом ограничений API:
	/// размера пакета, интенсивности запросов и операционного лимита методов
	/// в скользящем 10-минутном окне
	/// </summary>
	public interface IBitrixBatchesSendService
	{
		/// <summary>
		/// Отправка элементов пакетами с учётом лимитов Битрикс24.
		/// Элементы нарезаются на пакеты максимально допустимого размера, между пакетами выдерживается пауза,
		/// при приближении к операционному лимиту выполняется ожидание сброса бюджета
		/// Команды, отклонённые из-за операционного лимита, после освобождения бюджета отправляются повторно один раз
		/// </summary>
		/// <typeparam name="TItem">Тип элемента, отправляемого командой пакета</typeparam>
		/// <param name="items">Элементы для отправки</param>
		/// <param name="commandKeySelector">Селектор ключа команды пакетного запроса для элемента</param>
		/// <param name="sendBatch">Отправка одного пакета элементов, например через клиент пакетных запросов</param>
		/// <param name="onBatchItemsSucceeded"> Обработка элементов пакета, команды по которым выполнены успешно </param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат отправки серии пакетов</returns>
		Task<BitrixBatchesSendResult<TItem>> SendAll<TItem>(
			IReadOnlyList<TItem> items,
			Func<TItem, string> commandKeySelector,
			Func<IReadOnlyList<TItem>, CancellationToken, Task<Result<BitrixBatchResult>>> sendBatch,
			Func<IReadOnlyList<TItem>, CancellationToken, Task> onBatchItemsSucceeded,
			CancellationToken cancellationToken);
	}
}

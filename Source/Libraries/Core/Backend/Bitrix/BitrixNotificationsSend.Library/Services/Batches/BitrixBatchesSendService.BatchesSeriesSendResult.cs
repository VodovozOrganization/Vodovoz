using System;
using System.Collections.Generic;

namespace BitrixNotificationsSend.Library.Services.Batches
{
	public partial class BitrixBatchesSendService
	{
		/// <summary>
		/// Результат отправки одной серии пакетов команд в Битрикс24
		/// </summary>
		public class BatchesSeriesSendResult<TItem>
		{
			/// <summary>
			/// Количество успешно выполненных команд
			/// </summary>
			public int SuccessfulCount { get; set; }

			/// <summary>
			/// Элементы, команды по которым не выполнены из-за операционного лимита Битрикс24
			/// Их можно отправить повторно после освобождения бюджета
			/// </summary>
			public List<TItem> OperatingLimitFailedItems { get; } = new List<TItem>();

			/// <summary>
			/// Максимальное накопленное операционное время метода, сек
			/// </summary>
			public double OperatingSeconds { get; set; }

			/// <summary>
			/// Момент сброса операционного бюджета метода (UTC),
			/// null - если данные не приходили в ответах
			/// </summary>
			public DateTime? OperatingResetAt { get; set; }
		}
	}
}

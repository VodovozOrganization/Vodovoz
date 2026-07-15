using System.Collections.Generic;

namespace BitrixNotificationsSend.Library.Services.Batches
{
	/// <summary>
	/// Результат отправки серии пакетов команд в Битрикс24
	/// </summary>
	/// <typeparam name="TItem">Тип элемента, отправляемого командой пакета</typeparam>
	public class BitrixBatchesSendResult<TItem>
	{
		/// <summary>
		/// Количество успешно выполненных команд
		/// </summary>
		public int SuccessfulCount { get; set; }

		/// <summary>
		/// Элементы, команды по которым не выполнены из-за операционного лимита Битрикс24
		/// даже после повторной отправки по освобождении бюджета
		/// </summary>
		public IList<TItem> OperatingLimitFailedItems { get; set; } = new List<TItem>();
	}
}

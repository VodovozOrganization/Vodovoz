using System.Collections.Generic;

namespace BitrixNotificationsSend.Library.Services.Batches
{
	/// <summary>
	/// Результат отправки серии пакетов команд в Битрикс24
	/// </summary>
	/// <typeparam name="TItem">Тип элемента, отправляемого командой пакета</typeparam>
	public class BatchesSendResult<TItem>
	{
		/// <summary>
		/// Количество успешно выполненных команд
		/// </summary>
		public int SuccessfulCount { get; set; }

		/// <summary>
		/// Элементы, команды по которым не выполнены из-за операционного лимита Битрикс24
		/// </summary>
		public List<TItem> OperatingLimitFailedItems { get; } = new List<TItem>();
	}
}

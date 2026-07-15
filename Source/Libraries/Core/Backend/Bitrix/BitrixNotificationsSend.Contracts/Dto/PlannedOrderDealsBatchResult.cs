using System;
using System.Collections.Generic;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Разобранный результат пакетного создания сделок по плановым заказам в Битрикс24
	/// </summary>
	public class PlannedOrderDealsBatchResult
	{
		/// <summary>
		/// Ключи команд, по которым сделки успешно созданы
		/// </summary>
		public IList<string> CreatedDealKeys { get; set; } = new List<string>();

		/// <summary>
		/// Ошибки создания сделок по отдельным командам пакета
		/// </summary>
		public IList<PlannedOrderDealCreationError> Errors { get; set; } = new List<PlannedOrderDealCreationError>();

		/// <summary>
		/// Максимальное накопленное операционное время метода создания сделки из ответа Битрикс24, сек.
		/// Лимит Битрикс24 - 480 сек на метод в скользящем 10-минутном окне
		/// </summary>
		public double OperatingSeconds { get; set; }

		/// <summary>
		/// Момент сброса операционного бюджета метода (UTC) из ответа Битрикс24,
		/// null - если данные не пришли в ответе
		/// </summary>
		public DateTime? OperatingResetAt { get; set; }
	}
}

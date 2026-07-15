using BitrixNotificationsSend.Contracts.Dto;
using System;
using System.Collections.Generic;

namespace BitrixNotificationsSend.Library.Services
{
	public partial class PlannedOrdersNotificationsSendService
	{
		/// <summary>
		/// Результат отправки серии пакетов создания сделок по плановым заказам в Битрикс24
		/// </summary>
		public class DealsBatchesSendResult
		{
			/// <summary>
			/// Количество успешно созданных сделок
			/// </summary>
			public int SuccessfulDealsCount { get; set; }

			/// <summary>
			/// Плановые заказы, по которым сделки не созданы из-за операционного лимита Битрикс24.
			/// Их можно отправить повторно после освобождения бюджета
			/// </summary>
			public IList<PlannedOrderDto> OperatingLimitFailedOrders { get; set; } = new List<PlannedOrderDto>();

			/// <summary>
			/// Максимальное накопленное операционное время метода создания сделки, сек
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

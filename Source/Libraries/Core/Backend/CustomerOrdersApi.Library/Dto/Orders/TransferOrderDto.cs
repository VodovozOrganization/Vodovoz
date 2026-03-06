using System;
using System.ComponentModel.DataAnnotations;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// DTO для запроса на перенос заказа
	/// </summary>
	public class TransferOrderDto : OrderOperationDto
	{
		/// <summary>
		/// Дата, на которую необходимо перенести заказ
		/// </summary>
		[Required]
		public DateTime DeliveryDate { get; set; }

		/// <summary>
		/// Время доставки заказа, на которое необходимо перенести заказ
		/// </summary>
		[Required]
		public int DeliveryScheduleId { get; set; }
	}
}

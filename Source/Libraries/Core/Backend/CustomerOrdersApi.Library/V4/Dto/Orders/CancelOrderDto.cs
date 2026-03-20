using CustomerOrdersApi.Library.Dto.Orders;
using System.ComponentModel.DataAnnotations;

namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	public class CancelOrderDto : OrderOperationDto
	{
		/// <summary>
		/// Идентификатор транзакции
		/// </summary>
		[Required]
		public string TransactionId { get; set; }
	}
}

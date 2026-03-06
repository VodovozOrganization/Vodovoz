using System.ComponentModel.DataAnnotations;

namespace CustomerOrdersApi.Library.Dto.Orders
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

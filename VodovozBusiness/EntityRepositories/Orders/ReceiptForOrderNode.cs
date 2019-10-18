using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
namespace Vodovoz.EntityRepositories.Orders
{
	public class ReceiptForOrderNode
	{
		public int OrderId { get; set; }
		public int? ReceiptId { get; set; }
		public bool? WasSent { get; set; }
	}
}

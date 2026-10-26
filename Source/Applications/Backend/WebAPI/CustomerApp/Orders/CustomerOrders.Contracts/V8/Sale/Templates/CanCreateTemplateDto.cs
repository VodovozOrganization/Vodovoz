using System.Collections.Generic;
using CustomerApp.Contracts.Sale;
using CustomerOrders.Abstractions.V8.Sale;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	public class CanCreateTemplateDto : ICanCreateOnlineOrderTemplate
	{
		public int? ErpCounterpartyId { get; set; }
		public int? DeliveryPointId { get; set; }
		public bool IsSelfDelivery { get; set; }
		public bool IsFastDelivery { get; set; }
		public IEnumerable<ICartItem> CartItems { get; set; }
	}
}

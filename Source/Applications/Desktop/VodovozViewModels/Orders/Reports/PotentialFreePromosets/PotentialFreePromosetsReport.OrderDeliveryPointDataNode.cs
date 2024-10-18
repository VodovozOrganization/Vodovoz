using System;

namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	public partial class PotentialFreePromosetsReport
	{
		public class OrderDeliveryPointDataNode
		{
			public int OrderId { get; set; }
			public DateTime? OrderCreateDate { get; set; }
			public DateTime? OrderDeliveryDate { get; set; }
			public int AuthorId { get; set; }
			public string AuthorName { get; set; }
			public int ClientId { get; set; }
			public string ClientName { get; set; }
			public int PromosetId { get; set; }
			public string PromosetName { get; set; }
			public AddressDataNode AddressDataNode { get; set; }
			public string DeliveryPointCompiledAddress { get; set; }
			public int? DeliveryPointAddressCategoryId { get; set; }
			public string DeliveryPointAddressCategoryName { get; set; }
		}
	}
}

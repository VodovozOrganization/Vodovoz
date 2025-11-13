using System;

namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	public partial class PotentialFreePromosetsReport
	{
		public class OrderWithPhoneDataNode
		{
			public int OrderId { get; set; }
			public int ClientId { get; set; }
			public int DeliveryPointId { get; set; }
			public DateTime? OrderCreateDate { get; set; }
			public DateTime? OrderDeliveryDate { get; set; }
			public int AuthorId { get; set; }
			public string AuthorName { get; set; }
			public string PhoneNumber { get; set; }
			public string PhoneDigitNumber { get; set; }
			public string ClientName { get; set; }
			public string DeliveryPointAddress { get; set; }
			public string DeliveryPointCategory { get; set; }
			public int PromosetId { get; set; }
			public string PromosetName { get; set; }
			public bool IsRoot { get; set; }
		}
	}
}

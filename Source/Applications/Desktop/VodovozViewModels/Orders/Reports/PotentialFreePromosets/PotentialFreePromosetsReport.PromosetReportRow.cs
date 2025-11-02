using System;

namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	public partial class PotentialFreePromosetsReport
	{
		public class PromosetReportRow
		{
			public int SequenceNumber { get; set; }
			public string Address { get; set; }
			public string AddressCategory { get; set; }
			public string Phone { get; set; }
			public string Client { get; set; }
			public int Order { get; set; }
			public DateTime? OrderCreationDate { get; set; }
			public DateTime? OrderDeliveryDate { get; set; }
			public string Promoset { get; set; }
			public string Author { get; set; }
			public bool IsRootRow { get; set; }
		}
	}
}

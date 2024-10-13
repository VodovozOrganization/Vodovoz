using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	public class PotentialFreePromosetsReport
	{
		public IEnumerable<PromosetReportRow> ReportRows { get; set; } = new List<PromosetReportRow>();

		public class PromosetReportRow
		{
			public int SequenceNumber { get; set; }
			public string Address { get; set; }
			public string AddressType { get; set; }
			public string Phone { get; set; }
			public string Client { get; set; }
			public int Order { get; set; }
			public DateTime OrderCreationDate { get; set; }
			public DateTime? OrderDeliveryDate { get; set; }
			public string Promoset { get; set; }
			public string Author { get; set; }
			public bool IsRootRow { get; set; }
		}
	}
}

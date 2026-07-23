using System;
using Vodovoz.Domain.Client.ClientClassification;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public class MarketingReportOrderNode
	{
		public int OrderId { get; set; }
		public int ClientId { get; set; }
		public DateTime? DeliveryDate { get; set; }
		public DateTime? CreateDate { get; set; }
		public decimal OrderSum { get; set; }
		public decimal Bottles19LCount { get; set; }
		public bool HasAdditionalServices { get; set; }
		public int? Rating { get; set; }
		public int AuthorId { get; set; }
		public string AuthorName { get; set; }
		public CounterpartyCompositeClassification? AbcClassification { get; set; }

		public DateTime GetReportDate(MarketingReportDateType dateType) =>
			(dateType == MarketingReportDateType.CreationDate ? CreateDate : DeliveryDate) ?? DateTime.MinValue;
	}
}

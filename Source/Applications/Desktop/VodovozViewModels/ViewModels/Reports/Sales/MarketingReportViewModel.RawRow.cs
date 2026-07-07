using System;
using Vodovoz.Domain.Client.ClientClassification;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel
	{
		public class MarketingReportRawRow
		{
			public int OrderId { get; set; }
			public int ClientId { get; set; }
			public DateTime OrderDate { get; set; }
			public decimal OrderSum { get; set; }
			public CounterpartyCompositeClassification AbcClass { get; set; }
			public Vodovoz.Core.Domain.Clients.Source? OnlineSource { get; set; }
			public int? AuthorSubdivisionId { get; set; }
			public string AuthorSubdivisionName { get; set; }
			public int? Rating { get; set; }
			public int HasAdditionalServiceFlag { get; set; }
			public bool HasAdditionalService => HasAdditionalServiceFlag == 1;
			public decimal BottlesCount19L { get; set; }
			public bool IsFirstOrderEver { get; set; }
		}

	}
}

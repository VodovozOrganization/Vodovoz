using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		public partial class TurnoverWithDynamicsReport
		{
			public class OrderItemNode
			{
				public int Id { get; set; }

				public int NomenclatureId { get; set; }

				public string NomenclatureOfficialName { get; set; }

				public NomenclatureCategory NomenclatureCategory { get; set; }

				public int? CounterpartyId { get; set; }

				public CounterpartyType? CounterpartyType { get; set; }
				public string CounterpartySubtype { get; set; }
				public int? CounterpartySubtypeId { get; set; }

				public string CounterpartyPhones { get; set; }

				public string CounterpartyEmails { get; set; }

				public string CounterpartyFullName { get; set; }

				public int? OrganizationId { get; set; }

				public string OrganizationName { get; set; }

				public int? SubdivisionId { get; set; }

				public string SubdivisionName { get; set; }

				public PaymentType? PaymentType { get; set; }

				public int? OrderId { get; set; }

				public DateTime? OrderDeliveryDate { get; set; }

				public int RouteListId { get; set; }

				public int ProductGroupId { get; set; }

				public string ProductGroupName { get; set; }

				public decimal? ActualCount { get; set; }

				public decimal Count { get; set; }

				public decimal Price { get; set; }

				public decimal ActualSum { get; set; }

				public string OrderContactPhone { get; set; }

				public CounterpartyCompositeClassification CounterpartyClassification { get; set; }

				public int PromotionalSetId { get; set; }
				
				public string PromotionalSetName { get; set; }
				
				public int SalesManagerId { get; set; }
				
				public string SalesManagerName { get; set; }
				
				public int OrderAuthorId { get; set; }
				
				public string OrderAuthorName { get; set; }
				
			}
		}
	}
}

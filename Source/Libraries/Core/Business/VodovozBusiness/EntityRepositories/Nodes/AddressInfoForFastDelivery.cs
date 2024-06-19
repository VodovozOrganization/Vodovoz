using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Nodes
{
	public class AddressInfoForFastDelivery
	{
		public int RouteListId { get; set; }
		public int IndexInRoute { get; set; }
		public DateTime? StatusLastUpdate { get; set; }
		public RouteListItemStatus AddressStatus { get; set; }
		public int MinutesToUnload { get; set; }
		public decimal WaterCount { get; set; }
		public decimal ItemsSummaryWeight { get; set; }
		public decimal EquipmentsSummaryWeight { get; set; }
	}
}

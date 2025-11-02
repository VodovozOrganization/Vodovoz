namespace Vodovoz.EntityRepositories.Nodes
{
	public class RouteListFastDeliveriesCountNode
	{
		public int RouteListId { get; set; }
		public int UnclosedFastDeliveryAddresses { get; set; }
		public int MaxFastDeliveryOrdersCount { get; set; }
	}
}

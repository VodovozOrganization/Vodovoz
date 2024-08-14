using NetTopologySuite.Geometries;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public class OrderOnDayNode
	{
		public int OrderId { get; set; }
		public OrderStatus OrderStatus { get; set; }
		public decimal? DeliveryPointLatitude { get; set; }
		public decimal? DeliveryPointLongitude { get; set; }
		public string DeliveryPointShortAddress { get; set; }
		public string DeliveryPointCompiledAddress { get; set; }
		public Point DeliveryPointNetTopologyPoint { get; set; }
		public int DeliveryPointDistrictId { get; set; }
		public LogisticsRequirements LogisticsRequirements { get; set; }
		public OrderAddressType OrderAddressType { get; set; }
		public DeliverySchedule DeliverySchedule { get; set; }
		public int Total19LBottlesToDeliver { get; set; }
		public int Total6LBottlesToDeliver { get; set; }
		public int Total1500mlBottlesToDeliver { get; set; }
		public int Total600mlBottlesToDeliver { get; set; }
		public int Total500mlBottlesToDeliver { get; set; }
		public int? BottlesReturn { get; set; }
		public string OrderComment { get; set; }
		public string DeliveryPointComment { get; set; }
		public string CommentManager { get; set; }
		public string ODZComment { get; set; }
		public string OPComment { get; set; }
		public string DriverMobileAppComment { get; set; }
		public bool IsCoolerAddedToOrder { get; set; }
		public bool IsSmallBottlesAddedToOrder { get; set; }
	}
}

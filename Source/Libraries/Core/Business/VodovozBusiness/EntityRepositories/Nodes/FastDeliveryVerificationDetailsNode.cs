using System;
using Newtonsoft.Json;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Delivery;

namespace Vodovoz.EntityRepositories.Nodes
{
	public class FastDeliveryVerificationDetailsNode
	{
		public string DriverFIO => RouteList.Driver.GetPersonNameWithInitials();
		
		public FastDeliveryVerificationParameter<bool> IsGoodsEnough { get; set; }
			= new FastDeliveryVerificationParameter<bool>
			{
				IsValidParameter = true,
				ParameterValue = true
			};
		public FastDeliveryVerificationParameter<int> UnClosedFastDeliveries { get; set; }
			= new FastDeliveryVerificationParameter<int>
			{
				IsValidParameter = true,
				ParameterValue = default
			};
		public FastDeliveryVerificationParameter<TimeSpan> RemainingTimeForShipmentNewOrder { get; set; }
			= new FastDeliveryVerificationParameter<TimeSpan>
			{
				IsValidParameter = false,
				ParameterValue = default
			};
		public FastDeliveryVerificationParameter<TimeSpan> LastCoordinateTime { get; set; }
			= new FastDeliveryVerificationParameter<TimeSpan>
			{
				IsValidParameter = true,
				ParameterValue = default
			};
		public FastDeliveryVerificationParameter<decimal> DistanceByRoadToClient { get; set; }
			= new FastDeliveryVerificationParameter<decimal>
			{
				IsValidParameter = true,
				ParameterValue = default
			};
		public FastDeliveryVerificationParameter<decimal> DistanceByLineToClient { get; set; }
			= new FastDeliveryVerificationParameter<decimal>
			{
				IsValidParameter = true,
				ParameterValue = default
			};
		public bool IsValidRLToFastDelivery { get; set; } = true;
		public DateTime TimeStamp { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public RouteList RouteList { get; set; }
		public double RouteListFastDeliveryRadius { get; set; }
		public int RouteListMaxFastDeliveryOrders { get; set; }
		public int RouteListId { get; set; }
		public decimal? RouteListFastDeliveryMaxDistance { get; set; }
		public double FastDeliveryMaxDistanceParameterVersion { get; set; }
	}
}

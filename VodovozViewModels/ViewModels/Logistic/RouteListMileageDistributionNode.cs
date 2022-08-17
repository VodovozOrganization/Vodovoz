using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListMileageDistributionNode
	{
		public RouteList RouteList { get; set; }
		//public int Id { get; set; }
		//public string DeliveryShift { get; set; }
		//public string Driver { get; set; }

		public string Id => RouteList?.Id.ToString() ?? "";
		public string DeliveryShift => RouteList?.Shift?.Name ?? "";
		public string Driver => RouteList?.Driver?.FullName ?? "";

		public string Forwarder => RouteList?.Forwarder?.FullName ?? CustomForwarderColumn ?? "";
		public string CustomForwarderColumn { get; set; }

		public decimal? RecalculatedDistance => RouteList?.RecalculatedDistance ?? CustomRecalculatedDistanceColumn;
		public decimal? CustomRecalculatedDistanceColumn { get; set; }

		public decimal ConfirmedDistance
		{
			get
			{
				if(RouteList != null)
				{
					return RouteList.ConfirmedDistance;
				}

				return 0;
			} 
			set
			{
				if(RouteList != null)
				{
					RouteList.ConfirmedDistance = value;
				}
			}
		}

		public string MileageComment
		{
			get => RouteList?.MileageComment;
			set
			{
				if(RouteList != null)
				{
					RouteList.MileageComment = value;
				}
			}
		}
	}
}

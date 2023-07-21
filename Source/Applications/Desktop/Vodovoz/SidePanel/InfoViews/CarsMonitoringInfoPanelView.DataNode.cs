using System;

namespace Vodovoz.SidePanel.InfoViews
{
	public partial class CarsMonitoringInfoPanelView
	{
		sealed class DataNode
		{
			public string DriverName { get; set; }
			public int RouteListId { get; set; }
			public string CarNumber { get; set; }
			public string Address { get; set; }
			public int RouteListItemId { get; set; }
			public bool IsFastDeliveryOrder { get; set; }
			public string DeliveryType { get; set; }
			public DateTime DeliveryBefore { get; set; }
		}
	}
}

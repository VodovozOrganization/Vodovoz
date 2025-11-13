using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl
{
	public class EdoControlReportRow
	{
		public EdoControlReportRow(string groupTitle)
		{
			GroupTitle = groupTitle;
			IsRootRow = true;
		}

		public EdoControlReportRow(EdoControlReportOrderData data)
		{
			OrderId = data.OrderId.ToString();
			DeliveryDate = data.DeliveryDate.ToString("dd.MM.yyyy");
			ClientId = data.ClientId.ToString();
			ClientName = data.ClientName;
			RouteListId = data.RouteListId is null ? "" : data.RouteListId.Value.ToString();
			EdoContainerId = data.EdoContainerId is null ? "" : data.EdoContainerId.Value.ToString();
			EdoStatus = data.EdoStatus.GetEnumDisplayName();
			OrderDeliveryType = data.OrderDeliveryType.GetEnumDisplayName();
			AddressTransferType = data.AddressTransferType.GetEnumDisplayName();
		}

		public string GroupTitle { get; }
		public bool IsRootRow { get; }
		public string OrderId { get; }
		public string DeliveryDate { get; }
		public string ClientId { get; }
		public string ClientName { get; }
		public string RouteListId { get; }
		public string EdoContainerId { get; }
		public string EdoStatus { get; }
		public string OrderDeliveryType { get; }
		public string AddressTransferType { get; }
	}
}

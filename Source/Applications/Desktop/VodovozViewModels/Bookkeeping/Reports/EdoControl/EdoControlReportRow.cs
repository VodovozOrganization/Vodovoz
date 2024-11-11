using System;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Extensions;
using static Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters.IncludeExcludeBookkeepingReportsFilterFactory;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl
{
	public class EdoControlReportData
	{
		public int OrderId { get; set; }
		public DateTime DeliveryDate { get; set; }
		public int ClientId { get; set; }
		public string ClientName { get; set; }
		public int? RouteListId { get; set; }
		public int? EdoContainerId { get; set; }
		public EdoDocFlowStatus? EdoStatus { get; set; }
		public EdoControlReportOrderDeliveryType OrderDeliveryType { get; set; }
		public EdoControlReportAddressTransferType AddressTransferType { get; set; }
	}

	public class EdoControlReportRow
	{
		public EdoControlReportRow(string groupTitle)
		{
			GroupTitle = groupTitle;
			IsRootRow = true;
		}

		public EdoControlReportRow(EdoControlReportData data)
		{
			OrderId = data.OrderId.ToString();
			DeliveryDate = data.DeliveryDate.ToString("dd.MM.yyyy");
			ClientId = data.ClientId.ToString();
			ClientName = data.ClientName;
			RouteListId = data.RouteListId is null ? "" : data.RouteListId.Value.ToString();
			EdoContainerId = data.EdoContainerId is null ? "" : data.EdoContainerId.Value.ToString();
			EdoStatus = data.EdoStatus is null ? "" : data.EdoStatus.Value.GetEnumDisplayName();
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

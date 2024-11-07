using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using static Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters.IncludeExcludeBookkeepingReportsFilterFactory;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl
{
	public partial class EdoControlReport
	{
		private async Task SetReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			try
			{
				Rows = await CreateReportRows(uow, cancellationToken);
			}
			catch(Exception ex)
			{
				throw;
			}
		}

		private async Task<IList<EdoControlReportRow>> CreateReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var rows =
				from order in uow.Session.Query<Order>()
				join client in uow.Session.Query<Counterparty>() on order.Client.Id equals client.Id
				join rli in uow.Session.Query<RouteListItem>() on order.Id equals rli.Order.Id into routeListItems
				from routeListItem in routeListItems.DefaultIfEmpty()
				join ec in uow.Session.Query<EdoContainer>() on order.Id equals ec.Order.Id into edoContainers
				from edoContainer in edoContainers.DefaultIfEmpty()
				where order.DeliveryDate >= StartDate && order.DeliveryDate < EndDate.Date.AddDays(1)
				select new EdoControlReportRow
				{
					EdoContainerId = edoContainer.Id,
					ClientName = client.Name,
					OrderId = order.Id,
					RouteListId = routeListItem.RouteList.Id,
					DeliveryDate = order.DeliveryDate,
					EdoStatus = edoContainer.EdoDocFlowStatus,
					//OrderDeliveryType =
					//	order.IsFastDelivery
					//	? EdoControlReportOrderDeliveryType.FastDelivery
					//	: order.DeliverySchedule.Id == 462
					//		? EdoControlReportOrderDeliveryType.CloseDocument
					//		: EdoControlReportOrderDeliveryType.CommonDelivery,
					//AddressTransferType = 
					//	routeListItem.AddressTransferType == null
					//	? EdoControlReportAddressTransferType.NoTransfer
					//	: (EdoControlReportAddressTransferType)(int)routeListItem.AddressTransferType.Value
				};

			return await rows.ToListAsync();
		}
	}
}

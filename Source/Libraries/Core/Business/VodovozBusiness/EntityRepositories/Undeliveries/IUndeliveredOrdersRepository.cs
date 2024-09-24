using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Undeliveries
{
	public interface IUndeliveredOrdersRepository
	{
		Dictionary<GuiltyTypes, int> GetDictionaryWithUndeliveriesCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		IList<UndeliveredOrderCountNode> GetListOfUndeliveriesCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		IList<UndeliveredOrderCountNode> GetListOfUndeliveriesCountOnDptForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		IList<UndeliveredOrder> GetListOfUndeliveriesForOrder(IUnitOfWork uow, int orderId);
		IList<UndeliveredOrder> GetListOfUndeliveriesForOrder(IUnitOfWork uow, Order order);
		IList<int> GetListOfUndeliveryIdsForDriver(IUnitOfWork uow, Employee driver);
		IList<object[]> GetGuiltyAndCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		decimal GetUndelivered19LBottlesQuantity(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		Order GetOldOrderFromUndeliveredByNewOrderId(IUnitOfWork uow, int newOrderId);
		IQueryable<UndeliveredOrder> GetUndeliveriesForOrders(IUnitOfWork unitOfWork, IList<int> ordersIds);
		IQueryable<OksDailyReportUndeliveredOrderDataNode> GetUndeliveredOrdersForPeriod(IUnitOfWork uow, DateTime startDate, DateTime endDate);
	}

	public class OksDailyReportUndeliveredOrderDataNode
	{
		public int UndeliveredOrderId { get; set; }
		public GuiltyTypes GuiltySide { get; set; }
		public int? GuiltySubdivisionId { get; set; }
		public string GuiltySubdivisionName { get; set; }
		public UndeliveryStatus UndeliveryStatus { get; set; }
		public TransferType? TransferType { get; set; }
		public DateTime? OldOrderDeliveryDate { get; set; }
		public string ClientName { get; set; }
		public string Reason { get; set; }
		public string DriverNames { get; set; }
		public string ResultComments { get; set; }
	}
}

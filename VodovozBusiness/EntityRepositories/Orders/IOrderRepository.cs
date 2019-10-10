using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Repositories.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public interface IOrderRepository
	{
		int Get19LWatterQtyForOrder(IUnitOfWork uow, Domain.Orders.Order order);
		IList<Domain.Orders.Order> GetAcceptedOrdersForRegion(IUnitOfWork uow, DateTime date, ScheduleRestrictedDistrict district);
		IList<RouteList> GetAllRLForOrder(IUnitOfWork UoW, Domain.Orders.Order order);
		IList<Domain.Orders.Order> GetCurrentOrders(IUnitOfWork UoW, Counterparty counterparty);
		IList<DiscountReason> GetDiscountReasons(IUnitOfWork UoW, bool orderByDescending = false);
		IList<ClientEquipmentNode> GetEquipmentFromClientForOrder(IUnitOfWork uow, Domain.Orders.Order order);
		IList<ClientEquipmentNode> GetEquipmentToClientForOrder(IUnitOfWork uow, Domain.Orders.Order order);
		Domain.Orders.Order GetFirstRealOrderForClientForActionBottle(IUnitOfWork uow, Counterparty counterparty);
		OrderStatus[] GetGrantedStatusesToCreateSeveralOrders();
		Domain.Orders.Order GetLatestCompleteOrderForCounterparty(IUnitOfWork UoW, Counterparty counterparty);
		IList<Domain.Orders.Order> GetLatestOrdersForDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, int? count = null);
		OrderStatus[] GetOnClosingOrderStatuses();
		Domain.Orders.Order GetOrderOnDateAndDeliveryPoint(IUnitOfWork uow, DateTime date, DeliveryPoint deliveryPoint);
		IList<Domain.Orders.Order> GetOrdersBetweenDates(IUnitOfWork UoW, DateTime startDate, DateTime endDate);
		IList<Domain.Orders.Order> GetOrdersByCode1c(IUnitOfWork uow, string[] codes1c);
		QueryOver<Domain.Orders.Order> GetOrdersForRLEditingQuery(DateTime date, bool showShipped);
		IList<Domain.Orders.Order> GetOrdersToExport1c8(IUnitOfWork UoW, Export1cMode mode, DateTime startDate, DateTime endDate);
		QueryOver<Domain.Orders.Order> GetSelfDeliveryOrdersForPaymentQuery();
		OrderStatus[] GetStatusesForActualCount(Domain.Orders.Order order);
		OrderStatus[] GetStatusesForOrderCancelation();
		OrderStatus[] GetValidStatusesToUseActionBottle();
		bool IsBottleStockExists(IUnitOfWork uow, Counterparty counterparty);
		double GetAvgRandeBetwenOrders(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime? startDate = null, DateTime? endDate = null);
		double GetAvgRandeBetwenOrders(IUnitOfWork uow, DeliveryPoint deliveryPoint, out int? orderCount, DateTime? startDate = null, DateTime? endDate = null);
		OrderStatus[] GetUndeliveryStatuses();
		int[] GetShippeIdsStartingFromDate(IUnitOfWork uow, PaymentType paymentType, DateTime? startDate = null);
	}

	public class ClientEquipmentNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public int Count { get; set; }
	}
}
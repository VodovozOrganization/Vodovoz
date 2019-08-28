using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Repositories.Orders;
using VodovozOrder = Vodovoz.Domain.Orders.Order;


namespace Vodovoz.EntityRepositories.Orders
{
	public class OrderRepository : IOrderRepository
	{
		private static OrderRepository instance;

		public static OrderRepository GetInstance()
		{
			if(instance == null)
				instance = new OrderRepository();
			return instance;
		}

		protected OrderRepository() { }

		public QueryOver<VodovozOrder> GetSelfDeliveryOrdersForPaymentQuery()
		{
			return QueryOver.Of<VodovozOrder>()
			.Where(x => x.SelfDelivery)
			.Where(x => x.OrderStatus == OrderStatus.WaitForPayment);
		}

		public QueryOver<VodovozOrder> GetOrdersForRLEditingQuery(DateTime date, bool showShipped)
		{
			var query = QueryOver.Of<VodovozOrder>();
			if(!showShipped)
				query.Where(order => order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList);
			else
				query.Where(order => order.OrderStatus != OrderStatus.Canceled && order.OrderStatus != OrderStatus.NewOrder && order.OrderStatus != OrderStatus.WaitForPayment);
			return query.Where(order => order.DeliveryDate == date.Date && !order.SelfDelivery && !order.IsService);
		}

		public IList<VodovozOrder> GetAcceptedOrdersForRegion(IUnitOfWork uow, DateTime date, ScheduleRestrictedDistrict district)
		{
			DeliveryPoint point = null;
			return uow.Session.QueryOver<VodovozOrder>()
							  .JoinAlias(o => o.DeliveryPoint, () => point)
							  .Where(
							  		o => o.DeliveryDate == date.Date
									&& point.District.Id == district.Id
									&& !o.SelfDelivery
									&& o.OrderStatus == OrderStatus.Accepted
							  )
							  .List<VodovozOrder>()
							  ;
		}

		public VodovozOrder GetLatestCompleteOrderForCounterparty(IUnitOfWork UoW, Counterparty counterparty)
		{
			VodovozOrder orderAlias = null;
			var queryResult = UoW.Session.QueryOver(() => orderAlias)
				.Where(() => orderAlias.Client.Id == counterparty.Id)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.OrderBy(() => orderAlias.Id).Desc
				.Take(1).List();
			return queryResult.FirstOrDefault();
		}

		public IList<VodovozOrder> GetCurrentOrders(IUnitOfWork UoW, Counterparty counterparty)
		{
			VodovozOrder orderAlias = null;
			return UoW.Session.QueryOver(() => orderAlias)
				.Where(() => orderAlias.Client.Id == counterparty.Id)
				.Where(() => orderAlias.DeliveryDate >= DateTime.Today)
				.Where(() => orderAlias.OrderStatus != OrderStatus.Closed
					&& orderAlias.OrderStatus != OrderStatus.Canceled
					&& orderAlias.OrderStatus != OrderStatus.DeliveryCanceled
					&& orderAlias.OrderStatus != OrderStatus.NotDelivered)
				.List();
		}

		public IList<VodovozOrder> GetOrdersToExport1c8(IUnitOfWork UoW, Export1cMode mode, DateTime startDate, DateTime endDate)
		{
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;

			var export1cSubquerySum = QueryOver.Of(() => orderItemAlias)
									   .Where(() => orderItemAlias.Order.Id == orderAlias.Id)
									   .Select(Projections.Sum(
										   Projections.SqlFunction(new VarArgsSQLFunction("", " * ", ""),
																   NHibernateUtil.Decimal,
																   Projections.Conditional(
																	   Restrictions.IsNotNull(Projections.Property<OrderItem>(x => x.ActualCount)),
																	   Projections.Property<OrderItem>(x => x.ActualCount),
																	   Projections.Property<OrderItem>(x => x.Count)
																	  ),
																   Projections.Property<OrderItem>(x => x.Price),
																   Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "( 1 - ?1 / 100 )"),
																						   NHibernateUtil.Decimal,
																						   Projections.Property<OrderItem>(x => x.Discount)
																						  )
																  )
										  ))
									   ;

			var query = UoW.Session.QueryOver(() => orderAlias)
					  .Where(() => orderAlias.OrderStatus.IsIn(VodovozOrder.StatusesToExport1c))
					  .Where(() => startDate <= orderAlias.DeliveryDate && orderAlias.DeliveryDate <= endDate)
					  .Where(Subqueries.Le(0.01, export1cSubquerySum.DetachedCriteria));
			if(mode == Export1cMode.IPForTinkoff) {
				query.Where(o => o.PaymentType == PaymentType.ByCard)
					.Where(o => o.OnlineOrder != null);
			} else {
				query.Where(o => o.PaymentType == PaymentType.cashless);
			}

			return query.List();
		}

		public IList<VodovozOrder> GetOrdersBetweenDates(IUnitOfWork UoW, DateTime startDate, DateTime endDate)
		{
			VodovozOrder orderAlias = null;
			return UoW.Session.QueryOver(() => orderAlias)
				.Where(() => startDate <= orderAlias.DeliveryDate && orderAlias.DeliveryDate <= endDate).List();
		}

		public IList<VodovozOrder> GetOrdersByCode1c(IUnitOfWork uow, string[] codes1c)
		{
			return uow.Session.QueryOver<VodovozOrder>()
				.Where(c => c.Code1c.IsIn(codes1c))
				.List<VodovozOrder>();
		}

		internal static Func<IUnitOfWork, Counterparty, VodovozOrder> GetFirstRealOrderForClientTestGap;
		/// <summary>
		/// Первый заказ контрагента, который можно считать выполненым.
		/// </summary>
		/// <returns>Первый заказ</returns>
		/// <param name="uow">UoW</param>
		/// <param name="counterparty">Контрагент</param>
		public VodovozOrder GetFirstRealOrderForClient(IUnitOfWork uow, Counterparty counterparty)
		{
			if(counterparty?.FirstOrder != null && GetValidStatusesToUseActionBottle().Contains(counterparty.FirstOrder.OrderStatus))
				return counterparty.FirstOrder;

			if(GetFirstRealOrderForClientTestGap != null)
				return GetFirstRealOrderForClientTestGap(uow, counterparty);

			var query = uow.Session.QueryOver<VodovozOrder>()
						   .Where(o => o.Id > 0)
						   .Where(o => o.Client == counterparty)
						   .Where(o => o.OrderStatus.IsIn(GetValidStatusesToUseActionBottle()))
						   .OrderBy(o => o.DeliveryDate).Asc
						   .Take(1)
						   ;
			return query.List().FirstOrDefault();
		}

		/// <summary>
		/// Кол-во 19л. воды в заказе
		/// </summary>
		/// <returns>Кол-во 19л. воды в заказе</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="order">Заказ</param>
		public int Get19LWatterQtyForOrder(IUnitOfWork uow, VodovozOrder order)
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			var _19LWatterQty = uow.Session.QueryOver(() => orderItemAlias)
										  .Where(() => orderItemAlias.Order.Id == order.Id)
										  .Left.JoinQueryOver(i => i.Nomenclature, () => nomenclatureAlias)
										  .Where(n => n.Category == NomenclatureCategory.water && n.TareVolume == TareVolume.Vol19L)
										  .List()
										  .Sum(i => i.Count);
			return _19LWatterQty;
		}

		/// <summary>
		/// Оборудование заказа к клиенту
		/// </summary>
		/// <returns>Список оборудования к клиенту</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="order">Заказ</param>
		public IList<ClientEquipmentNode> GetEquipmentToClientForOrder(IUnitOfWork uow, VodovozOrder order)
		{
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
			ClientEquipmentNode resultAlias = null;

			var equipToClient = uow.Session.QueryOver(() => orderEquipmentAlias)
								   .Where(() => orderEquipmentAlias.Order.Id == order.Id)
								   .Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
								   .Left.JoinQueryOver(i => i.Nomenclature, () => nomenclatureAlias)
								   .SelectList(list => list
											   .Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
											   .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
											   .Select(() => nomenclatureAlias.ShortName).WithAlias(() => resultAlias.ShortName)
											   .Select(() => orderEquipmentAlias.Count).WithAlias(() => resultAlias.Count)
									  )
								   .TransformUsing(Transformers.AliasToBean<ClientEquipmentNode>())
								   .List<ClientEquipmentNode>();
			return equipToClient;
		}

		/// <summary>
		/// Оборудование заказа от клиента
		/// </summary>
		/// <returns>Список оборудования от клиенту</returns>
		/// <param name="uow">Uow.</param>
		/// <param name="order">Заказ</param>
		public IList<ClientEquipmentNode> GetEquipmentFromClientForOrder(IUnitOfWork uow, VodovozOrder order)
		{
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
			ClientEquipmentNode resultAlias = null;

			var equipFromClient = uow.Session.QueryOver(() => orderEquipmentAlias)
								   .Where(() => orderEquipmentAlias.Order.Id == order.Id)
								   .Where(() => orderEquipmentAlias.Direction == Direction.PickUp)
								   .Left.JoinQueryOver(i => i.Nomenclature, () => nomenclatureAlias)
								   .SelectList(list => list
											   .Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
											   .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
											   .Select(() => nomenclatureAlias.ShortName).WithAlias(() => resultAlias.ShortName)
											   .Select(() => orderEquipmentAlias.Count).WithAlias(() => resultAlias.Count)
									  )
								   .TransformUsing(Transformers.AliasToBean<ClientEquipmentNode>())
								   .List<ClientEquipmentNode>();
			return equipFromClient;
		}

		/// <summary>
		/// Список последних заказов для точки доставки.
		/// </summary>
		/// <returns>Список последних заказов для точки доставки.</returns>
		/// <param name="UoW">IUnitOfWork</param>
		/// <param name="deliveryPoint">Точка доставки.</param>
		/// <param name="count">Требуемое количество последних заказов.</param>
		public IList<VodovozOrder> GetLatestOrdersForDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, int? count = null)
		{
			VodovozOrder orderAlias = null;
			var queryResult = UoW.Session.QueryOver(() => orderAlias)
				.Where(() => orderAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.OrderBy(() => orderAlias.Id).Desc;
			if(count != null)
				return queryResult.Take(count.Value).List();
			else
				return queryResult.List();
		}

		/// <summary>
		/// Список МЛ для заказа, отсортированный в порядке владения этим заказом, в случае переносов
		/// </summary>
		/// <returns>Список МЛ</returns>
		/// <param name="UoW">UoW</param>
		/// <param name="order">Заказ</param>
		public IList<RouteList> GetAllRLForOrder(IUnitOfWork UoW, VodovozOrder order)
		{
			var query = UoW.Session.QueryOver<RouteListItem>()
						   .Where(i => i.Order == order)
						   .OrderBy(i => i.Id).Desc
						   .Select(i => i.RouteList)
						   .List<RouteList>();
			return query;
		}

		/// <summary>
		/// Возврат отсортированного списка скидок
		/// </summary>
		/// <returns>Список скидок</returns>
		/// <param name="UoW">UoW</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию.</param>
		public IList<DiscountReason> GetDiscountReasons(IUnitOfWork UoW, bool orderByDescending = false)
		{
			var query = UoW.Session.QueryOver<DiscountReason>()
						   .OrderBy(i => i.Name);
			return orderByDescending ? query.Desc().List() : query.Asc().List();
		}

		public VodovozOrder GetOrderOnDateAndDeliveryPoint(IUnitOfWork uow, DateTime date, DeliveryPoint deliveryPoint)
		{
			var notSupportedStatuses = new OrderStatus[] {
				OrderStatus.NewOrder,
				OrderStatus.Canceled,
				OrderStatus.NotDelivered
			};

			return uow.Session.QueryOver<VodovozOrder>()
					  .WhereRestrictionOn(x => x.OrderStatus).Not.IsIn(notSupportedStatuses)
					  .Where(x => x.DeliveryDate == date)
					  .Where(x => x.DeliveryPoint.Id == deliveryPoint.Id)
					  .List().FirstOrDefault();
		}

		public bool IsBottleStockExists(IUnitOfWork uow, Counterparty counterparty)
		{
			var stockBottleOrder = uow.Session.QueryOver<VodovozOrder>()
				.Where(x => x.IsBottleStock)
				.And(x => x.Client.Id == counterparty.Id)
				.Take(1)
				.SingleOrDefault();

			return stockBottleOrder != null;
		}

		public double GetAvgRandeBetwenOrders(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime? startDate = null, DateTime? endDate = null)
		{
			VodovozOrder orderAlias = null;

			var orderQuery = uow.Session.QueryOver(() => orderAlias)
					.Where(() => orderAlias.DeliveryPoint.Id == deliveryPoint.Id);

			if(startDate.HasValue)
				orderQuery = orderQuery.Where(() => orderAlias.DeliveryDate >= startDate);
			else
				orderQuery = orderQuery.Where(() => orderAlias.DeliveryDate >= DateTime.Now.AddMonths(-3));

			if(endDate.HasValue)
				orderQuery = orderQuery.Where(() => orderAlias.DeliveryDate <= endDate.Value);

			IList<VodovozOrder> orders = orderQuery.List();

			IList<int> dateDif = new List<int>();
			for(int i = 1; i < orders.Count; i++) {
				int dif = DateTime.Compare(orders[i].DeliveryDate.Value, orders[i - 1].DeliveryDate.Value);
				dateDif.Add(dif);
			}

			if(dateDif?.FirstOrDefault() == null)
				return 0f;
			else
				return dateDif.Average();

		}

		public OrderStatus[] GetOnClosingOrderStatuses()
		{
			return new OrderStatus[] {
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};
		}

		public OrderStatus[] GetStatusesForOrderCancelation()
		{
			return new OrderStatus[] {
				OrderStatus.NewOrder,
				OrderStatus.WaitForPayment,
				OrderStatus.Accepted,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock
			};
		}

		public OrderStatus[] GetStatusesForActualCount(VodovozOrder order)
		{
			if(order.SelfDelivery) {
				return new OrderStatus[0];
			} else {
				return new OrderStatus[]{
					OrderStatus.Canceled,
					OrderStatus.Closed,
					OrderStatus.DeliveryCanceled,
					OrderStatus.NotDelivered,
					OrderStatus.Shipped,
					OrderStatus.UnloadingOnStock
				};
			}
		}

		public OrderStatus[] GetGrantedStatusesToCreateSeveralOrders()
		{
			return new OrderStatus[]{
				OrderStatus.Canceled,
				OrderStatus.NewOrder,
				OrderStatus.DeliveryCanceled,
				OrderStatus.NotDelivered,
				OrderStatus.WaitForPayment
			};
		}

		public OrderStatus[] GetValidStatusesToUseActionBottle()
		{
			return new OrderStatus[]{
				OrderStatus.Accepted,
				OrderStatus.Closed,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.WaitForPayment
			};
		}
	}
}

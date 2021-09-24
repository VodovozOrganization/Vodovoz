using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.Domain.Sale;
using Vodovoz.Services;
using Order = NHibernate.Criterion.Order;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Orders
{
	public class OrderRepository : IOrderRepository
	{
		public QueryOver<VodovozOrder> GetSelfDeliveryOrdersForPaymentQuery()
		{
			return QueryOver.Of<VodovozOrder>()
			.Where(x => x.SelfDelivery)
			.Where(x => x.OrderStatus == OrderStatus.WaitForPayment);
		}

		public QueryOver<VodovozOrder> GetOrdersForRLEditingQuery(DateTime date, bool showShipped)
		{
			var query = QueryOver.Of<VodovozOrder>().Where(order => order.DeliveryDate == date.Date && !order.SelfDelivery)
													.Where(o => o.DeliverySchedule != null)
													.Where(x => x.DeliveryPoint != null)
													;
			if(!showShipped)
				query.Where(order => order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList);
			else
				query.Where(order => order.OrderStatus != OrderStatus.Canceled && order.OrderStatus != OrderStatus.NewOrder && order.OrderStatus != OrderStatus.WaitForPayment);
			return query;
		}

		public IList<VodovozOrder> GetAcceptedOrdersForRegion(IUnitOfWork uow, DateTime date, District district)
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

		public IList<VodovozOrder> GetCounterpartyOrders(IUnitOfWork UoW, Counterparty counterparty)
		{
			VodovozOrder orderAlias = null;
			return UoW.Session.QueryOver(() => orderAlias)
				.Where(() => orderAlias.Client.Id == counterparty.Id)
				.List();
		}

		public IList<VodovozOrder> GetOrdersToExport1c8(
			IUnitOfWork uow,
			IOrderParametersProvider orderParametersProvider, 
			Export1cMode mode,
			DateTime startDate,
			DateTime endDate,
			int? organizationId = null)
		{
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;

			var export1CSubquerySum = QueryOver.Of(() => orderItemAlias)
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

			var query = uow.Session.QueryOver(() => orderAlias)
				.Where(() => orderAlias.OrderStatus.IsIn(VodovozOrder.StatusesToExport1c))
				.Where(() => startDate <= orderAlias.DeliveryDate && orderAlias.DeliveryDate <= endDate);

			if(organizationId.HasValue) {
				CounterpartyContract counterpartyContractAlias = null;

				query.Left.JoinAlias(() => orderAlias.Contract, () => counterpartyContractAlias)
					.Where(() => counterpartyContractAlias.Organization.Id == organizationId);
			}

			switch(mode) {
				case Export1cMode.BuhgalteriaOOO:
					query.Where(o => o.PaymentType == PaymentType.cashless)
						.And(Subqueries.Le(0.01, export1CSubquerySum.DetachedCriteria));
					break;
				case Export1cMode.BuhgalteriaOOONew:
					CashReceipt cashReceiptAlias = null;

					query.JoinEntityAlias(() => cashReceiptAlias, () => cashReceiptAlias.Order.Id == orderAlias.Id, JoinType.LeftOuterJoin)
						.Where(Restrictions.Disjunction()
							.Add(() => orderAlias.PaymentType == PaymentType.cashless)
							.Add(Restrictions.Conjunction()
								.Add(Restrictions.On(() => orderAlias.PaymentType)
									.IsIn(new[] { PaymentType.Terminal, PaymentType.cash }))
								.Add(Restrictions.IsNotNull(Projections.Property(() => cashReceiptAlias.Id))))
							.Add(Restrictions.Conjunction()
								.Add(() => orderAlias.PaymentType == PaymentType.ByCard)
								.Add(Restrictions.Disjunction()
									.Add(Restrictions.On(() => orderAlias.PaymentByCardFrom.Id)
										.IsIn(orderParametersProvider.PaymentsByCardFromNotToSendSalesReceipts))
									.Add(Restrictions.IsNotNull(Projections.Property(() => cashReceiptAlias.Id))))
							)
						);
					break;
				case Export1cMode.IPForTinkoff:
					query.Where(o => o.PaymentType == PaymentType.ByCard)
						.And(o => o.OnlineOrder != null)
						.And(Subqueries.Le(0.01, export1CSubquerySum.DetachedCriteria));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
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

		/// <summary>
		/// Первый заказ контрагента, который можно считать выполненым.
		/// </summary>
		/// <returns>Первый заказ</returns>
		/// <param name="uow">UoW</param>
		/// <param name="counterparty">Контрагент</param>
		public VodovozOrder GetFirstRealOrderForClientForActionBottle(IUnitOfWork uow, VodovozOrder order,Counterparty counterparty)
		{
			if(uow == null) 
				throw new ArgumentNullException(nameof(uow));
			if(order == null) 
				throw new ArgumentNullException(nameof(order));
			if(counterparty == null) 
				throw new ArgumentNullException(nameof(counterparty));
			

			if(counterparty?.FirstOrder != null && GetValidStatusesToUseActionBottle().Contains(counterparty.FirstOrder.OrderStatus))
				return counterparty.FirstOrder;

			var query = uow.Session.QueryOver<VodovozOrder>()
						   .Where(o => o.Id != order.Id)
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
			return (int)_19LWatterQty;
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
		/// Список последних заказов для контрагента .
		/// </summary>
		/// <returns>Список последних заказов для контрагента.</returns>
		/// <param name="UoW">IUnitOfWork</param>
		/// <param name="client">Контрагент.</param>
		/// <param name="count">Требуемое количество последних заказов.</param>
		public IList<Domain.Orders.Order> GetLatestOrdersForCounterparty(IUnitOfWork UoW, Counterparty client, int? count = null)
		{
			VodovozOrder orderAlias = null;
			var queryResult = UoW.Session.QueryOver(() => orderAlias)
				.Where(() => orderAlias.Client == client)
				.OrderBy(() => orderAlias.DeliveryDate).Desc;
			if(count != null)
				return queryResult.Take(count.Value).List();
			else
				return queryResult.List();
		}
		
		/// <summary>
		/// Проверка возможности изменения даты контракта при изменении даты доставки заказа.
		/// Если дата первого заказа меньше newDeliveryDate и это - текущий изменяемый заказ - возвращает True.
		/// Если первый заказ меньше newDeliveryDate и он не является текущим заказом - возвращает False.
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="client">Поиск заказов по этому контрагенту</param>
		/// <param name="newDeliveryDate">Новая дата доставки заказа</param>
		/// <param name="orderId">Текущий изменяемый заказ</param>
		/// <returns>Возможность смены даты контракта</returns>
		public bool CanChangeContractDate(IUnitOfWork uow, Counterparty client, DateTime newDeliveryDate, int orderId)
		{
			VodovozOrder orderAlias = null;
			
			var result = uow.Session.QueryOver(() => orderAlias)
				.Where(() => orderAlias.Client == client)
				.OrderBy(() => orderAlias.DeliveryDate).Asc
				.List().FirstOrDefault();
			if(result.DeliveryDate < newDeliveryDate && result.Id != orderId)
			{
				return false;
			}
			return true;
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

		public Dictionary<int, IEnumerable<int>> GetAllRouteListsForOrders(IUnitOfWork UoW, IEnumerable<VodovozOrder> orders)
		{
			VodovozOrder orderAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;

			var rls = UoW.Session.QueryOver<RouteListItem>(() => routeListItemAlias)
							.Left.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
							.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
							.Where(Restrictions.In(Projections.Property(() => routeListItemAlias.Order.Id), orders.Select(x => x.Id).ToArray()))
							.SelectList(list => list
								.Select(() => orderAlias.Id)
								.Select(() => routeListAlias.Id)
							)
							.TransformUsing(Transformers.PassThrough)
							.List<object[]>()
							.GroupBy(x => (int)x[0]).ToDictionary(x => x.Key, x => x.Select(y => (int)y[1]));
			return rls;
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

		public IList<DiscountReason> GetActiveDiscountReasons(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DiscountReason>()
				.WhereNot(dr => dr.IsArchive).OrderBy(dr => dr.Name).Asc().List();
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
		
		public IList<Domain.Orders.Order> GetSameOrderForDateAndDeliveryPoint(IUnitOfWorkFactory uowFactory, DateTime date, DeliveryPoint deliveryPoint)
		{
			var uow = uowFactory.CreateWithoutRoot();
			
			return uow.Session.QueryOver<VodovozOrder>()
				.Where(x => x.DeliveryDate == date)
				.Where(x => x.DeliveryPoint.Id == deliveryPoint.Id)
				.List();
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

		public double GetAvgRangeBetweenOrders(IUnitOfWork uow, DeliveryPoint deliveryPoint, out int? orderCount, DateTime? startDate = null, DateTime? endDate = null)
		{
			VodovozOrder orderAlias = null;

			var orderQuery = uow.Session.QueryOver(() => orderAlias)
					.Where(Restrictions.Eq(
						Projections.Property<VodovozOrder>(x => x.DeliveryPoint.Id),
						deliveryPoint?.Id))
					.And(() => orderAlias.OrderStatus == OrderStatus.Closed);

			if(startDate.HasValue)
				orderQuery = orderQuery.Where(() => orderAlias.DeliveryDate >= startDate);
			else
				orderQuery = orderQuery.Where(() => orderAlias.DeliveryDate >= DateTime.Now.AddMonths(-3));

			if(endDate.HasValue)
				orderQuery = orderQuery.Where(() => orderAlias.DeliveryDate <= endDate.Value);

			IList<VodovozOrder> orders = orderQuery.List();
			orderCount = orders?.Count;

			if(orders?.FirstOrDefault() == null || orders.Count < 3)
				return 0f;

			IList<int> dateDif = new List<int>();
			for(int i = 1; i < orders.Count; i++) {
				int dif = (orders[i].DeliveryDate.Value - orders[i - 1].DeliveryDate.Value).Days;
				dateDif.Add(dif);
			}

			if(dateDif.Any())
				return dateDif.Average();
			else
				return 0f;

		}

		public double GetAvgRandeBetwenOrders(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime? startDate = null, DateTime? endDate = null)
		{
			return GetAvgRangeBetweenOrders(uow, deliveryPoint, out int? orderCount, startDate, endDate);
		}

		public OrderStatus[] GetOnClosingOrderStatuses()
		{
			return new OrderStatus[] {
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};
		}

		public bool IsOrderCloseWithoutDelivery(IUnitOfWork uow, Domain.Orders.Order order)
		{
			if(uow == null) 
				throw new ArgumentNullException(nameof(uow));
			if(order == null)
				throw new ArgumentNullException(nameof(order));

			if(order.OrderStatus != OrderStatus.Closed)
				return false;


			var routeListItem = uow.Session.QueryOver<RouteListItem>()
					.Where(x => x.Order.Id == order.Id)
					.Take(1).List()?.FirstOrDefault();
			if(routeListItem != null)
				return false;

			var selfDeliveryDocument = uow.Session.QueryOver<SelfDeliveryDocument>()
					.Where(x => x.Order.Id == order.Id)
					.Take(1).List()?.FirstOrDefault();
			if(selfDeliveryDocument != null)
				return false;

			return true;
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

		public OrderStatus[] GetUndeliveryStatuses()
		{
			return new OrderStatus[]
				{
					OrderStatus.NotDelivered,
					OrderStatus.DeliveryCanceled,
					OrderStatus.Canceled
				};
		}

		public IEnumerable<ReceiptForOrderNode> GetOrdersForCashReceiptServiceToSend(
			IUnitOfWork uow,
			IOrderParametersProvider orderParametersProvider,
			IOrganizationParametersProvider organizationParametersProvider,
			ISalesReceiptsParametersProvider salesReceiptsParametersProvider,
			DateTime? startDate = null)
		{
			#region Aliases Restrictions Projections

			var paymentByCardFromNotToSendSalesReceipts = orderParametersProvider.PaymentsByCardFromNotToSendSalesReceipts;
			var vodovozSouthOrganizationId = organizationParametersProvider.VodovozSouthOrganizationId;

			ExtendedReceiptForOrderNode extendedReceiptForOrderNodeAlias = null;

			OrderItem orderItemAlias = null;
			VodovozOrder orderAlias = null;
			CashReceipt cashReceiptAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			Counterparty counterpartyAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			Organization organizationAlias = null;

			var orderSumProjection = Projections.Sum(
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.Decimal, "CAST(IFNULL(?1 * ?2 - ?3, 0) AS DECIMAL(14,2))"),
					NHibernateUtil.Decimal,
					Projections.Property(() => orderItemAlias.Count),
					Projections.Property(() => orderItemAlias.Price),
					Projections.Property(() => orderItemAlias.DiscountMoney)
				)
			);

			var orderSumRestriction = Restrictions.Gt(orderSumProjection, 0);

			var alwaysSendOrdersRestriction = Restrictions.Disjunction()
				.Add(() => productGroupAlias.OnlineStore != null)
				.Add(() => counterpartyAlias.AlwaysSendReceitps)
				.Add(() => orderAlias.SelfDelivery)
				.Add(Restrictions.In(Projections.Property(() => orderAlias.PaymentType),
					new[] { PaymentType.ByCard, PaymentType.Terminal }.ToArray()));

			var orderDeliveredStatuses = Restrictions.In(Projections.Property(() => orderAlias.OrderStatus),
				new[] { OrderStatus.Shipped, OrderStatus.UnloadingOnStock }.ToArray());

			var orderPaymentTypesRestriction = Restrictions.In(Projections.Property(() => orderAlias.PaymentType),
				new[] { PaymentType.cash, PaymentType.Terminal, PaymentType.ByCard }.ToArray());

			var paidByCardRestriction = Restrictions.Disjunction()
				.Add(() => orderAlias.PaymentType != PaymentType.ByCard)
				.Add(() => organizationAlias.Id != vodovozSouthOrganizationId)
				.Add(Restrictions.On(() => orderAlias.PaymentByCardFrom.Id)
					.Not.IsIn(paymentByCardFromNotToSendSalesReceipts));

			#endregion

			#region AlwaysSendOrders

			var alwaysSendOrdersQuery = uow.Session.QueryOver<VodovozOrder>(() => orderAlias)
				.JoinEntityAlias(() => cashReceiptAlias, () => cashReceiptAlias.Order.Id == orderAlias.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => orderAlias.Contract, () => counterpartyContractAlias)
				.Left.JoinAlias(() => counterpartyContractAlias.Organization, () => organizationAlias)
				.Where(alwaysSendOrdersRestriction)
				.And(paidByCardRestriction)
				.And(Restrictions.Disjunction()
					.Add(orderDeliveredStatuses)
					.Add(Restrictions.Conjunction()
						.Add(() => orderAlias.SelfDelivery)
						.Add(() => orderAlias.IsSelfDeliveryPaid)))
				.And(orderSumRestriction)
				.And(orderPaymentTypesRestriction)
				.And(Restrictions.Disjunction()
					.Add(Restrictions.IsNull(Projections.Property(() => cashReceiptAlias.Id)))
					.Add(() => !cashReceiptAlias.Sent));

			if(startDate.HasValue)
			{
				alwaysSendOrdersQuery.Where(() => orderAlias.DeliveryDate >= startDate.Value);
			}

			var alwaysSendOrders = alwaysSendOrdersQuery
				.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => extendedReceiptForOrderNodeAlias.OrderId)
					.Select(() => orderAlias.PaymentType).WithAlias(() => extendedReceiptForOrderNodeAlias.PaymentType)
					.Select(orderSumProjection).WithAlias(() => extendedReceiptForOrderNodeAlias.OrderSum)
					.Select(() => cashReceiptAlias.Id).WithAlias(() => extendedReceiptForOrderNodeAlias.ReceiptId)
					.Select(() => cashReceiptAlias.Sent).WithAlias(() => extendedReceiptForOrderNodeAlias.WasSent))
				.TransformUsing(Transformers.AliasToBean<ExtendedReceiptForOrderNode>())
				.Future<ExtendedReceiptForOrderNode>();

			//Здесь фильтрация идёт не на уровне запроса, т.к. не NHibernate упорно не хочет клась сложное условие в HAVING
			var result = alwaysSendOrders
				.Where(x =>
					x.PaymentType != PaymentType.cash
					|| x.PaymentType == PaymentType.cash && x.OrderSum < 20000)
				.Select(x => new ReceiptForOrderNode
				{
					OrderId = x.OrderId,
					ReceiptId = x.ReceiptId,
					WasSent = x.WasSent
				});

			#endregion

			#region UniqueOrderSumOrders

			if(salesReceiptsParametersProvider.SendUniqueOrderSumOrders)
			{
				var uniqueOrderSumSendOrdersQuery = uow.Session.QueryOver<VodovozOrder>(() => orderAlias)
					.JoinEntityAlias(() => cashReceiptAlias, () => cashReceiptAlias.Order.Id == orderAlias.Id, JoinType.LeftOuterJoin)
					.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
					.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
					.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
					.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
					.Left.JoinAlias(() => orderAlias.Contract, () => counterpartyContractAlias)
					.Left.JoinAlias(() => counterpartyContractAlias.Organization, () => organizationAlias)
					.Where(Restrictions.Not(alwaysSendOrdersRestriction))
					.And(paidByCardRestriction)
					.And(orderDeliveredStatuses)
					.And(orderSumRestriction)
					.And(orderPaymentTypesRestriction);

				if(startDate.HasValue)
				{
					uniqueOrderSumSendOrdersQuery.Where(() => orderAlias.DeliveryDate >= startDate.Value);
				}

				var notUniqueOrderSumSendOrdersTemp = uniqueOrderSumSendOrdersQuery
					.SelectList(list => list
						.SelectGroup(() => orderAlias.Id).WithAlias(() => extendedReceiptForOrderNodeAlias.OrderId)
						.Select(() => orderAlias.PaymentType).WithAlias(() => extendedReceiptForOrderNodeAlias.PaymentType)
						.Select(orderSumProjection).WithAlias(() => extendedReceiptForOrderNodeAlias.OrderSum)
						.Select(CustomProjections.Date(() => orderAlias.DeliveryDate))
						.WithAlias(() => extendedReceiptForOrderNodeAlias.DeliveryDate)
						.Select(() => cashReceiptAlias.Id).WithAlias(() => extendedReceiptForOrderNodeAlias.ReceiptId)
						.Select(() => cashReceiptAlias.Sent).WithAlias(() => extendedReceiptForOrderNodeAlias.WasSent))
					.TransformUsing(Transformers.AliasToBean<ExtendedReceiptForOrderNode>())
					.Future<ExtendedReceiptForOrderNode>();

				var notUniqueOrderSumSendOrders = notUniqueOrderSumSendOrdersTemp.Where(x =>
					x.PaymentType != PaymentType.cash
					|| x.PaymentType == PaymentType.cash && x.OrderSum < 20000).ToList();

				var alreadySentOrders =
					new List<ExtendedReceiptForOrderNode>(notUniqueOrderSumSendOrders.Where(x => x.WasSent.HasValue && x.WasSent.Value));
				var uniqueOrderSumSendNodes = new List<ExtendedReceiptForOrderNode>();

				foreach(var node in notUniqueOrderSumSendOrders.Where(x => !x.WasSent.HasValue || !x.WasSent.Value))
				{
					if(alreadySentOrders.All(x => x.OrderSum != node.OrderSum || x.DeliveryDate != node.DeliveryDate)
						&& uniqueOrderSumSendNodes.All(x => x.OrderSum != node.OrderSum || x.DeliveryDate != node.DeliveryDate))
					{
						uniqueOrderSumSendNodes.Add(node);
					}
				}
				var uniqueOrderSumSendOrderNodes = uniqueOrderSumSendNodes.Select(x => new ReceiptForOrderNode
					{ OrderId = x.OrderId, ReceiptId = x.ReceiptId, WasSent = x.WasSent });

				result = result.Union(uniqueOrderSumSendOrderNodes);
			}
			
			#endregion

			return result;
		}

		public SmsPaymentStatus? GetOrderPaymentStatus(IUnitOfWork uow, int orderId)
		{
			SmsPayment smsPaymentAlias = null;

			var orders = uow.Session.QueryOver(() => smsPaymentAlias)
				.Where(() => smsPaymentAlias.Order.Id == orderId)
				.List();
			if(orders.Any(x => x.SmsPaymentStatus == SmsPaymentStatus.Paid)) {
				return SmsPaymentStatus.Paid;
			}

			if(orders.Any(x => x.SmsPaymentStatus == SmsPaymentStatus.WaitingForPayment)) {
				return SmsPaymentStatus.WaitingForPayment;
			}

			if(orders.Any(x => x.SmsPaymentStatus == SmsPaymentStatus.Cancelled)) {
				return SmsPaymentStatus.Cancelled;
			}

			return null;
		}

		public decimal GetCounterpartyDebt(IUnitOfWork uow, int counterpartyId)
		{
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			PaymentItem paymentItemAlias = null;
			CashlessMovementOperation cashlessMovOperationAlias = null;
			
			var total = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Where(() => counterpartyAlias.Id == counterpartyId)
				.And(() => orderAlias.OrderStatus != OrderStatus.NewOrder)
				.And(() => orderAlias.OrderStatus != OrderStatus.Canceled)
				.And(() => orderAlias.OrderStatus != OrderStatus.DeliveryCanceled)
				.And(() => orderAlias.OrderStatus != OrderStatus.NotDelivered)
				.And(() => orderAlias.PaymentType == PaymentType.cashless)
				.And(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid)
				.Select(
					Projections.Sum(
						Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "ROUND(?1 * IFNULL(?2, ?3) - ?4, 2)"),
							NHibernateUtil.Decimal, new IProjection[] {
								Projections.Property(() => orderItemAlias.Price),
								Projections.Property(() => orderItemAlias.ActualCount),
								Projections.Property(() => orderItemAlias.Count),
								Projections.Property(() => orderItemAlias.DiscountMoney)})
					)
				).SingleOrDefault<decimal>();
			
			var totalPayPartiallyPaidOrders = uow.Session.QueryOver(() => paymentItemAlias)
				.Left.JoinAlias(() => paymentItemAlias.CashlessMovementOperation, () => cashlessMovOperationAlias)
				.Left.JoinAlias(() => paymentItemAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Where(() => counterpartyAlias.Id == counterpartyId)
				.And(() => orderAlias.OrderStatus != OrderStatus.NewOrder)
				.And(() => orderAlias.OrderStatus != OrderStatus.Canceled)
				.And(() => orderAlias.OrderStatus != OrderStatus.DeliveryCanceled)
				.And(() => orderAlias.OrderStatus != OrderStatus.NotDelivered)
				.And(() => orderAlias.PaymentType == PaymentType.cashless)
				.And(() => orderAlias.OrderPaymentStatus == OrderPaymentStatus.PartiallyPaid)
				.Select(
					Projections.Sum(() => cashlessMovOperationAlias.Expense)
				).SingleOrDefault<decimal>();

			return total - totalPayPartiallyPaidOrders;
		}

		public IList<PaymentItem> GetPaymentItemsForOrder(IUnitOfWork uow, int orderId)
		{
			var paymentItems = uow.Session.QueryOver<PaymentItem>()
				.Where(x => x.Order.Id == orderId)
				.List();

			return paymentItems;
		}
		
		public bool IsSelfDeliveryOrderWithoutShipment(IUnitOfWork uow, int orderId)
		{
			var selfDeliveryDocument = uow.Session.QueryOver<SelfDeliveryDocument>()
			                              .Where(x => x.Order.Id == orderId)
			                              .Take(1).List()?.FirstOrDefault();
			if(selfDeliveryDocument != null)
				return false;

			return true;
		}

		public bool OrderHasSentReceipt(IUnitOfWork uow, int orderId)
		{
			var receipt = uow.Session.QueryOver<CashReceipt>()
				.Where(x => x.Order.Id == orderId)
				.SingleOrDefault();

			return receipt != null;
		}

		public bool CanAddFlyerToOrder(
			IUnitOfWork uow, IRouteListParametersProvider routeListParametersProvider, int flyerId, int geographicGroupId)
		{
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;
			VodovozOrder orderAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			OrderEquipment orderEquipmentAlias = null;

			var warehouseId = geographicGroupId == routeListParametersProvider.SouthGeographicGroupId
				? routeListParametersProvider.WarehouseSofiiskayaId 
				: routeListParametersProvider.WarehouseParnasId;

			var subqueryAdded = uow.Session.QueryOver(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == flyerId)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.IncomingWarehouse)))
				.Where(o => o.IncomingWarehouse.Id == warehouseId)
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount))
				.SingleOrDefault<decimal>();

			var subqueryRemoved = uow.Session.QueryOver(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == flyerId)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.WriteoffWarehouse)))
				.Where(o => o.WriteoffWarehouse.Id == warehouseId)
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount))
				.SingleOrDefault<decimal>();

			var subqueryReserved = uow.Session.QueryOver(() => orderAlias)
				.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Where(() => orderEquipmentAlias.Nomenclature.Id == flyerId)
				.Where(() => districtAlias.GeographicGroup.Id == geographicGroupId)
				.Where(() => orderAlias.OrderStatus == OrderStatus.NewOrder
							 || orderAlias.OrderStatus == OrderStatus.Accepted
				             || orderAlias.OrderStatus == OrderStatus.InTravelList
				             || orderAlias.OrderStatus == OrderStatus.OnLoading)
				.Select(Projections.Sum(() => orderEquipmentAlias.Count))
				.SingleOrDefault<int>();

			return subqueryAdded - subqueryRemoved - subqueryReserved > 0;
		}

		public IEnumerable<VodovozOrder> GetOrders(IUnitOfWork uow, int[] ids)
        {
			VodovozOrder vodovozOrderAlias = null;
			var query = uow.Session.QueryOver(() => vodovozOrderAlias)
				.Where(
					Restrictions.In(
						Projections.Property(() => vodovozOrderAlias.Id),
						ids
						)
					);

			return query.List();
		}

        public VodovozOrder GetOrder(IUnitOfWork unitOfWork, int orderId)
        {
			return unitOfWork.GetById<VodovozOrder>(orderId);
        }
    }
}

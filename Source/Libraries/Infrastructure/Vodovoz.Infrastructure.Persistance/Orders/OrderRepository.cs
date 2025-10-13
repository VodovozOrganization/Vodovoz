using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Client;
using VodovozBusiness.Domain.Operations;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;
using Order = Vodovoz.Domain.Orders.Order;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Orders
{
	internal sealed class OrderRepository : IOrderRepository
	{
		private readonly IOrganizationSettings _organizationSettings;

		public OrderRepository(IOrganizationSettings organizationSettings)
		{
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
		}

		public QueryOver<VodovozOrder> GetSelfDeliveryOrdersForPaymentQuery()
		{
			return QueryOver.Of<VodovozOrder>()
			.Where(x => x.SelfDelivery)
			.Where(x => x.OrderStatus == OrderStatus.WaitForPayment);
		}

		public QueryOver<VodovozOrder> GetOrdersForRLEditingQuery(
			DateTime date, bool showShipped, VodovozOrder orderBaseAlias = null, bool excludeTrucks = false)
		{
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;

			var query = QueryOver.Of<VodovozOrder>(() => orderBaseAlias)
				.Where(o => o.DeliveryDate == date.Date && !o.SelfDelivery)
				.Where(o => o.DeliverySchedule != null)
				.Where(o => o.DeliveryPoint != null);

			if(!showShipped)
			{
				query.Where(order => order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList);
			}
			else
			{
				query.Where(order => order.OrderStatus != OrderStatus.Canceled
					&& order.OrderStatus != OrderStatus.NewOrder
					&& order.OrderStatus != OrderStatus.WaitForPayment);
			}

			if(excludeTrucks)
			{
				query
					.JoinEntityAlias(
						() => routeListItemAlias,
						() => orderBaseAlias.Id == routeListItemAlias.Order.Id,
						JoinType.LeftOuterJoin)
					.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
					.Left.JoinAlias(() => routeListAlias.Car, () => carAlias)
					.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
					.Where(() => routeListAlias.Id == null
						|| (carModelAlias.CarTypeOfUse != CarTypeOfUse.Truck
							&& carModelAlias.CarTypeOfUse != CarTypeOfUse.Loader))
					.And(() => routeListItemAlias.Id == null || routeListItemAlias.Status != RouteListItemStatus.Transfered);
			}

			return query;
		}

		public IList<VodovozOrder> GetAcceptedOrdersForRegion(IUnitOfWork uow, DateTime date, int districtId)
		{
			DeliveryPoint deliveryPointAlias = null;

			return uow.Session.QueryOver<VodovozOrder>()
				.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				.Where(o => o.DeliveryDate == date.Date)
				.And(() => deliveryPointAlias.District.Id == districtId)
				.And(o => !o.SelfDelivery)
				.And(o => o.OrderStatus == OrderStatus.Accepted)
				.List<VodovozOrder>();
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
			IOrderSettings orderSettings,
			Export1cMode mode,
			DateTime startDate,
			DateTime endDate,
			int? organizationId = null)
		{
			var oldReceiptFromYouKassa = new[] {
				orderSettings.PaymentByCardFromSiteId,
				orderSettings.PaymentByCardFromMobileAppId
			};

			var notRetailPadeTypes = new[]
			{
				PaymentType.Barter,
				PaymentType.Cashless,
				PaymentType.ContractDocumentation
			};

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
				.Where(() => orderAlias.OrderStatus.IsIn(VodovozOrder.StatusesToExport1c));

			if(organizationId.HasValue)
			{
				CounterpartyContract counterpartyContractAlias = null;

				query.Left.JoinAlias(() => orderAlias.Contract, () => counterpartyContractAlias)
					.Where(() => counterpartyContractAlias.Organization.Id == organizationId);
			}

			switch(mode)
			{
				case Export1cMode.BuhgalteriaOOO:
				case Export1cMode.ComplexAutomation:
					query
						.Where(() => startDate <= orderAlias.DeliveryDate && orderAlias.DeliveryDate <= endDate)
						.Where(o => o.PaymentType == PaymentType.Cashless)
						.Where(Subqueries.Le(0.01, export1CSubquerySum.DetachedCriteria));
					break;
				case Export1cMode.Retail:
					AddWithCashReceipOnlyRestrictionsToOrderQuery(query, orderAlias);
					query
						.Where(() => startDate <= orderAlias.DeliveryDate && orderAlias.DeliveryDate <= endDate)
						.WhereRestrictionOn(() => orderAlias.PaymentType).Not.IsIn(notRetailPadeTypes);
					break;
				case Export1cMode.BuhgalteriaOOONew:
					CashReceipt cashReceiptAlias = null;

					query.JoinEntityAlias(() => cashReceiptAlias, () => cashReceiptAlias.Order.Id == orderAlias.Id, JoinType.LeftOuterJoin)
					.Where(
							Restrictions.Ge(
								Projections.SqlFunction("DATE", NHibernateUtil.DateTime,
									Projections.SqlFunction(
										new SQLFunctionTemplate(NHibernateUtil.DateTime, "IFNULL(?1, ?2)"),
										NHibernateUtil.DateTime,
										Projections.Property(() => cashReceiptAlias.FiscalDocumentDate),
										Projections.Property(() => orderAlias.DeliveryDate)
									)
								),
								startDate
							)
						)
						.Where(
							Restrictions.Le(
								Projections.SqlFunction("DATE", NHibernateUtil.DateTime,
									Projections.SqlFunction(
										new SQLFunctionTemplate(NHibernateUtil.DateTime, "IFNULL(?1, ?2)"),
										NHibernateUtil.DateTime,
										Projections.Property(() => cashReceiptAlias.FiscalDocumentDate),
										Projections.Property(() => orderAlias.DeliveryDate)
									)
								),
								endDate
							)
						)
						.Where(Restrictions.Disjunction()
							.Add(() => orderAlias.PaymentType == PaymentType.Cashless)
							.Add(Restrictions.Where(() => cashReceiptAlias.Status == CashReceiptStatus.Sended))
							//Включение в выгрузку старых заказов, на которых нет чеков
							//(с 26.04.2023 16:10 заказы с этими источниками оплаты имеют чеки сформированные через ДВ)
							.Add(Restrictions.Conjunction()
								.Add(Restrictions.On(() => orderAlias.PaymentByCardFrom.Id).IsIn(oldReceiptFromYouKassa))
								.Add(Restrictions.Where(() => orderAlias.CreateDate <= new DateTime(2023, 04, 26, 16, 20, 00)))
							)
						);
					break;
				case Export1cMode.IPForTinkoff:
					query
						.Where(() => startDate <= orderAlias.DeliveryDate && orderAlias.DeliveryDate <= endDate)
						.Where(o => o.PaymentType == PaymentType.PaidOnline)
						.Where(o => o.OnlinePaymentNumber != null)
						.Where(Subqueries.Le(0.01, export1CSubquerySum.DetachedCriteria));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}

			query.TransformUsing(Transformers.DistinctRootEntity);

			return query.List();
		}

		private void AddWithCashReceipOnlyRestrictionsToOrderQuery(IQueryOver<VodovozOrder, VodovozOrder> query, VodovozOrder orderAlias)
		{
			var sendedCashReceiptStatuses = new[]
			{
				FiscalDocumentStatus.WaitForCallback, FiscalDocumentStatus.Printed, FiscalDocumentStatus.Completed
			};
			
			EdoFiscalDocument edoFiscalDocumentAlias = null;
			EdoTask edoTaskAlias = null;
			OrderEdoRequest edoRequestAlias = null;
					
			var subQueryWithCashReceipts = QueryOver.Of(() => edoFiscalDocumentAlias)
				.JoinAlias(() => edoFiscalDocumentAlias.ReceiptEdoTask, () => edoTaskAlias)
				.JoinEntityAlias(() => edoRequestAlias, () => edoTaskAlias.Id == edoRequestAlias.Task.Id)
				.Where(() => edoRequestAlias.Order.Id == orderAlias.Id)
				.WhereRestrictionOn(() => edoFiscalDocumentAlias.Status)
				.IsIn(sendedCashReceiptStatuses)
				.Select(Projections.Property(() => edoFiscalDocumentAlias.Id));
			
				query.WithSubquery.WhereExists(subQueryWithCashReceipts);
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
		public VodovozOrder GetFirstRealOrderForClientForActionBottle(IUnitOfWork uow, VodovozOrder order, Counterparty counterparty)
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

		public bool HasCounterpartyFirstRealOrder(IUnitOfWork uow, Counterparty counterparty)
		{
			if(counterparty is null)
			{
				return false;
			}

			if(counterparty.FirstOrder != null && !GetUndeliveryAndNewStatuses().Contains(counterparty.FirstOrder.OrderStatus))
			{
				return true;
			}

			var query = uow.Session.QueryOver<VodovozOrder>()
					.Where(o => o.Client == counterparty)
					.AndRestrictionOn(o => o.OrderStatus).Not.IsIn(GetUndeliveryAndNewStatuses())
					.OrderBy(o => o.DeliveryDate).Asc
					.Take(1);

			return query.SingleOrDefault() != null;
		}

		public bool HasCounterpartyOtherFirstRealOrder(IUnitOfWork uow, Counterparty counterparty, int orderId)
		{
			if(counterparty.FirstOrder != null
				&& counterparty.FirstOrder.Id != orderId
				&& !GetUndeliveryAndNewStatuses().Contains(counterparty.FirstOrder.OrderStatus))
			{
				return true;
			}

			var query = uow.Session.QueryOver<VodovozOrder>()
				.Where(o => o.Client == counterparty)
				.And(o => o.Id != orderId)
				.AndRestrictionOn(o => o.OrderStatus).Not.IsIn(GetUndeliveryAndNewStatuses())
				.OrderBy(o => o.DeliveryDate).Asc
				.Take(1);

			return query.SingleOrDefault() != null;
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

		public Dictionary<int, IEnumerable<int>> GetAllRouteListsForOrders(IUnitOfWork UoW, IEnumerable<int> orders)
		{
			VodovozOrder orderAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;

			var rls = UoW.Session.QueryOver<RouteListItem>(() => routeListItemAlias)
							.Left.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
							.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
							.Where(Restrictions.In(Projections.Property(() => routeListItemAlias.Order.Id), orders.ToArray()))
							.SelectList(list => list
								.Select(() => orderAlias.Id)
								.Select(() => routeListAlias.Id)
							)
							.TransformUsing(Transformers.PassThrough)
							.List<object[]>()
							.GroupBy(x => (int)x[0]).ToDictionary(x => x.Key, x => x.Select(y => (int)y[1]));
			return rls;
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

		public IList<Domain.Orders.Order> GetSameOrderForDateAndDeliveryPoint(
			IUnitOfWorkFactory uowFactory,
			DateTime date,
			DeliveryPoint deliveryPoint)
		{
			using(var uow = uowFactory.CreateWithoutRoot())
			{
				return uow.Session.QueryOver<VodovozOrder>()
					.Where(x => x.DeliveryDate == date)
					.Where(x => x.DeliveryPoint.Id == deliveryPoint.Id)
					.List();
			}
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
			for(int i = 1; i < orders.Count; i++)
			{
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
				OrderStatus.Shipped,
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

		public OrderStatus[] GetStatusesForOrderCancelationWithCancellation()
		{
			return new OrderStatus[] {
				OrderStatus.NewOrder,
				OrderStatus.WaitForPayment,
				OrderStatus.Accepted,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Canceled,
				OrderStatus.NotDelivered,
				OrderStatus.DeliveryCanceled
			};
		}

		public OrderStatus[] GetStatusesForEditGoodsInOrderInRouteList()
		{
			return new OrderStatus[] {
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.UnloadingOnStock
			};
		}

		public OrderStatus[] GetStatusesForFreeBalanceOperations()
		{
			return new OrderStatus[] {
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
			};
		}

		public OrderStatus[] GetStatusesForActualCount(VodovozOrder order)
		{
			if(order.SelfDelivery)
			{
				return new OrderStatus[0];
			}
			else
			{
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

		public static OrderStatus[] GetUndeliveryAndNewStatuses()
		{
			return new[]
			{
				OrderStatus.NewOrder,
				OrderStatus.NotDelivered,
				OrderStatus.DeliveryCanceled,
				OrderStatus.Canceled
			};
		}

		public static OrderStatus[] GetStatusesForCalculationAlreadyReceivedBottlesCountByReferPromotion()
		{
			return new[]
			{
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};
		}

		public SmsPaymentStatus? GetOrderSmsPaymentStatus(IUnitOfWork uow, int orderId)
		{
			SmsPayment smsPaymentAlias = null;

			var orders = uow.Session.QueryOver(() => smsPaymentAlias)
				.Where(() => smsPaymentAlias.Order.Id == orderId)
				.List();
			if(orders.Any(x => x.SmsPaymentStatus == SmsPaymentStatus.Paid))
			{
				return SmsPaymentStatus.Paid;
			}

			if(orders.Any(x => x.SmsPaymentStatus == SmsPaymentStatus.WaitingForPayment))
			{
				return SmsPaymentStatus.WaitingForPayment;
			}

			if(orders.Any(x => x.SmsPaymentStatus == SmsPaymentStatus.Cancelled))
			{
				return SmsPaymentStatus.Cancelled;
			}

			return null;
		}

		public decimal GetCounterpartyDebt(IUnitOfWork uow, int counterpartyId)
		{
			var notPaidOrdersSum = GetCounterpartyNotFullyPaidOrdersSum(uow, counterpartyId);
			var partiallyPaidOrdersPaymentsSum = GetCounterpartyPartiallyPaidOrdersPaymentsSum(uow, counterpartyId);

			return notPaidOrdersSum - partiallyPaidOrdersPaymentsSum;
		}

		public decimal GetCounterpartyWaitingForPaymentOrdersDebt(IUnitOfWork uow, int counterpartyId)
		{
			var notPaidOrdersSum = GetCounterpartyNotFullyPaidOrdersSum(
				uow,
				counterpartyId,
				includeOrderStatuses: new List<OrderStatus> { OrderStatus.WaitForPayment });

			var partiallyPaidOrdersPaymentsSum = GetCounterpartyPartiallyPaidOrdersPaymentsSum(
				uow,
				counterpartyId,
				includeOrderStatuses: new List<OrderStatus> { OrderStatus.WaitForPayment });

			return notPaidOrdersSum - partiallyPaidOrdersPaymentsSum;
		}

		public decimal GetCounterpartyClosingDocumentsOrdersDebtAndNotWaitingForPayment(IUnitOfWork uow, int counterpartyId, IDeliveryScheduleSettings deliveryScheduleSettings)
		{
			var closingDocumentDeliveryScheduleId = deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId;

			var notPaidOrdersSum = GetCounterpartyNotFullyPaidOrdersSum(
				uow,
				counterpartyId,
				excludeOrderStatuses: new List<OrderStatus> { OrderStatus.WaitForPayment },
				includeDeliveryScheduleIds: new List<int> { closingDocumentDeliveryScheduleId });

			var partiallyPaidOrdersPaymentsSum = GetCounterpartyPartiallyPaidOrdersPaymentsSum(
				uow,
				counterpartyId,
				excludeOrderStatuses: new List<OrderStatus> { OrderStatus.WaitForPayment },
				includeDeliveryScheduleIds: new List<int> { closingDocumentDeliveryScheduleId });

			return notPaidOrdersSum - partiallyPaidOrdersPaymentsSum;
		}

		public decimal GetCounterpartyNotWaitingForPaymentAndNotClosingDocumentsOrdersDebt(
			IUnitOfWork uow, int counterpartyId, IDeliveryScheduleSettings deliveryScheduleSettings)
		{
			var closingDocumentDeliveryScheduleId = deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId;

			var notPaidOrdersSum = GetCounterpartyNotFullyPaidOrdersSum(
				uow,
				counterpartyId,
				excludeOrderStatuses: new List<OrderStatus> { OrderStatus.WaitForPayment },
				excludeDeliveryScheduleIds: new List<int> { closingDocumentDeliveryScheduleId });

			var partiallyPaidOrdersPaymentsSum = GetCounterpartyPartiallyPaidOrdersPaymentsSum(
				uow,
				counterpartyId,
				excludeOrderStatuses: new List<OrderStatus> { OrderStatus.WaitForPayment },
				excludeDeliveryScheduleIds: new List<int> { closingDocumentDeliveryScheduleId });

			return notPaidOrdersSum - partiallyPaidOrdersPaymentsSum;
		}

		private decimal GetCounterpartyNotFullyPaidOrdersSum(
			IUnitOfWork uow,
			int counterpartyId,
			IEnumerable<OrderStatus> includeOrderStatuses = null,
			IEnumerable<int> includeDeliveryScheduleIds = null,
			IEnumerable<OrderStatus> excludeOrderStatuses = null,
			IEnumerable<int> excludeDeliveryScheduleIds = null)
		{
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Where(() => counterpartyAlias.Id == counterpartyId)
				.And(() => orderAlias.PaymentType == PaymentType.Cashless)
				.AndRestrictionOn(() => orderAlias.OrderStatus).Not.IsIn(OrderRepository.GetUndeliveryAndNewStatuses())
				.And(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid);

			if(includeOrderStatuses != null)
			{
				query.Where(Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), includeOrderStatuses.ToArray()));
			}

			if(includeDeliveryScheduleIds != null)
			{
				query.Where(Restrictions.In(
					Projections.Property(() => orderAlias.DeliverySchedule.Id),
					includeDeliveryScheduleIds.ToArray()));
			}

			if(excludeOrderStatuses != null)
			{
				query.Where(Restrictions.Not(
					Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), excludeOrderStatuses.ToArray())));
			}

			if(excludeDeliveryScheduleIds != null)
			{

				query.Where(
					Restrictions.Disjunction()
					.Add(Restrictions.IsNull(Projections.Property(() => orderAlias.DeliverySchedule.Id)))
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => orderAlias.DeliverySchedule.Id), excludeDeliveryScheduleIds.ToArray()))));
			}

			var total = query
				.Select(OrderProjections.GetOrderSumProjection())
				.SingleOrDefault<decimal>();

			return total;
		}

		private decimal GetCounterpartyPartiallyPaidOrdersPaymentsSum(
			IUnitOfWork uow,
			int counterpartyId,
			IEnumerable<OrderStatus> includeOrderStatuses = null,
			IEnumerable<int> includeDeliveryScheduleIds = null,
			IEnumerable<OrderStatus> excludeOrderStatuses = null,
			IEnumerable<int> excludeDeliveryScheduleIds = null)
		{
			VodovozOrder orderAlias = null;
			Counterparty counterpartyAlias = null;
			PaymentItem paymentItemAlias = null;
			CashlessMovementOperation cashlessMovOperationAlias = null;

			var query = uow.Session.QueryOver(() => paymentItemAlias)
				.Left.JoinAlias(() => paymentItemAlias.CashlessMovementOperation, () => cashlessMovOperationAlias)
				.Left.JoinAlias(() => paymentItemAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Where(() => counterpartyAlias.Id == counterpartyId)
				.And(() => orderAlias.PaymentType == PaymentType.Cashless)
				.AndRestrictionOn(() => orderAlias.OrderStatus).Not.IsIn(OrderRepository.GetUndeliveryAndNewStatuses())
				.And(() => orderAlias.OrderPaymentStatus == OrderPaymentStatus.PartiallyPaid)
				.And(() => paymentItemAlias.PaymentItemStatus != AllocationStatus.Cancelled);

			if(includeOrderStatuses != null)
			{
				query.Where(Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), includeOrderStatuses.ToArray()));
			}

			if(includeDeliveryScheduleIds != null)
			{
				query.Where(Restrictions.In(
					Projections.Property(() => orderAlias.DeliverySchedule.Id),
					includeDeliveryScheduleIds.ToArray()));
			}

			if(excludeOrderStatuses != null)
			{
				query.Where(Restrictions.Not(
					Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), excludeOrderStatuses.ToArray())));
			}

			if(excludeDeliveryScheduleIds != null)
			{

				query.Where(
					Restrictions.Disjunction()
					.Add(Restrictions.IsNull(Projections.Property(() => orderAlias.DeliverySchedule.Id)))
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => orderAlias.DeliverySchedule.Id), excludeDeliveryScheduleIds.ToArray()))));
			}

			var totalPaymentsSum = query
				.Select(Projections.Sum(() => cashlessMovOperationAlias.Expense))
				.SingleOrDefault<decimal>();

			return totalPaymentsSum;
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

		public bool OrderHasSentUPD(IUnitOfWork uow, int orderId)
		{
			var upds =
			(from et in uow.Session.Query<EdoTask>()
			 join er in uow.Session.Query<OrderEdoRequest>() on et.Id equals er.Task.Id
			 join tri in uow.Session.Query<TransferEdoRequestIteration>() on et.Id equals tri.OrderEdoTask.Id into transferEdoRequestIterations
			 from transferEdoRequestIteration in transferEdoRequestIterations.DefaultIfEmpty()
			 join ter in uow.Session.Query<TransferEdoRequest>() on transferEdoRequestIteration.Id equals ter.Iteration.Id into transferEdoRequests
			 from transferEdoRequest in transferEdoRequests.DefaultIfEmpty()
			 join tet in uow.Session.Query<TransferEdoTask>() on transferEdoRequest.TransferEdoTask.Id equals tet.Id into transferEdoTasks
			 from transferEdoTask in transferEdoTasks.DefaultIfEmpty()
			 join oed in uow.Session.Query<OrderEdoDocument>() on et.Id equals oed.DocumentTaskId into edoDocuments
			 from edoDocument in edoDocuments.DefaultIfEmpty()
			 where
				 er.Order.Id == orderId
				 && (edoDocument != null)
				 && (edoDocument.Status != EdoDocumentStatus.Cancelled
				 || edoDocument.Status != EdoDocumentStatus.Warning)
				 && er.DocumentType == EdoDocumentType.UPD
			 select
			 et.Id)
				.ToList();

			return upds.Any();
		}

		public bool OrderHasSentReceipt(IUnitOfWork uow, int orderId)
		{
			if(IsReceiptSentOldDocflow(uow, orderId))
			{
				return true;
			}

			return IsReceiptSentNewDocflow(uow, orderId);
		}

		private bool IsReceiptSentOldDocflow(IUnitOfWork uow, int orderId)
		{
			var receipts = uow.Session.QueryOver<CashReceipt>()
				.Where(x => x.Order.Id == orderId)
				.Where(x => x.Status == CashReceiptStatus.Sended)
				.List();

			return receipts.Any();
		}

		private bool IsReceiptSentNewDocflow(IUnitOfWork uow, int orderId)
		{
			var fiscalDocumentStages = new[]
			{
				FiscalDocumentStage.Sent,
				FiscalDocumentStage.Completed
			};

			var receipts =
				(from edoTask in uow.Session.Query<ReceiptEdoTask>()
				 join edoRequest in uow.Session.Query<OrderEdoRequest>() on edoTask.Id equals edoRequest.Task.Id
				 join efd in uow.Session.Query<EdoFiscalDocument>() on edoTask.Id equals efd.ReceiptEdoTask.Id into fiscalDocuments
				 from fiscalDocument in fiscalDocuments.DefaultIfEmpty()
				 join tri in uow.Session.Query<TransferEdoRequestIteration>() on edoTask.Id equals tri.OrderEdoTask.Id into transferEdoRequestIterations
				 from transferEdoRequestIteration in transferEdoRequestIterations.DefaultIfEmpty()
				 join ter in uow.Session.Query<TransferEdoRequest>() on transferEdoRequestIteration.Id equals ter.Iteration.Id into transferEdoRequests
				 from transferEdoRequest in transferEdoRequests.DefaultIfEmpty()
				 join tet in uow.Session.Query<TransferEdoTask>() on transferEdoRequest.TransferEdoTask.Id equals tet.Id into transferEdoTasks
				 from transferEdoTask in transferEdoTasks.DefaultIfEmpty()
				 join ted in uow.Session.Query<TransferEdoDocument>() on transferEdoTask.Id equals ted.TransferTaskId into transferEdoDocuments
				 from transferEdoDocument in transferEdoDocuments.DefaultIfEmpty()
				 where
					 edoRequest.Order.Id == orderId
					 && (transferEdoDocument.Id != null || fiscalDocumentStages.Contains(fiscalDocument.Stage))
				 select
				 edoTask.Id)
				.ToList();

			return receipts.Any();
		}

		public bool HasFlyersOnStock(
			IUnitOfWork uow, IRouteListSettings routeListSettings, int flyerId, int geographicGroupId)
		{
			WarehouseBulkGoodsAccountingOperation operationAlias = null;
			VodovozOrder orderAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			OrderEquipment orderEquipmentAlias = null;

			var warehouseId = geographicGroupId == routeListSettings.SouthGeographicGroupId
				? routeListSettings.WarehouseSofiiskayaId
				: routeListSettings.WarehouseBugriId;

			var subQueryBalance = uow.Session.QueryOver(() => operationAlias)
				.Where(() => operationAlias.Nomenclature.Id == flyerId)
				.Where(o => o.Warehouse.Id == warehouseId)
				.Select(Projections.Sum<WarehouseBulkGoodsAccountingOperation>(o => o.Amount))
				.SingleOrDefault<decimal>();

			var subQueryReserved = uow.Session.QueryOver(() => orderAlias)
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

			return subQueryBalance - subQueryReserved > 0;
		}

		public bool IsMovedToTheNewOrder(IUnitOfWork uow, OrderItem orderItem)
		{
			var movedOrderItems = uow.Session.QueryOver<OrderItem>()
				.Where(o => o.CopiedFromUndelivery.Id == orderItem.Id && o.Id != orderItem.Id)
				.List<OrderItem>();

			return movedOrderItems.Count > 0;
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

		public IList<int> GetUnpaidOrdersIds(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime? startDate,
			DateTime? endDate,
			Organization organization = null)
		{
			VodovozOrder orderAlias = null;
			Organization ourOrganizationAlias = null;
			CounterpartyContract contractAlias = null;
			Organization contractOrganizationAlias = null;

			var defaultOurOrganizationId = _organizationSettings.GetCashlessOrganisationId;

			var query = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OurOrganization, () => ourOrganizationAlias)
				.Left.JoinAlias(() => orderAlias.Contract, () => contractAlias)
				.Left.JoinAlias(() => contractAlias.Organization, () => contractOrganizationAlias)
				.Where(() => orderAlias.Client.Id == counterpartyId)
				.Where(() => orderAlias.DeliveryDate >= startDate && orderAlias.DeliveryDate <= endDate)
				.Where(() => orderAlias.OrderPaymentStatus == OrderPaymentStatus.UnPaid)
				.Where(() => orderAlias.PaymentType == PaymentType.Cashless);

			query.Where(
				Restrictions.Or(
					Restrictions.And(
						Restrictions.IsNotNull(Projections.Property(() => ourOrganizationAlias.Id)),
						Restrictions.Eq(Projections.Property(() => ourOrganizationAlias.Id), organization.Id)
					),
					Restrictions.And(
						Restrictions.IsNotNull(Projections.Property(() => contractOrganizationAlias.Id)),
						Restrictions.Eq(Projections.Property(() => contractOrganizationAlias.Id), organization.Id)
					)
				)
			);

			return query
				.Select(x => x.Id)
				.List<int>();
		}

		public VodovozOrder GetOrder(IUnitOfWork unitOfWork, int orderId)
		{
			return unitOfWork.GetById<VodovozOrder>(orderId);
		}

		public int? GetMaxOrderDailyNumberForDate(IUnitOfWorkFactory uowFactory, DateTime deliveryDate)
		{
			int? dailyNumber;

			using(var uow = uowFactory.CreateWithoutRoot(
				$"Получение максимального ежедневного номера заказа на {deliveryDate}"))
			{
				dailyNumber = uow.Session.QueryOver<VodovozOrder>()
					.Where(o => o.DeliveryDate == deliveryDate)
					.Select(Projections.Max<VodovozOrder>(o => o.DailyNumber))
					.SingleOrDefault<int?>();
			}

			return dailyNumber;
		}

		public DateTime? GetOrderDeliveryDate(IUnitOfWorkFactory uowFactory, int orderId)
		{
			DateTime? deliveryDate;

			using(var uow = uowFactory.CreateWithoutRoot($"Получение даты доставки заказа №{orderId}"))
			{
				deliveryDate = uow.Session.QueryOver<VodovozOrder>()
					.Where(o => o.Id == orderId)
					.Select(o => o.DeliveryDate)
					.SingleOrDefault<DateTime?>();
			}

			return deliveryDate;
		}

		public IList<NotFullyPaidOrderNode> GetAllNotFullyPaidOrdersByClientAndOrg(
			IUnitOfWork uow, int counterpartyId, int organizationId, int closingDocumentDeliveryScheduleId)
		{
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			PaymentItem paymentItemAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			Organization orderOrganizationAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;
			CashlessMovementOperation cashlessMovementOperationAlias = null;
			NotFullyPaidOrderNode resultAlias = null;

			var orderSumProjection = OrderProjections.GetOrderSumProjection();
			var allocatedSumProjection = QueryOver.Of(() => paymentItemAlias)
				.JoinAlias(pi => pi.CashlessMovementOperation, () => cashlessMovementOperationAlias)
				.Where(pi => pi.Order.Id == orderAlias.Id)
				.Where(pi => pi.PaymentItemStatus != AllocationStatus.Cancelled)
				.Select(Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, ?2)"),
					NHibernateUtil.Decimal,
						Projections.Sum(() => cashlessMovementOperationAlias.Expense),
						Projections.Constant(0)));

			return uow.Session.QueryOver(() => orderAlias)
				.Inner.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.Inner.JoinAlias(o => o.Contract, () => counterpartyContractAlias)
				.Inner.JoinAlias(() => counterpartyContractAlias.Organization, () => orderOrganizationAlias)
				.Inner.JoinAlias(o => o.DeliverySchedule, () => deliveryScheduleAlias)
				.Where(() => orderAlias.Client.Id == counterpartyId)
				.And(() => orderOrganizationAlias.Id == organizationId)
				.And(() => orderAlias.OrderStatus == OrderStatus.Shipped
							|| orderAlias.OrderStatus == OrderStatus.UnloadingOnStock
							|| orderAlias.OrderStatus == OrderStatus.Closed)
				.And(() => orderAlias.PaymentType == PaymentType.Cashless)
				.And(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid)
				.And(() => deliveryScheduleAlias.Id != closingDocumentDeliveryScheduleId)
				.SelectList(list =>
					list.SelectGroup(o => o.Id).WithAlias(() => resultAlias.Id)
						.Select(o => o.DeliveryDate).WithAlias(() => resultAlias.OrderDeliveryDate)
						.Select(o => o.CreateDate).WithAlias(() => resultAlias.OrderCreationDate)
						.Select(orderSumProjection).WithAlias(() => resultAlias.OrderSum)
						.SelectSubQuery(allocatedSumProjection).WithAlias(() => resultAlias.AllocatedSum))
				.Where(Restrictions.Gt(orderSumProjection, 0))
				.TransformUsing(Transformers.AliasToBean<NotFullyPaidOrderNode>())
				.OrderBy(o => o.DeliveryDate).Asc
				.OrderBy(o => o.CreateDate).Asc
				.List<NotFullyPaidOrderNode>();
		}

		public PaymentType GetCurrentOrderPaymentTypeInDB(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<VodovozOrder>()
				.Where(o => o.Id == orderId)
				.Select(o => o.PaymentType)
				.SingleOrDefault<PaymentType>();
		}

		public IEnumerable<VodovozOrder> GetCashlessOrdersForEdoSendUpd(
			IUnitOfWork uow, DateTime startDate, int organizationId, int closingDocumentDeliveryScheduleId)
		{
			return GetOrdersForFirstUpdSending(uow, startDate, organizationId, closingDocumentDeliveryScheduleId);
		}

		public IEnumerable<int> GetNewEdoProcessOrders(IUnitOfWork uow, IEnumerable<int> orderIds)
		{
			var query =
				from carLoadDocumentItem in uow.Session.Query<CarLoadDocumentItem>()
				join carLoadDocument in uow.Session.Query<CarLoadDocument>() on carLoadDocumentItem.Document.Id equals carLoadDocument.Id
				join nomenclature in uow.Session.Query<Nomenclature>() on carLoadDocumentItem.Nomenclature.Id equals nomenclature.Id
				join order in uow.Session.Query<Order>() on carLoadDocumentItem.OrderId equals order.Id
				join client in uow.Session.Query<Counterparty>() on order.Client.Id equals client.Id
				join oer in uow.Session.Query<OrderEdoRequest>() on order.Id equals oer.Order.Id into orderEdoRequests
				from orderEdoRequest in orderEdoRequests.DefaultIfEmpty()
				where
					orderIds.Contains((int)carLoadDocumentItem.OrderId)
					&& carLoadDocumentItem.IsIndividualSetForOrder
					&& nomenclature.IsAccountableInTrueMark
					&& (orderEdoRequest != null
						|| (client.IsNewEdoProcessing && orderEdoRequest == null && order.OrderStatus == OrderStatus.OnTheWay))
				select (int)carLoadDocumentItem.OrderId;

			return query.Distinct().ToList();
		}

		public IList<VodovozOrder> GetOrdersForEdoSendBills(IUnitOfWork uow, DateTime startDate, int organizationId, int closingDocumentDeliveryScheduleId)
		{
			Counterparty counterpartyAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			VodovozOrder orderAlias = null;
			EdoContainer edoContainerAlias = null;
			CounterpartyEdoAccount defaultEdoAccountAlias = null;

			var orderStatusesForOrderDocumentCloser = new[] { OrderStatus.Closed, OrderStatus.WaitForPayment };

			var query = uow.Session.QueryOver(() => orderAlias)
				.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinAlias(() => orderAlias.Contract, () => counterpartyContractAlias)
				.JoinEntityAlias(() => edoContainerAlias,
					() => orderAlias.Id == edoContainerAlias.Order.Id && edoContainerAlias.Type == DocumentContainerType.Bill, JoinType.LeftOuterJoin)
				.JoinAlias(() => counterpartyAlias.CounterpartyEdoAccounts,
					() => defaultEdoAccountAlias,
					JoinType.InnerJoin,
					Restrictions.Where(() => defaultEdoAccountAlias.OrganizationId == counterpartyContractAlias.Organization.Id
						&& defaultEdoAccountAlias.IsDefault));

			query.Where(() => orderAlias.DeliveryDate >= startDate && edoContainerAlias.Id == null);

			var orderStatusRestriction = Restrictions.Or(
					Restrictions.NotEqProperty(Projections.Property(() => orderAlias.DeliverySchedule.Id), Projections.Constant(closingDocumentDeliveryScheduleId)),
					Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), orderStatusesForOrderDocumentCloser));

			var prohibitedOrderStatusRestriction = Restrictions.Where(() => orderAlias.OrderStatus != OrderStatus.NewOrder);

			query
				.And(() => orderAlias.PaymentType == PaymentType.Cashless)
				.And(() => counterpartyContractAlias.Organization.Id == organizationId)
				.And(orderStatusRestriction)
				.And(prohibitedOrderStatusRestriction)
				.And(() => counterpartyAlias.NeedSendBillByEdo)
				.And(() => defaultEdoAccountAlias.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
				.AndRestrictionOn(() => orderAlias.OrderStatus).Not.IsIn(GetUndeliveryAndNewStatuses())
				.TransformUsing(Transformers.DistinctRootEntity);
			
			return query.List();
		}

		public IList<EdoContainer> GetPreparingToSendEdoContainers(IUnitOfWork uow, DateTime startDate, int organizationId)
		{
			EdoContainer edoContainerAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			VodovozOrder orderAlais = null;

			var result = uow.Session.QueryOver(() => edoContainerAlias)
				.JoinAlias(() => edoContainerAlias.Order, () => orderAlais)
				.JoinAlias(() => orderAlais.Contract, () => counterpartyContractAlias)
				.Where(() => edoContainerAlias.EdoDocFlowStatus == EdoDocFlowStatus.PreparingToSend)
				.And(() => edoContainerAlias.Created >= startDate)
				.And(() => counterpartyContractAlias.Organization.Id == organizationId)
				.List();

			return result;
		}

		public EdoContainer GetEdoContainerByMainDocumentId(IUnitOfWork uow, string mainDocId)
		{
			return uow.Session.QueryOver<EdoContainer>()
				.Where(x => x.MainDocumentId == mainDocId)
				.SingleOrDefault();
		}

		public EdoContainer GetEdoContainerByDocFlowId(IUnitOfWork uow, Guid? docFlowId)
		{
			if(docFlowId is null)
			{
				return null;
			}

			return uow.Session.QueryOver<EdoContainer>()
				.Where(x => x.DocFlowId == docFlowId)
				.SingleOrDefault();
		}

		public IList<EdoContainer> GetEdoContainersByOrderId(IUnitOfWork uow, int orderId)
		{
			return uow.Session.QueryOver<EdoContainer>()
				.Where(x => x.Order.Id == orderId)
				.List();
		}

		public IEnumerable<Payment> GetOrderPayments(IUnitOfWork uow, int orderId)
		{
			Payment paymentAlias = null;
			PaymentItem paymentItemAlias = null;

			var payments = uow.Session.QueryOver(() => paymentAlias)
				.JoinAlias(p => p.Items, () => paymentItemAlias)
				.Where(() => paymentItemAlias.Order.Id == orderId)
				.And(() => paymentItemAlias.PaymentItemStatus != AllocationStatus.Cancelled)
				.TransformUsing(Transformers.DistinctRootEntity)
				.List<Payment>();

			return payments;
		}

		public bool HasSignedUpdDocumentFromEdo(IUnitOfWork uow, int orderId)
		{
			var result = uow.Session.QueryOver<EdoContainer>()
				.Where(x => x.Order.Id == orderId)
				.And(x => x.Type == DocumentContainerType.Upd)
				.And(x => x.EdoDocFlowStatus == EdoDocFlowStatus.Succeed)
				.Take(1)
				.SingleOrDefault();

			return result != null;
		}

		public IList<VodovozOrder> GetOrdersForTrueMark(IUnitOfWork uow, DateTime? startDate, int organizationId)
		{
			Counterparty counterpartyAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			TrueMarkDocument trueMarkApiDocument = null;
			CounterpartyEdoAccount defaultEdoAccountAlias = null;

			var orderStatuses = new[] { OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };

			var query = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.JoinAlias(o => o.Contract, () => counterpartyContractAlias)
				.JoinAlias(o => counterpartyContractAlias.Organization, () => counterpartyContractAlias)
				.JoinEntityAlias(() => trueMarkApiDocument, () => orderAlias.Id == trueMarkApiDocument.Order.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => counterpartyAlias.CounterpartyEdoAccounts,
					() => defaultEdoAccountAlias,
					JoinType.LeftOuterJoin,
					Restrictions.Where(() => defaultEdoAccountAlias.OrganizationId == counterpartyContractAlias.Organization.Id
						&& defaultEdoAccountAlias.IsDefault));

			var hasGtinNomenclaturesSubQuery = QueryOver.Of(() => orderItemAlias)
					.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
					.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
					.And(() => nomenclatureAlias.IsAccountableInTrueMark)
					.Select(Projections.Id());

			if(startDate.HasValue)
			{
				query.Where(() => orderAlias.DeliveryDate > startDate);
			}

			var result = query.Where(() => counterpartyContractAlias.Organization.Id == organizationId)
				.And(Restrictions.IsNull(Projections.Property(() => trueMarkApiDocument.Id)))
				.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(orderStatuses)
				.WithSubquery.WhereExists(hasGtinNomenclaturesSubQuery)
				.And(Restrictions.Disjunction()
					.Add(Restrictions.Conjunction()
						.Add(() => counterpartyAlias.PersonType == PersonType.legal)
						.Add(() => orderAlias.PaymentType == PaymentType.Cashless)
						.Add(() => counterpartyAlias.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
						.Add(Restrictions.Disjunction()
							.Add(() => defaultEdoAccountAlias.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
							.Add(() => counterpartyAlias.RegistrationInChestnyZnakStatus != RegistrationInChestnyZnakStatus.InProcess
									   && counterpartyAlias.RegistrationInChestnyZnakStatus != RegistrationInChestnyZnakStatus.Registered)))
					.Add(Restrictions.Conjunction()
						.Add(() => orderAlias.PaymentType == PaymentType.Barter)
						.Add(Restrictions.Gt(Projections.Property(() => counterpartyAlias.INN), 0))
					)
				)
				.And(() => orderAlias.PaymentType != PaymentType.ContractDocumentation)
				.And(() => !counterpartyAlias.IsNewEdoProcessing)
				.TransformUsing(Transformers.RootEntity)
				.List();
			return result;
		}

		public IList<VodovozOrder> GetOrdersWithSendErrorsForTrueMarkApi(IUnitOfWork uow, DateTime? startDate, int organizationId)
		{
			Counterparty counterpartyAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			TrueMarkDocument trueMarkApiDocument = null;
			CounterpartyEdoAccount defaultEdoAccountAlias = null;

			var orderStatuses = new[] { OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };

			var query = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.JoinAlias(o => o.Contract, () => counterpartyContractAlias)
				.JoinEntityAlias(() => trueMarkApiDocument, () => orderAlias.Id == trueMarkApiDocument.Order.Id)
				.JoinAlias(() => counterpartyAlias.CounterpartyEdoAccounts,
					() => defaultEdoAccountAlias,
					JoinType.LeftOuterJoin,
					Restrictions.Where(() => defaultEdoAccountAlias.OrganizationId == counterpartyContractAlias.Organization.Id
						&& defaultEdoAccountAlias.IsDefault));

			var hasGtinNomenclaturesSubQuery = QueryOver.Of(() => orderItemAlias)
					.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
					.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
					.And(() => nomenclatureAlias.IsAccountableInTrueMark)
					.Select(Projections.Id());

			if(startDate.HasValue)
			{
				query.Where(() => orderAlias.DeliveryDate > startDate);
			}

			var result = query.Where(() => counterpartyContractAlias.Organization.Id == organizationId)
				.And(() => trueMarkApiDocument.IsSuccess == false)
				.And(() => trueMarkApiDocument.Type == TrueMarkDocument.TrueMarkDocumentType.Withdrawal)
				.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(orderStatuses)
				.WithSubquery.WhereExists(hasGtinNomenclaturesSubQuery)
				.And(Restrictions.Disjunction()
					.Add(Restrictions.Conjunction()
						.Add(() => counterpartyAlias.PersonType == PersonType.legal)
						.Add(() => orderAlias.PaymentType == PaymentType.Cashless)
						.Add(() => counterpartyAlias.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
						.Add(Restrictions.Disjunction()
							.Add(() => defaultEdoAccountAlias.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
							.Add(() => counterpartyAlias.RegistrationInChestnyZnakStatus != RegistrationInChestnyZnakStatus.InProcess
								&& counterpartyAlias.RegistrationInChestnyZnakStatus != RegistrationInChestnyZnakStatus.Registered)))
					.Add(Restrictions.Conjunction()
						.Add(() => orderAlias.PaymentType == PaymentType.Barter)
						.Add(Restrictions.Gt(Projections.Property(() => counterpartyAlias.INN), 0))
					)
				)
				.And(() => orderAlias.PaymentType != PaymentType.ContractDocumentation)
				.And(() => !counterpartyAlias.IsNewEdoProcessing)
				.TransformUsing(Transformers.RootEntity)
				.List();

			return result;
		}

		public decimal GetIsAccountableInTrueMarkOrderItemsCount(IUnitOfWork uow, int orderId)
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			return uow.Session.QueryOver<VodovozOrder>()
				.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(o => o.Id == orderId)
				.And(() => nomenclatureAlias.IsAccountableInTrueMark)
				.Select(Projections.Sum(() => orderItemAlias.Count))
				.SingleOrDefault<decimal>();
		}

		public IList<OrderItem> GetIsAccountableInTrueMarkOrderItems(IUnitOfWork uow, int orderId)
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			return uow.Session.QueryOver(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => orderItemAlias.Order.Id == orderId)
				.And(() => nomenclatureAlias.IsAccountableInTrueMark)
				.List();
		}

		public IList<TrueMarkCancellationDto> GetOrdersForCancellationInTrueMark(IUnitOfWork uow, DateTime startDate, int organizationId)
		{
			Counterparty counterpartyAlias = null;
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			TrueMarkDocument trueMarkApiDocumentAlias = null;
			OrderEdoTrueMarkDocumentsActions orderEdoTrueMarkDocumentsActionsAlias = null;
			Organization organizationAlias = null;
			CounterpartyContract contractAlias = null;
			TrueMarkCancellationDto resultAlias = null;
			CounterpartyEdoAccount defaultEdoAccountAlias = null;

			var orderStatuses = new[] { OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };

			var hasGtinNomenclaturesSubQuery = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => nomenclatureAlias.IsAccountableInTrueMark)
				.Select(Projections.Id());

			var hasCancellationSubquery = QueryOver.Of(() => trueMarkApiDocumentAlias)
				.Where(() => trueMarkApiDocumentAlias.Order.Id == orderAlias.Id)
				.Where(() => trueMarkApiDocumentAlias.Type == TrueMarkDocument.TrueMarkDocumentType.WithdrawalCancellation)
				.Select(Projections.Id());

			var organizationInnSubquery = QueryOver.Of<Organization>()
				.Where(o => o.Id == organizationId)
				.Select(o => o.INN);

			var correctSubquery = QueryOver.Of(() => orderAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.JoinEntityAlias(() => orderEdoTrueMarkDocumentsActionsAlias,
					() => orderAlias.Id == orderEdoTrueMarkDocumentsActionsAlias.Order.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => counterpartyAlias.CounterpartyEdoAccounts,
					() => defaultEdoAccountAlias,
					JoinType.LeftOuterJoin,
					Restrictions.Where(() => defaultEdoAccountAlias.OrganizationId == contractAlias.Organization.Id
						&& defaultEdoAccountAlias.IsDefault))
				.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(orderStatuses)
				.WithSubquery.WhereExists(hasGtinNomenclaturesSubQuery)
				.Where(Restrictions.Disjunction()
					.Add(Restrictions.Conjunction()
						.Add(() => counterpartyAlias.PersonType == PersonType.legal)
						.Add(() => orderAlias.PaymentType == PaymentType.Cashless)
						.Add(() => counterpartyAlias.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
						.Add(Restrictions.Disjunction()
							.Add(() => defaultEdoAccountAlias.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
							.Add(() => counterpartyAlias.RegistrationInChestnyZnakStatus != RegistrationInChestnyZnakStatus.InProcess
									   && counterpartyAlias.RegistrationInChestnyZnakStatus != RegistrationInChestnyZnakStatus.Registered)
							)
						)
					.Add(Restrictions.Conjunction()
						.Add(() => orderAlias.PaymentType == PaymentType.Barter)
						.Add(Restrictions.Gt(Projections.Property(() => counterpartyAlias.INN), 0))
					)
				)
				.Where(() => orderAlias.PaymentType != PaymentType.ContractDocumentation)
				.Where(() => orderAlias.Id == trueMarkApiDocumentAlias.Order.Id)
				.Where(() => orderAlias.DeliveryDate > startDate)
				.Where(() => orderEdoTrueMarkDocumentsActionsAlias.IsNeedToCancelTrueMarkDocument == null || !orderEdoTrueMarkDocumentsActionsAlias.IsNeedToCancelTrueMarkDocument)
				.Where(() => !counterpartyAlias.IsNewEdoProcessing)
				.Select(o => o.Id);

			var result = uow.Session.QueryOver(() => trueMarkApiDocumentAlias)
				.JoinAlias(() => trueMarkApiDocumentAlias.Order, () => orderAlias)
				.JoinAlias(() => trueMarkApiDocumentAlias.Organization, () => organizationAlias)
				.Where(() => orderAlias.DeliveryDate > startDate)
				.Where(() => trueMarkApiDocumentAlias.Organization.Id == organizationId)
				.WithSubquery.WhereNotExists(correctSubquery)
				.WithSubquery.WhereNotExists(hasCancellationSubquery)
				.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => trueMarkApiDocumentAlias.Guid).WithAlias(() => resultAlias.DocGuid)
					.Select(() => organizationAlias.INN).WithAlias(() => resultAlias.OrganizationInn)
				)
				.TransformUsing(Transformers.AliasToBean<TrueMarkCancellationDto>())
				.List<TrueMarkCancellationDto>();

			return result;
		}

		public IEnumerable<OrderDto> GetCounterpartyOrdersFromOnlineOrders(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom)
		{
			var orders =
				from onlineOrder in uow.Session.Query<OnlineOrder>()
				join order in uow.Session.Query<VodovozOrder>()
					on onlineOrder.Id equals order.OnlineOrder.Id
				join deliverySchedule in uow.Session.Query<DeliverySchedule>()
					on order.DeliverySchedule.Id equals deliverySchedule.Id into schedules
				join orderRating in uow.Session.Query<OrderRating>()
					on onlineOrder.Id equals orderRating.OnlineOrder.Id into orderRatings
				from orderRating in orderRatings.DefaultIfEmpty()
				from deliverySchedule in schedules.DefaultIfEmpty()
				where order.Client.Id == counterpartyId
				let address = order.DeliveryPoint != null ? order.DeliveryPoint.ShortAddress : null
				let deliveryPointId = order.DeliveryPoint != null ? order.DeliveryPoint.Id : (int?)null
				let orderStatus =
					order.OrderStatus == OrderStatus.Canceled
					|| order.OrderStatus == OrderStatus.DeliveryCanceled
					|| order.OrderStatus == OrderStatus.NotDelivered
						? ExternalOrderStatus.Canceled
						: order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList
							? ExternalOrderStatus.OrderPerformed
							: order.OrderStatus == OrderStatus.Shipped
							|| order.OrderStatus == OrderStatus.Closed
							|| order.OrderStatus == OrderStatus.UnloadingOnStock
								? ExternalOrderStatus.OrderCompleted
								: order.OrderStatus == OrderStatus.WaitForPayment
									? ExternalOrderStatus.WaitingForPayment
									: order.OrderStatus == OrderStatus.OnTheWay
										? ExternalOrderStatus.OrderDelivering
										: order.OrderStatus == OrderStatus.OnLoading
											? ExternalOrderStatus.OrderCollecting
											: ExternalOrderStatus.OrderProcessing
				
				let ratingAvailable =
					order.CreateDate.HasValue
					&& order.CreateDate >= ratingAvailableFrom
					&& orderRating == null
					&& (orderStatus == ExternalOrderStatus.OrderCompleted
						|| orderStatus == ExternalOrderStatus.Canceled
						|| orderStatus == ExternalOrderStatus.OrderDelivering)
				
				let orderPaymentStatus = order.OnlinePaymentNumber.HasValue
					? OnlineOrderPaymentStatus.Paid
					: OnlineOrderPaymentStatus.UnPaid
					
				let deliveryScheduleString = order.IsFastDelivery
					? DeliverySchedule.FastDelivery
					: deliverySchedule != null
						? deliverySchedule.DeliveryTime
						: null

				select new OrderDto
				{
					OrderId = order.Id,
					OnlineOrderId = onlineOrder.Id,
					OrderStatus = orderStatus,
					//OrderPaymentStatus = orderPaymentStatus, на старте null
					DeliveryDate = order.DeliveryDate.Value,
					CreationDate = order.CreateDate.Value,
					OrderSum = order.OrderSum,
					DeliveryAddress = address,
					DeliverySchedule = deliveryScheduleString,
					RatingValue = orderRating.Rating,
					IsRatingAvailable = ratingAvailable,
					IsNeedPayment = false,
					DeliveryPointId = deliveryPointId
				};

			return orders;
		}
		
		public IEnumerable<OrderDto> GetCounterpartyOrdersWithoutOnlineOrders(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom)
		{
			var orders = from order in uow.Session.Query<VodovozOrder>()
				join deliverySchedule in uow.Session.Query<DeliverySchedule>()
					on order.DeliverySchedule.Id equals deliverySchedule.Id into schedules
				join orderRating in uow.Session.Query<OrderRating>()
					on order.Id equals orderRating.Order.Id into orderRatings
				from orderRating in orderRatings.DefaultIfEmpty()
				from deliverySchedule in schedules.DefaultIfEmpty()
				where order.Client.Id == counterpartyId && order.OnlineOrder == null
				let address = order.DeliveryPoint != null ? order.DeliveryPoint.ShortAddress : null
				let deliveryPointId = order.DeliveryPoint != null ? order.DeliveryPoint.Id : (int?)null
				let orderStatus =
					order.OrderStatus == OrderStatus.Canceled
					|| order.OrderStatus == OrderStatus.DeliveryCanceled
					|| order.OrderStatus == OrderStatus.NotDelivered
						? ExternalOrderStatus.Canceled
						: order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList
							? ExternalOrderStatus.OrderPerformed
							: order.OrderStatus == OrderStatus.Shipped
							|| order.OrderStatus == OrderStatus.Closed
							|| order.OrderStatus == OrderStatus.UnloadingOnStock
								? ExternalOrderStatus.OrderCompleted
								: order.OrderStatus == OrderStatus.WaitForPayment
									? ExternalOrderStatus.WaitingForPayment
									: order.OrderStatus == OrderStatus.OnTheWay
										? ExternalOrderStatus.OrderDelivering
										: order.OrderStatus == OrderStatus.OnLoading
											? ExternalOrderStatus.OrderCollecting
											: ExternalOrderStatus.OrderProcessing
				
				let ratingAvailable =
					order.CreateDate.HasValue
					&& order.CreateDate >= ratingAvailableFrom
					&& orderRating == null
					&& (orderStatus == ExternalOrderStatus.OrderCompleted
						|| orderStatus == ExternalOrderStatus.Canceled
						|| orderStatus == ExternalOrderStatus.OrderDelivering)
				
				let orderPaymentStatus = order.OnlinePaymentNumber.HasValue
					? OnlineOrderPaymentStatus.Paid
					: OnlineOrderPaymentStatus.UnPaid
					
				let deliveryScheduleString = order.IsFastDelivery
					? DeliverySchedule.FastDelivery
					: deliverySchedule != null
						? deliverySchedule.DeliveryTime
						: null

				select new OrderDto
				{
					OrderId = order.Id,
					OnlineOrderId = null,
					OrderStatus = orderStatus,
					//OrderPaymentStatus = orderPaymentStatus, на старте null
					DeliveryDate = order.DeliveryDate.Value,
					CreationDate = order.CreateDate.Value,
					OrderSum = order.OrderSum,
					DeliveryAddress = address,
					DeliverySchedule = deliveryScheduleString,
					RatingValue = orderRating.Rating,
					IsRatingAvailable = ratingAvailable,
					IsNeedPayment = false,
					DeliveryPointId = deliveryPointId
				};

			return orders;
		}

		public IList<OrderOnDayNode> GetOrdersOnDay(IUnitOfWork uow, OrderOnDayFilters orderOnDayFilters)
		{
			//Подзапрос со всем фильтрами заказов, будет использоваться для фильтрации
			//необходимых сущностей, при их загрузке, по требуемым заказам

			//Такое выделение в подазпрос необходимо, потому что загрузка один ко многим
			//сразу по нескольким таблицам невозможна в одном запросе, например
			//нельзя одним запросом получить все OrderItems и OrderEquipmets
			//Это так же относиться просто к join, если хотим получить OrderItems, нельзя
			//джойнить другие таблицы один ко многим

			DeliveryPoint deliveryPointAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;
			District districtAlias = null;
			GeoGroup geographicGroupAlias = null;
			VodovozOrder orderAlias = null;
			Counterparty clientAlias = null;
			CounterpartyContract contractAlias = null;
			CounterpartyEdoAccount defaultEdoAccountAlias = null;

			var mainQuery = QueryOver.Of(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => orderAlias.DeliverySchedule, () => deliveryScheduleAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => clientAlias)
				.Left.JoinAlias(() => orderAlias.Contract, () => contractAlias)
				.Where(() => orderAlias.DeliveryDate == orderOnDayFilters.DateForRouting.Date)
				.Where(() => !orderAlias.SelfDelivery)
				.Where(() => orderAlias.DeliveryPoint != null)
				.Where(() => orderAlias.DeliverySchedule != null)
				.Where(() => !orderAlias.IsContractCloser)
				.Where(() => orderAlias.DeliverySchedule.Id != orderOnDayFilters.ClosingDocumentDeliveryScheduleId);

			if(!orderOnDayFilters.ShowCompleted)
			{
				mainQuery.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(new[] {
					OrderStatus.Accepted,
					OrderStatus.InTravelList
				});
			}
			else
			{
				mainQuery.WhereRestrictionOn(() => orderAlias.OrderStatus).Not.IsIn(new[] {
					OrderStatus.Canceled,
					OrderStatus.NewOrder,
					OrderStatus.WaitForPayment
				});
			}

			var routeListItemCriteria = QueryOver.Of<RouteListItem>()
				.Where(x => x.Order.Id == orderAlias.Id)
				.Select(x => x.Id)
				.Take(1)
				.DetachedCriteria;

			var selfDeliveryDocumentCriteria = QueryOver.Of<SelfDeliveryDocument>()
				.Where(x => x.Order.Id == orderAlias.Id)
				.Select(x => x.Id)
				.Take(1)
				.DetachedCriteria;

			var closedWithoutDeliveryCriterion = Restrictions.Conjunction()
				.Add(Restrictions.Where(() => orderAlias.OrderStatus != OrderStatus.Closed))
				.Add(Subqueries.IsNotNull(routeListItemCriteria))
				.Add(Subqueries.IsNotNull(selfDeliveryDocumentCriteria));

			//Исключаем закрытые без доставки
			mainQuery.WhereNot(closedWithoutDeliveryCriterion);

			if(orderOnDayFilters.GeographicGroupIds.Any())
			{
				mainQuery
					.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
					.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicGroupAlias)
					.WhereRestrictionOn(() => districtAlias.GeographicGroup.Id).IsIn(orderOnDayFilters.GeographicGroupIds);
			}

			if(orderOnDayFilters.FastDeliveryEnabled || orderOnDayFilters.IsCodesScanInWarehouseRequired)
			{
				mainQuery
					.JoinAlias(
						() => clientAlias.CounterpartyEdoAccounts,
						() => defaultEdoAccountAlias,
						JoinType.LeftOuterJoin,
						Restrictions.Where(() => defaultEdoAccountAlias.OrganizationId == contractAlias.Organization.Id
							&& defaultEdoAccountAlias.IsDefault));
				
				var additionalParametersRestriction = Restrictions.Conjunction();

				if(orderOnDayFilters.FastDeliveryEnabled)
				{
					additionalParametersRestriction.Add(() => orderAlias.IsFastDelivery);
				}

				if(orderOnDayFilters.IsCodesScanInWarehouseRequired)
				{
					additionalParametersRestriction.Add(Restrictions.Conjunction()
						.Add(() => orderAlias.PaymentType == PaymentType.Cashless)
						.Add(() => clientAlias.OrderStatusForSendingUpd == OrderStatusForSendingUpd.EnRoute)
						.Add(() => defaultEdoAccountAlias.ConsentForEdoStatus == ConsentForEdoStatus.Agree));
				}

				mainQuery.Where(additionalParametersRestriction);
			}

			mainQuery.WhereRestrictionOn(() => orderAlias.OrderAddressType)
				.IsIn(orderOnDayFilters.OrderAddressTypes.ToArray());

			switch(orderOnDayFilters.DeliveryScheduleType)
			{
				case DeliveryScheduleFilterType.DeliveryStart:
					mainQuery
						.Where(() => deliveryPointAlias.Latitude != null)
						.Where(() => deliveryPointAlias.Longitude != null)
						.Where(Restrictions.Ge(
							Projections.SqlFunction("TIME", NHibernateUtil.Time, Projections.Property(() => deliveryScheduleAlias.From)),
							orderOnDayFilters.DeliveryFromTime.ToString("hh\\:mm\\:ss"))
						)
						.Where(Restrictions.Le(
							Projections.SqlFunction("TIME", NHibernateUtil.Time, Projections.Property(() => deliveryScheduleAlias.From)),
							orderOnDayFilters.DeliveryToTime.ToString("hh\\:mm\\:ss"))
						);
					break;
				case DeliveryScheduleFilterType.DeliveryEnd:
					mainQuery
						.Where(() => deliveryPointAlias.Latitude != null)
						.Where(() => deliveryPointAlias.Longitude != null)
						.Where(Restrictions.Ge(
							Projections.SqlFunction("TIME", NHibernateUtil.Time, Projections.Property(() => deliveryScheduleAlias.To)),
							orderOnDayFilters.DeliveryFromTime.ToString("hh\\:mm\\:ss"))
						)
						.Where(Restrictions.Le(
							Projections.SqlFunction("TIME", NHibernateUtil.Time, Projections.Property(() => deliveryScheduleAlias.To)),
							orderOnDayFilters.DeliveryToTime.ToString("hh\\:mm\\:ss"))
						);
					break;
				case DeliveryScheduleFilterType.DeliveryStartAndEnd:
					mainQuery
						.Where(() => deliveryPointAlias.Latitude != null)
						.Where(() => deliveryPointAlias.Longitude != null)
						.Where(Restrictions.Ge(
							Projections.SqlFunction("TIME", NHibernateUtil.Time, Projections.Property(() => deliveryScheduleAlias.To)),
							orderOnDayFilters.DeliveryFromTime.ToString("hh\\:mm\\:ss"))
						)
						.Where(Restrictions.Le(
							Projections.SqlFunction("TIME", NHibernateUtil.Time, Projections.Property(() => deliveryScheduleAlias.To)),
							orderOnDayFilters.DeliveryToTime.ToString("hh\\:mm\\:ss"))
						)
						.Where(Restrictions.Ge(
							Projections.SqlFunction("TIME", NHibernateUtil.Time, Projections.Property(() => deliveryScheduleAlias.From)),
							orderOnDayFilters.DeliveryFromTime.ToString("hh\\:mm\\:ss"))
						)
						.Where(Restrictions.Le(
							Projections.SqlFunction("TIME", NHibernateUtil.Time, Projections.Property(() => deliveryScheduleAlias.From)),
							orderOnDayFilters.DeliveryToTime.ToString("hh\\:mm\\:ss"))
						);
					break;
				case DeliveryScheduleFilterType.OrderCreateDate:
					mainQuery
						.Where(() => deliveryPointAlias.Latitude != null)
						.Where(() => deliveryPointAlias.Longitude != null)
						.Where(Restrictions.Ge(
							Projections.SqlFunction("TIME", NHibernateUtil.Time, Projections.Property(() => orderAlias.CreateDate)),
							orderOnDayFilters.DeliveryFromTime.ToString("hh\\:mm\\:ss"))
						)
						.Where(Restrictions.Le(
							Projections.SqlFunction("TIME", NHibernateUtil.Time, Projections.Property(() => orderAlias.CreateDate)),
							orderOnDayFilters.DeliveryToTime.ToString("hh\\:mm\\:ss"))
						);
					break;
			}

			mainQuery.Select(Projections.Id());

			//Запросы загрузки необходмых сущностей для работы диалога в части работы с заказами

			VodovozOrder order2Alias = null;
			OrderItem orderItem2Alias = null;
			OrderItem copiedFromUndeliveryOrderItem2Alias = null;

			// 1 
			// Просто сразу грузим все части города, потому что их мало
			uow.Session.QueryOver<GeoGroup>()
				.Future();

			// 2
			// Загружаем заказы и одиночные сущности связанные с заказом
			var ordersQuery = uow.Session.QueryOver(() => order2Alias)
				.WithSubquery.WhereProperty(() => order2Alias.Id).In(mainQuery)
				.Fetch(SelectMode.Fetch, () => order2Alias.DeliveryPoint)
				.Fetch(SelectMode.Fetch, () => order2Alias.DeliveryPoint.District)
				.Fetch(SelectMode.Fetch, () => order2Alias.Contract)
				.Fetch(SelectMode.Fetch, () => order2Alias.Contract.Organization)
				.Fetch(SelectMode.Fetch, () => order2Alias.DeliverySchedule)
				.Fetch(SelectMode.Fetch, () => order2Alias.LogisticsRequirements)
				.Future();

			// 3
			//Загружаем все номенклатуры которые используются в требуемых заказах отдельным запросом,
			//чтобы потом в запросе номер 4 они подставились в OrderItem.Nomenclature,
			//так как ChildFetch по orderItem2Alias загрузит только id номенклатуры
			uow.Session.QueryOver(() => orderItem2Alias)
				.WithSubquery.WhereProperty(() => orderItem2Alias.Order.Id).In(mainQuery)
				.Fetch(SelectMode.Fetch, () => orderItem2Alias.Nomenclature)
				.Fetch(SelectMode.Fetch, () => orderItem2Alias.CopiedFromUndelivery)
				.Future();

			// 4
			//Загружаем все OrderItems для требуемых заказов
			uow.Session.QueryOver(() => order2Alias)
				.Left.JoinAlias(() => order2Alias.OrderItems, () => orderItem2Alias)
				.WithSubquery.WhereProperty(() => order2Alias.Id).In(mainQuery)
				.Fetch(SelectMode.ChildFetch, () => order2Alias)
				.Fetch(SelectMode.ChildFetch, () => orderItem2Alias)
				.Fetch(SelectMode.Fetch, () => order2Alias.OrderItems)
				.Future();

			//Конец запроса

			var orderList = ordersQuery.ToList();
			var result = orderList
				.Where(x => x.Total19LBottlesToDeliver >= orderOnDayFilters.MinBottles19L
				&& x.Total19LBottlesToDeliver <= orderOnDayFilters.MaxBottles19L)
				.Distinct()
				.Select(o => new OrderOnDayNode
				{
					OrderId = o.Id,
					OrderStatus = o.OrderStatus,
					DeliveryPointLatitude = o.DeliveryPoint.Latitude,
					DeliveryPointLongitude = o.DeliveryPoint.Longitude,
					DeliveryPointShortAddress = o.DeliveryPoint.ShortAddress,
					DeliveryPointCompiledAddress = o.DeliveryPoint.CompiledAddress,
					DeliveryPointNetTopologyPoint = o.DeliveryPoint.NetTopologyPoint,
					DeliveryPointDistrictId = o.DeliveryPoint.District.Id,
					LogisticsRequirements = o.LogisticsRequirements,
					OrderAddressType = o.OrderAddressType,
					DeliverySchedule = o.DeliverySchedule,
					Total19LBottlesToDeliver = o.Total19LBottlesToDeliver,
					Total6LBottlesToDeliver = o.Total6LBottlesToDeliver,
					Total1500mlBottlesToDeliver = o.Total1500mlBottlesToDeliver,
					Total600mlBottlesToDeliver = o.Total600mlBottlesToDeliver,
					Total500mlBottlesToDeliver = o.Total500mlBottlesToDeliver,
					BottlesReturn = o.BottlesReturn,
					OrderComment = o.Comment,
					DeliveryPointComment = o.DeliveryPoint.Comment,
					CommentManager = o.CommentManager,
					ODZComment = o.ODZComment,
					OPComment = o.OPComment,
					DriverMobileAppComment = o.DriverMobileAppComment,
					IsCoolerAddedToOrder = o.IsCoolerAddedToOrder,
					IsSmallBottlesAddedToOrder = o.IsSmallBottlesAddedToOrder
				})
				.ToList();

			return result;
		}

		public IList<OrderWithAllocation> GetOrdersWithAllocationsOnDayByOrdersIds(IUnitOfWork uow, IEnumerable<int> orderIds)
		{
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty clientAlias = null;
			OrderWithAllocation resultAlias = null;

			var allocated = QueryOver.Of<PaymentItem>()
				.Where(pi => pi.Order.Id == orderAlias.Id && pi.PaymentItemStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum<PaymentItem>(pi => pi.Sum));

			var query = uow.Session.QueryOver(() => orderAlias)
				.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.JoinAlias(o => o.Client, () => clientAlias)
				.WhereRestrictionOn(o => o.Id).IsInG(orderIds)
				.SelectList(list => list
					.SelectGroup(o => o.Id).WithAlias(() => resultAlias.OrderId)
					.Select(o => o.DeliveryDate).WithAlias(() => resultAlias.OrderDeliveryDate)
					.Select(o => o.OrderStatus).WithAlias(() => resultAlias.OrderStatus)
					.Select(o => o.OrderPaymentStatus).WithAlias(() => resultAlias.OrderPaymentStatus)
					.Select(OrderProjections.GetOrderSumProjection()).WithAlias(() => resultAlias.OrderSum)
					.SelectSubQuery(allocated).WithAlias(() => resultAlias.OrderAllocation)
					.Select(() => clientAlias.FullName).WithAlias(() => resultAlias.OrderClientName)
					.Select(() => clientAlias.INN).WithAlias(() => resultAlias.OrderClientInn)
				)
				.TransformUsing(Transformers.AliasToBean<OrderWithAllocation>());

			return query.List<OrderWithAllocation>();
		}

		public IList<OrderWithAllocation> GetOrdersWithAllocationsOnDayByCounterparty(IUnitOfWork uow, int counterpartyId, IEnumerable<int> exceptOrderIds)
		{
			VodovozOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty clientAlias = null;
			OrderWithAllocation resultAlias = null;

			var allocated = QueryOver.Of<PaymentItem>()
				.Where(pi => pi.Order.Id == orderAlias.Id && pi.PaymentItemStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum<PaymentItem>(pi => pi.Sum));

			var query = uow.Session.QueryOver(() => orderAlias)
				.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.JoinAlias(o => o.Client, () => clientAlias)
				.WhereRestrictionOn(o => o.Id).Not.IsInG(exceptOrderIds)
				.AndRestrictionOn(o => o.OrderStatus).Not.IsIn(
					new[] { OrderStatus.NewOrder, OrderStatus.Canceled, OrderStatus.DeliveryCanceled, OrderStatus.NotDelivered })
				.And(o => o.Client.Id == counterpartyId)
				.And(o => o.PaymentType == PaymentType.Cashless)
				.SelectList(list => list
					.SelectGroup(o => o.Id).WithAlias(() => resultAlias.OrderId)
					.Select(o => o.DeliveryDate).WithAlias(() => resultAlias.OrderDeliveryDate)
					.Select(o => o.OrderStatus).WithAlias(() => resultAlias.OrderStatus)
					.Select(o => o.OrderPaymentStatus).WithAlias(() => resultAlias.OrderPaymentStatus)
					.Select(OrderProjections.GetOrderSumProjection()).WithAlias(() => resultAlias.OrderSum)
					.SelectSubQuery(allocated).WithAlias(() => resultAlias.OrderAllocation)
					.Select(() => true).WithAlias(() => resultAlias.IsMissingFromDocument)
					.Select(() => clientAlias.FullName).WithAlias(() => resultAlias.OrderClientName)
					.Select(() => clientAlias.INN).WithAlias(() => resultAlias.OrderClientInn)
				)
				.TransformUsing(Transformers.AliasToBean<OrderWithAllocation>());

			return query.List<OrderWithAllocation>();
		}

		public IList<OrderWithAllocation> GetAllocationsToOrdersWithAnotherClient(
			IUnitOfWork uow,
			int counterpartyId,
			string counterpartyInn,
			IEnumerable<int> exceptOrderIds)
		{
			var query =
				from payment in uow.Session.Query<Payment>()
				join paymentItem in uow.Session.Query<PaymentItem>()
					on payment.Id equals paymentItem.Payment.Id
				join order in uow.Session.Query<Order>()
					on paymentItem.Order.Id equals order.Id
				join orderClient in uow.Session.Query<Counterparty>()
					on order.Client.Id equals orderClient.Id

				where payment.CounterpartyInn == counterpartyInn
					&& paymentItem.PaymentItemStatus != AllocationStatus.Cancelled
					&& order.Client.Id != counterpartyId
					&& !exceptOrderIds.Contains(order.Id)
					&& order.PaymentType == PaymentType.Cashless
				group paymentItem by paymentItem.Order.Id
				into orderGroup

				let order = orderGroup.First().Order
				let orderSum = order.ActualGoodsTotalSum
				
				select new OrderWithAllocation
				{
					OrderStatus = order.OrderStatus,
					OrderClientInn = order.Client.INN,
					OrderClientName = order.Client.Name,
					IsMissingFromDocument = true,
					OrderSum = orderSum ?? 0,
					OrderAllocation = orderGroup.Sum(x => x.Sum),
					OrderId = order.Id,
					OrderDeliveryDate = order.DeliveryDate.Value,
					OrderPaymentStatus = order.OrderPaymentStatus,
				};

			return query.Distinct().ToList();
		}

		public int GetReferredCounterpartiesCountByReferPromotion(IUnitOfWork uow, int referrerId)
		{
			var referredCounterpartiesCount =
			(
				from counterparty in uow.Session.Query<Counterparty>()
				where counterparty.Referrer.Id == referrerId

				let finishedReferOrders = from orders in uow.Session.Query<Domain.Orders.Order>()
										  where orders.Client.Id == counterparty.Id
										  && GetOnClosingOrderStatuses().Contains(orders.OrderStatus)
										  select orders.Id

				where finishedReferOrders.Any()
				select counterparty.Id
			)
			.Count();

			return referredCounterpartiesCount;
		}

		public int GetAlreadyReceivedBottlesCountByReferPromotion(IUnitOfWork uow, Order order, int referFriendReasonId)
		{
			var alreadyReceivedBottlesByReferPromotion =
			(
				from orderItems in uow.Session.Query<OrderItem>()
				where orderItems.Order.Client.Id == order.Client.Id
				&& orderItems.Order.Id != order.Id
				&& orderItems.DiscountReason.Id == referFriendReasonId
				&& GetStatusesForCalculationAlreadyReceivedBottlesCountByReferPromotion().Contains(orderItems.Order.OrderStatus)
				select (orderItems.ActualCount ?? orderItems.Count)
			)
			.Sum(x => (int?)x);

			return alreadyReceivedBottlesByReferPromotion ?? 0;
		}

		private IEnumerable<VodovozOrder> GetOrdersForFirstUpdSending(
			IUnitOfWork uow, DateTime startDate, int organizationId, int closingDocumentDeliveryScheduleId)
		{
			Counterparty counterpartyAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			VodovozOrder orderAlias = null;
			EdoContainer edoContainerAlias = null;
			OrderEdoTrueMarkDocumentsActions orderEdoTrueMarkDocumentsActionsAlias = null;
			CounterpartyEdoAccount defaultEdoAccountAlias = null;

			var orderStatuses = new[] { OrderStatus.OnTheWay, OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };
			var orderStatusesForOrderDocumentCloser = new[] { OrderStatus.Closed };

			var query = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.JoinAlias(o => o.Contract, () => counterpartyContractAlias)
				.JoinEntityAlias(() => edoContainerAlias,
					() => orderAlias.Id == edoContainerAlias.Order.Id && edoContainerAlias.Type == DocumentContainerType.Upd, JoinType.LeftOuterJoin)
				.JoinAlias(() => counterpartyAlias.CounterpartyEdoAccounts,
					() => defaultEdoAccountAlias,
					JoinType.LeftOuterJoin,
					Restrictions.Where(() => defaultEdoAccountAlias.OrganizationId == counterpartyContractAlias.Organization.Id
						&& defaultEdoAccountAlias.IsDefault))
				.Where(() => orderAlias.DeliveryDate >= startDate)
				.And(() => !counterpartyAlias.IsNewEdoProcessing);

			var orderStatusRestriction = Restrictions.Or(
				Restrictions.And(
					Restrictions.Or(
						Restrictions.IsNull(Projections.Property(() => orderAlias.DeliverySchedule.Id)),
						Restrictions.NotEqProperty(
							Projections.Property(() => orderAlias.DeliverySchedule.Id),
							Projections.Constant(closingDocumentDeliveryScheduleId))),
					Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), orderStatuses)
					),
				Restrictions.And(
					Restrictions.EqProperty(
						Projections.Property(() => orderAlias.DeliverySchedule.Id),
						Projections.Constant(closingDocumentDeliveryScheduleId)),
					Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), orderStatusesForOrderDocumentCloser)
					)
				);

			var result = query.Where(() => counterpartyAlias.PersonType == PersonType.legal)
				.And(() => orderAlias.PaymentType == PaymentType.Cashless)
				.And(() => counterpartyContractAlias.Organization.Id == organizationId)
				.And(Restrictions.IsNull(Projections.Property(() => edoContainerAlias.Id)))
				.And(Restrictions.Disjunction()
					.Add(() => (
						counterpartyAlias.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess
							|| counterpartyAlias.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered)
							&& (defaultEdoAccountAlias.ConsentForEdoStatus == ConsentForEdoStatus.Agree))
					.Add(() => counterpartyAlias.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds
						&& defaultEdoAccountAlias.ConsentForEdoStatus == ConsentForEdoStatus.Agree))
				.And(orderStatusRestriction)
				.TransformUsing(Transformers.DistinctRootEntity)
				.SetTimeout(120)
				.List();

			return result;
		}
		
		private IEnumerable<VodovozOrder> GetOrdersForResendUpd(IUnitOfWork uow)
		{
			VodovozOrder orderAlias = null;
			OrderEdoTrueMarkDocumentsActions orderEdoTrueMarkDocumentsActionsAlias = null;

			var manualResendUpdStartDate = DateTime.Parse("2022-11-15");

			var result = uow.Session.QueryOver(() => orderAlias)
				.JoinEntityAlias(() => orderEdoTrueMarkDocumentsActionsAlias,
					() => orderAlias.Id == orderEdoTrueMarkDocumentsActionsAlias.Order.Id)
				.Where(() => orderEdoTrueMarkDocumentsActionsAlias.IsNeedToResendEdoUpd
					&& orderAlias.DeliveryDate >= manualResendUpdStartDate)
				.TransformUsing(Transformers.DistinctRootEntity)
				.SetTimeout(120)
				.List();

			return result;
		}
		
		public IEnumerable<VodovozOrder> GetOrdersForResendBills(IUnitOfWork uow, int? organizationId = null)
		{
			var result = from orders in uow.Session.Query<Order>()
				join actions in uow.Session.Query<OrderEdoTrueMarkDocumentsActions>()
					on orders.Id equals actions.Order.Id
				join counterpartyContract in uow.Session.Query<CounterpartyContract>()
					on orders.Contract.Id equals counterpartyContract.Id
				where actions.IsNeedToResendEdoBill
					&& (organizationId == null || counterpartyContract.Organization.Id == organizationId)
				select orders;			

			return result
				.Distinct()
				.ToList();
		}

		public IQueryable<OksDailyReportOrderDiscountDataNode> GetOrdersDiscountsDataForPeriod(IUnitOfWork uow, DateTime startDate, DateTime endDate)
		{
			var discounts =
				from orderItem in uow.Session.Query<OrderItem>()
				join nomenclature in uow.Session.Query<Nomenclature>() on orderItem.Nomenclature.Id equals nomenclature.Id
				join order in uow.Session.Query<Order>() on orderItem.Order.Id equals order.Id
				join counterparty in uow.Session.Query<Counterparty>() on order.Client.Id equals counterparty.Id
				join discountReason in uow.Session.Query<DiscountReason>() on orderItem.DiscountReason.Id equals discountReason.Id
				join un in uow.Session.Query<MeasurementUnits>() on nomenclature.Unit.Id equals un.Id into units
				from unit in units.DefaultIfEmpty()
				where 
				order.DeliveryDate >= startDate && order.DeliveryDate < endDate.LatestDayTime()
				select new OksDailyReportOrderDiscountDataNode
				{
					OrderId = order.Id,
					ClientName = counterparty.FullName,
					NomenclatureName = nomenclature.Name,
					OrderItemPrice = orderItem.Price,
					Amount = orderItem.ActualCount == null ? orderItem.Count : orderItem.ActualCount.Value,
					Discount = orderItem.Discount,
					DiscountMoney = orderItem.DiscountMoney,
					DiscountResonId = discountReason.Id,
					DiscountReasonName = discountReason.Name
				};

			return discounts;
		}

		public IList<RouteListItemTrueMarkProductCode> GetAddedRouteListItemTrueMarkProductCodesByOrderId(IUnitOfWork uow, int orderId)
		{
			var productCodes =
				from routeListItem in uow.Session.Query<RouteListItem>()
				join productCode in uow.Session.Query<RouteListItemTrueMarkProductCode>() on routeListItem.Id equals productCode.RouteListItem.Id
				where routeListItem.Order.Id == orderId
				select productCode;

			return productCodes.ToList();
		}

		public bool IsAllRouteListItemTrueMarkProductCodesAddedToOrder(IUnitOfWork uow, int orderId)
		{
			var accountableInTrueMarkGtinItemsCount = GetIsAccountableInTrueMarkOrderItems(uow, orderId)
				.GroupBy(x => x.Nomenclature.Gtins)
				.ToDictionary(x => x.Key, x => x.Sum(item => item.Count));

			var addedTrueMarkCodes = GetAddedRouteListItemTrueMarkProductCodesByOrderId(uow, orderId)
				.Where(x => x.SourceCodeStatus == SourceProductCodeStatus.Accepted)
				.Select(x => x.ResultCode)
				.GroupBy(x => x.Gtin)
				.ToDictionary(x => x.Key, x => x);

			foreach(var gtinsItemCount in accountableInTrueMarkGtinItemsCount)
			{
				var addedCodesCount = 0;

				foreach(var gtin in gtinsItemCount.Key)
				{
					addedCodesCount +=
						addedTrueMarkCodes.TryGetValue(gtin.GtinNumber, out var addedCodes)
						? addedCodes.Count()
						: 0;
				}

				if(addedCodesCount < gtinsItemCount.Value)
				{
					return false;
				}
			}

			return true;
		}

		public IList<TrueMarkProductCodeOrderItem> GetTrueMarkCodesAddedByDriverToOrderItemByOrderItemId(IUnitOfWork uow, int orderItemId)
		{
			var codesOrderItems = uow.Session.Query<TrueMarkProductCodeOrderItem>()
				.Where(x => x.OrderItemId == orderItemId)
				.ToList();

			return codesOrderItems;
		}

		public IList<TrueMarkProductCodeOrderItem> GetTrueMarkCodesAddedByDriverToOrderByOrderId(IUnitOfWork uow, int orderId)
		{
			var codesOrderItems = 
				from order in uow.Session.Query<Order>()
				join orderItem in uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
				join codeOrderItem in uow.Session.Query<TrueMarkProductCodeOrderItem>() on orderItem.Id equals codeOrderItem.OrderItemId
				where order.Id == orderId
				select codeOrderItem;

			return codesOrderItems.ToList();
		}

		public IList<TrueMarkWaterIdentificationCode> GetTrueMarkCodesAddedInWarehouseToOrderByOrderId(IUnitOfWork uow, int orderId)
		{
			var codes =
				(from carLoadDocumentItem in uow.Session.Query<CarLoadDocumentItem>()
				 join productCode in uow.Session.Query<CarLoadDocumentItemTrueMarkProductCode>() on carLoadDocumentItem.Id equals productCode.CarLoadDocumentItem.Id
				 where carLoadDocumentItem.OrderId == orderId
				 select productCode.ResultCode)
				.ToList();

			return codes;
		}

		public bool IsOrderCarLoadDocumentLoadOperationStateDone(IUnitOfWork uow, int orderId)
		{
			var carLoadDocumentLoadOperationState =
				(from carLoadDocument in uow.Session.Query<CarLoadDocument>()
				 join CarLoadDocumentItem in uow.Session.Query<CarLoadDocumentItem>() on carLoadDocument.Id equals CarLoadDocumentItem.Document.Id
				 where CarLoadDocumentItem.OrderId == orderId
				 select carLoadDocument.LoadOperationState)
				.FirstOrDefault();

			return carLoadDocumentLoadOperationState == CarLoadDocumentLoadOperationState.Done;
		}

		/// <inheritdoc/>
		public async Task<bool> IsAllDriversScannedCodesInOrderProcessed(IUnitOfWork uow, int orderId, CancellationToken cancellationToken = default)
		{
			var query =
				from order in uow.Session.Query<OrderEntity>()
				join orderItem in uow.Session.Query<OrderItemEntity>() on order.Id equals orderItem.Order.Id
				join driversScannedCode in uow.Session.Query<DriversScannedTrueMarkCode>() on orderItem.Id equals driversScannedCode.OrderItemId
				where
				order.Id == orderId
				&& driversScannedCode.DriversScannedTrueMarkCodeStatus == DriversScannedTrueMarkCodeStatus.None
				select driversScannedCode.Id;

			var driversScannedCodes = await query.ToListAsync(cancellationToken);

			return !driversScannedCodes.Any();
		}

		public IList<OrderItem> GetOrderItems(IUnitOfWork uow, int orderId)
		{
			OrderItem orderItemAlias = null;

			return uow.Session.QueryOver(() => orderItemAlias)
				.Where(() => orderItemAlias.Order.Id == orderId)
				.List();
		}
	}
}

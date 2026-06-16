using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.Domain.Sale;
using VodovozBusiness.EntityRepositories.Sale;
using VodovozBusiness.Nodes.SalesReport;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Sale
{
	public class SalesReportRepository : ISalesReportRepository
	{
		public async Task<IList<SalesReportDataNode>> GetSalesReportData(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			OrderDateFilterType orderDateType,
			SalesReportFilters filters,
			CancellationToken cancellationToken)
		{
			var inclusiveEndDate = endDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
			var orderQuery = await GetOrderQueryByDateTypeAsync(uow, startDate, inclusiveEndDate, orderDateType, filters, cancellationToken);

			return await GetSalesDataByOrderQueryAsync(uow, orderQuery, filters, cancellationToken);
		}

		private async Task<QueryOver<Order, Order>> GetOrderQueryByDateTypeAsync(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			OrderDateFilterType orderDateType,
			SalesReportFilters filters,
			CancellationToken cancellationToken)
		{
			switch(orderDateType)
			{
				case OrderDateFilterType.CreationDate:
					return GetOrderQueryByCreationDate(startDate, endDate, filters);
				case OrderDateFilterType.DeliveryDate:
					return GetOrderQueryByDeliveryDate(startDate, endDate, filters);
				case OrderDateFilterType.PaymentDate:
					return await GetOrderQueryByPaymentDateAsync(uow, startDate, endDate, filters, cancellationToken);
				default:
					return GetOrderQueryByDeliveryDate(startDate, endDate, filters);
			}
		}

		private QueryOver<Order, Order> GetOrderQueryByDeliveryDate(
			DateTime startDate,
			DateTime endDate,
			SalesReportFilters filters)
		{
			Order orderAlias = null;

			var query = QueryOver.Of(() => orderAlias)
				.Where(o => o.DeliveryDate >= startDate && o.DeliveryDate <= endDate)
				.Where(o => !o.IsContractCloser)
				.Select(Projections.Id());

			ApplyOrderFilters(query, filters);

			return query;
		}

		private QueryOver<Order, Order> GetOrderQueryByCreationDate(
			DateTime startDate,
			DateTime endDate,
			SalesReportFilters filters)
		{
			Order orderAlias = null;

			var query = QueryOver.Of(() => orderAlias)
				.Where(o => o.CreateDate >= startDate && o.CreateDate < endDate)
				.Where(o => !o.IsContractCloser)
				.Select(Projections.Id());

			ApplyOrderFilters(query, filters);

			return query;
		}

		private async Task<QueryOver<Order, Order>> GetOrderQueryByPaymentDateAsync(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			SalesReportFilters filters,
			CancellationToken cancellationToken)
		{
			var orderIds = new HashSet<int>();

			var cashlessQuery = GetCashlessPaymentOrderQuery(startDate, endDate);
			var cashlessIds = await cashlessQuery
				.GetExecutableQueryOver(uow.Session)
				.ListAsync<int>(cancellationToken);

			foreach(var id in cashlessIds)
			{
				orderIds.Add(id);
			}

			var sbpQuery = GetSbpPaymentOrderQuery(startDate, endDate);
			var sbpIds = await sbpQuery
				.GetExecutableQueryOver(uow.Session)
				.ListAsync<int>(cancellationToken);

			foreach(var id in sbpIds)
			{
				orderIds.Add(id);
			}

			var cashTerminalQuery = GetCashAndTerminalPaymentOrderQuery(startDate, endDate);
			var cashTerminalIds = await cashTerminalQuery
				.GetExecutableQueryOver(uow.Session)
				.ListAsync<int>(cancellationToken);

			foreach(var id in cashTerminalIds)
			{
				orderIds.Add(id);
			}

			var otherQuery = GetOtherPaymentOrderQuery(startDate, endDate);
			var otherIds = await otherQuery
				.GetExecutableQueryOver(uow.Session)
				.ListAsync<int>(cancellationToken);

			foreach(var id in otherIds)
			{
				orderIds.Add(id);
			}

			if(orderIds.Count == 0)
			{
				return QueryOver.Of<Order>()
					.Where(o => o.Id == -1)
					.Select(Projections.Id());
			}

			var query = QueryOver.Of<Order>()
				.WhereRestrictionOn(o => o.Id).IsIn(orderIds.ToArray())
				.Select(Projections.Id());

			ApplyOrderFilters(query, filters);

			return query;
		}

		private QueryOver<Order, Order> GetCashlessPaymentOrderQuery(DateTime startDate, DateTime endDate)
		{
			Order orderAlias = null;
			PaymentItem paymentItemAlias = null;
			Payment paymentFromBankAlias = null;

			var query = QueryOver.Of(() => orderAlias)
				.JoinEntityAlias(() => paymentItemAlias,
					() => paymentItemAlias.Order.Id == orderAlias.Id
						&& paymentItemAlias.PaymentItemStatus == AllocationStatus.Accepted,
					JoinType.InnerJoin)
				.JoinAlias(() => paymentItemAlias.Payment, () => paymentFromBankAlias)
				.Where(() => !orderAlias.IsContractCloser)
				.Where(() => orderAlias.PaymentType == PaymentType.Cashless)
				.Where(() => orderAlias.OrderPaymentStatus == OrderPaymentStatus.Paid)
				.Where(() => paymentFromBankAlias.Date >= startDate)
				.Where(() => paymentFromBankAlias.Date <= endDate)
				.Select(Projections.Distinct(Projections.Id()));

			return query;
		}

		private QueryOver<Order, Order> GetSbpPaymentOrderQuery(DateTime startDate, DateTime endDate)
		{
			Order orderAlias = null;
			FastPayment fastPaymentAlias = null;

			var sbpPaymentTypes = new[] { PaymentType.DriverApplicationQR, PaymentType.SmsQR };
			var onlinePaymentFromIds = new[] { 11, 12, 13 };

			var query = QueryOver.Of(() => orderAlias)
				.JoinEntityAlias(() => fastPaymentAlias,
					() => fastPaymentAlias.Order.Id == orderAlias.Id
						&& fastPaymentAlias.FastPaymentStatus == FastPaymentStatus.Performed
						&& fastPaymentAlias.PaidDate >= startDate 
						&& fastPaymentAlias.PaidDate <= endDate,
					JoinType.LeftOuterJoin)
				.Where(() => fastPaymentAlias.PaidDate >= startDate)
				.Where(() => fastPaymentAlias.PaidDate <= endDate)
				.Where(
					Restrictions.Or(
						Restrictions.In(Projections.Property(() => orderAlias.PaymentType), sbpPaymentTypes),
						Restrictions.And(
							Restrictions.Where(() => orderAlias.PaymentType == PaymentType.PaidOnline),
							Restrictions.In(Projections.Property(() => orderAlias.PaymentByCardFrom.Id), onlinePaymentFromIds)
						)
					)
				)
				.Select(Projections.Id());

			return query;
		}

		private QueryOver<Order, Order> GetCashAndTerminalPaymentOrderQuery(DateTime startDate, DateTime endDate)
		{
			Order orderAlias = null;
			var cashPaymentTypes = new[] { PaymentType.Cash, PaymentType.Terminal };

			var query = QueryOver.Of(() => orderAlias)
				.Where(o => o.DeliveryDate >= startDate && o.DeliveryDate <= endDate)
				.Where(o => !o.IsContractCloser)
				.Where(Restrictions.In(Projections.Property(() => orderAlias.PaymentType), cashPaymentTypes))
				.Select(Projections.Id());

			return query;
		}

		private QueryOver<Order, Order> GetOtherPaymentOrderQuery(DateTime startDate, DateTime endDate)
		{
			Order orderAlias = null;

			var otherPaymentTypes = new[] { PaymentType.Barter, PaymentType.ContractDocumentation };
			var onlinePaymentFromIds = new[] { 1, 2, 3, 9, 14, 15, 16 };

			var query = QueryOver.Of(() => orderAlias)
				.Where(o => o.CreateDate >= startDate && o.CreateDate <= endDate)
				.Where(o => !o.IsContractCloser)
				.Where(
					Restrictions.Disjunction()
						.Add(Restrictions.In(Projections.Property(() => orderAlias.PaymentType), otherPaymentTypes))
						.Add(
							Restrictions.And(
								Restrictions.Where(() => orderAlias.PaymentType == PaymentType.PaidOnline),
								Restrictions.In(Projections.Property(() => orderAlias.PaymentByCardFrom.Id), onlinePaymentFromIds)
							)
						)
				)
				.Select(Projections.Id());

			return query;
		}

		/// <summary>
		/// Применить фильтры, которые относятся к самому заказу
		/// </summary>
		/// <param name="query"></param>
		/// <param name="filters"></param>
		private void ApplyOrderFilters(
			IQueryOver<Order, Order> query,
			SalesReportFilters filters)
		{
			// фильтр по контрагентам
			if(filters.CounterpartyInclude != null && filters.CounterpartyInclude.Any())
			{
				query.WhereRestrictionOn(o => o.Client.Id).IsIn(filters.CounterpartyInclude);
			}
			if(filters.CounterpartyExclude != null && filters.CounterpartyExclude.Any())
			{
				query.WhereRestrictionOn(o => o.Client.Id).Not.IsIn(filters.CounterpartyExclude);
			}

			// Фильтр по автору заказа
			if(filters.OrderAuthorInclude != null && filters.OrderAuthorInclude.Any())
			{
				query.WhereRestrictionOn(o => o.Author.Id).IsIn(filters.OrderAuthorInclude);
			}
			if(filters.OrderAuthorExclude != null && filters.OrderAuthorExclude.Any())
			{
				query.WhereRestrictionOn(o => o.Author.Id).Not.IsIn(filters.OrderAuthorExclude);
			}

			// Фильтр по типу оплаты
			if(filters.PaymentTypeInclude != null && filters.PaymentTypeInclude.Any())
			{
				query.WhereRestrictionOn(o => o.PaymentType).IsIn(filters.PaymentTypeInclude);
			}
			if(filters.PaymentTypeExclude != null && filters.PaymentTypeExclude.Any())
			{
				query.WhereRestrictionOn(o => o.PaymentType).Not.IsIn(filters.PaymentTypeExclude);
			}
		}

		private async Task<IList<SalesReportDataNode>> GetSalesDataByOrderQueryAsync(
			IUnitOfWork uow,
			QueryOver<Order, Order> orderQuery,
			SalesReportFilters filters,
			CancellationToken cancellationToken)
		{
			SalesReportDataNode resultAlias = null;
			DiscountReason discountReasonAlias = null;
			Order orderAlias = null;
			RouteListItem routeListItemAlias = null;
			RouteList routeListAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Phone phoneClientAlias = null;
			Phone phoneDeliveryPointAlias = null;
			Employee authorAlias = null;
			Employee managerAlias = null;
			Subdivision subdivisionAlias = null;
			CounterpartyContract contractAlias = null;
			Organization organizationAlias = null;
			PromotionalSet promotionalSetAlias = null;
			CounterpartyClassification counterpartyClassificationAlias = null;
			CounterpartySubtype counterpartySubtypeAlias = null;
			District districtAlias = null;
			GeoGroup geoGroupAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias)
				.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.JoinAlias(o => o.Author, () => authorAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(o => o.Contract, () => contractAlias)
				.Left.JoinAlias(() => contractAlias.Organization, () => organizationAlias)
				.Left.JoinAlias(() => counterpartyAlias.SalesManager, () => managerAlias)
				.Left.JoinAlias(() => authorAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => orderItemAlias.PromoSet, () => promotionalSetAlias)
				.Left.JoinAlias(() => counterpartyAlias.CounterpartySubtype, () => counterpartySubtypeAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geoGroupAlias)
				.Left.JoinAlias(() => counterpartyAlias.Phones, () => phoneClientAlias, () => !phoneClientAlias.IsArchive)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => phoneDeliveryPointAlias, () => !phoneDeliveryPointAlias.IsArchive)
				.JoinEntityAlias(() => routeListItemAlias,
					() => routeListItemAlias.Order.Id == orderAlias.Id
						&& routeListItemAlias.Status != RouteListItemStatus.Transfered
						&& routeListItemAlias.Status != RouteListItemStatus.Canceled
						&& routeListItemAlias.Status != RouteListItemStatus.Overdue,
					JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => counterpartyClassificationAlias,
					() => counterpartyClassificationAlias.CounterpartyId == counterpartyAlias.Id,
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.WithSubquery.WhereProperty(o => o.Id).In(orderQuery)
				.Where(o => !o.IsContractCloser);

			// Фильтр по номенклатуре
			if(filters.NomenclatureInclude != null && filters.NomenclatureInclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.Id).IsIn(filters.NomenclatureInclude);
			}
			if(filters.NomenclatureExclude != null && filters.NomenclatureExclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.Id).Not.IsIn(filters.NomenclatureExclude);
			}

			// Фильтр по категории номенклатуры
			if(filters.NomenclatureCategoryInclude != null && filters.NomenclatureCategoryInclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.Category).IsIn(filters.NomenclatureCategoryInclude);
			}
			if(filters.NomenclatureCategoryExclude != null && filters.NomenclatureCategoryExclude.Any())
			{
				query.Where(
					Restrictions.Or(
						Restrictions.Not(Restrictions.On(() => nomenclatureAlias.Category).IsIn(filters.NomenclatureCategoryExclude)),
						Restrictions.IsNull(Projections.Property(() => nomenclatureAlias.Category))
					)
				);
			}

			// Фильтр по контрагентам
			if(filters.CounterpartyInclude != null && filters.CounterpartyInclude.Any())
			{
				query.WhereRestrictionOn(() => counterpartyAlias.Id).IsIn(filters.CounterpartyInclude);
			}
			if(filters.CounterpartyExclude != null && filters.CounterpartyExclude.Any())
			{
				query.WhereRestrictionOn(() => counterpartyAlias.Id).Not.IsIn(filters.CounterpartyExclude);
			}

			// Фильтр по типу контрагента
			if(filters.CounterpartyTypeInclude != null && filters.CounterpartyTypeInclude.Any())
			{
				query.WhereRestrictionOn(() => counterpartyAlias.CounterpartyType).IsIn(filters.CounterpartyTypeInclude);
			}
			if(filters.CounterpartyTypeExclude != null && filters.CounterpartyTypeExclude.Any())
			{
				query.WhereRestrictionOn(() => counterpartyAlias.CounterpartyType).Not.IsIn(filters.CounterpartyTypeExclude);
			}

			// Фильтр по подтипу контрагента
			if(filters.CounterpartySubtypeInclude != null && filters.CounterpartySubtypeInclude.Any())
			{
				query.Where(
					Restrictions.Or(
						Restrictions.Eq(Projections.Property(() => counterpartyAlias.CounterpartyType), CounterpartyType.AdvertisingDepartmentClient),
						Restrictions.InG(Projections.Property(() => counterpartySubtypeAlias.Id), filters.CounterpartySubtypeInclude)
					)
				);
			}
			if(filters.CounterpartySubtypeExclude != null && filters.CounterpartySubtypeExclude.Any())
			{
				query.WhereRestrictionOn(() => counterpartySubtypeAlias.Id).Not.IsIn(filters.CounterpartySubtypeExclude);
			}

			// Фильтр по организациям
			if(filters.OrganizationInclude != null && filters.OrganizationInclude.Any())
			{
				query.WhereRestrictionOn(() => organizationAlias.Id).IsIn(filters.OrganizationInclude);
			}
			if(filters.OrganizationExclude != null && filters.OrganizationExclude.Any())
			{
				query.Where(
					Restrictions.Or(
						Restrictions.Not(Restrictions.On(() => organizationAlias.Id).IsIn(filters.OrganizationExclude)),
						Restrictions.IsNull(Projections.Property(() => organizationAlias.Id))
					)
				);
			}

			// Фильтр по подразделениям автора
			if(filters.SubdivisionInclude != null && filters.SubdivisionInclude.Any())
			{
				query.WhereRestrictionOn(() => subdivisionAlias.Id).IsIn(filters.SubdivisionInclude);
			}
			if(filters.SubdivisionExclude != null && filters.SubdivisionExclude.Any())
			{
				query.WhereRestrictionOn(() => subdivisionAlias.Id).Not.IsIn(filters.SubdivisionExclude);
			}

			// Фильтр по автору заказа
			if(filters.OrderAuthorInclude != null && filters.OrderAuthorInclude.Any())
			{
				query.WhereRestrictionOn(() => authorAlias.Id).IsIn(filters.OrderAuthorInclude);
			}
			if(filters.OrderAuthorExclude != null && filters.OrderAuthorExclude.Any())
			{
				query.WhereRestrictionOn(() => authorAlias.Id).Not.IsIn(filters.OrderAuthorExclude);
			}

			// Фильтр по менеджеру контрагента
			if(filters.SalesManagerInclude != null && filters.SalesManagerInclude.Any())
			{
				query.WhereRestrictionOn(() => managerAlias.Id).IsIn(filters.SalesManagerInclude);
			}
			if(filters.SalesManagerExclude != null && filters.SalesManagerExclude.Any())
			{
				query.Where(
					Restrictions.Or(
						Restrictions.Not(Restrictions.On(() => managerAlias.Id).IsIn(filters.SalesManagerExclude)),
						Restrictions.IsNull(Projections.Property(() => managerAlias.Id))
					)
				);
			}

			// Фильтр по гео-группам
			if(filters.GeoGroupInclude != null && filters.GeoGroupInclude.Any())
			{
				query.WhereRestrictionOn(() => geoGroupAlias.Id).IsIn(filters.GeoGroupInclude);
			}
			if(filters.GeoGroupExclude != null && filters.GeoGroupExclude.Any())
			{
				query.Where(
				   Restrictions.Or(
					   Restrictions.Not(Restrictions.On(() => geoGroupAlias.Id).IsIn(filters.GeoGroupExclude)),
					   Restrictions.Or(
						   Restrictions.IsNull(Projections.Property(() => deliveryPointAlias.Id)),
						   Restrictions.IsNull(Projections.Property(() => geoGroupAlias.Id)))
				   )
			   );
			}

			ApplyPaymentTypeFilters(query, filters, orderAlias);

			ApplyCounterpartyClassificationFilters(query, filters, counterpartyClassificationAlias);

			// Фильтр по промонаборам
			if(filters.PromotionalSetInclude != null && filters.PromotionalSetInclude.Any())
			{
				query.WhereRestrictionOn(() => promotionalSetAlias.Id).IsIn(filters.PromotionalSetInclude);
			}
			if(filters.PromotionalSetExclude != null && filters.PromotionalSetExclude.Any())
			{
				query.Where(
				   Restrictions.Or(
					   Restrictions.Not(Restrictions.On(() => promotionalSetAlias.Id).IsIn(filters.PromotionalSetExclude)),
					   Restrictions.IsNull(Projections.Property(() => promotionalSetAlias.Id))
				   )
			   );
			}

			// Фильтр по группам товаров
			if(filters.ProductGroupInclude != null && filters.ProductGroupInclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.ProductGroup.Id).IsIn(filters.ProductGroupInclude);
			}
			if(filters.ProductGroupExclude != null && filters.ProductGroupExclude.Any())
			{
				query.Where(
					Restrictions.Or(
						Restrictions.Not(Restrictions.On(() => nomenclatureAlias.ProductGroup.Id).IsIn(filters.ProductGroupExclude)),
						Restrictions.IsNull(Projections.Property(() => nomenclatureAlias.ProductGroup.Id))
					)
				);
			}

			// Фильтр по статусам заказа
			if(filters.OrderStatusInclude != null && filters.OrderStatusInclude.Any())
			{
				query.WhereRestrictionOn(x => x.OrderStatus).IsIn(filters.OrderStatusInclude);
			}
			if(filters.OrderStatusExclude != null && filters.OrderStatusExclude.Any())
			{
				query.WhereRestrictionOn(x => x.OrderStatus).Not.IsIn(filters.OrderStatusExclude);
			}

			// Фильтр по основаниям скидок
			if(filters.DiscountReasonInclude != null && filters.DiscountReasonInclude.Any())
			{
				var includeSubquery = QueryOver.Of<OrderItem>()
					.Where(oi => oi.Id == orderItemAlias.Id)
					.JoinQueryOver(oi => oi.DiscountReasons, () => discountReasonAlias)
					.Where(Restrictions.In(Projections.Property(() => discountReasonAlias.Id), filters.DiscountReasonInclude))
					.Select(Projections.Constant(1));

				query.WithSubquery.WhereExists(includeSubquery);
			}
			if(filters.DiscountReasonExclude != null && filters.DiscountReasonExclude.Any())
			{
				var excludeSubquery = QueryOver.Of<OrderItem>()
					.Where(oi => oi.Id == orderItemAlias.Id)
					.JoinQueryOver(oi => oi.DiscountReasons, () => discountReasonAlias)
					.Where(Restrictions.In(Projections.Property(() => discountReasonAlias.Id), filters.DiscountReasonExclude))
					.Select(Projections.Constant(1));

				query.WithSubquery.WhereNotExists(excludeSubquery);
			}

			// Фильтр по самовывозу
			if(filters.IsSelfDelivery.HasValue)
			{
				query.Where(o => o.SelfDelivery == filters.IsSelfDelivery.Value);
			}

			// Фильтр по чекам
			if(filters.OnlyWithCashReceipts.HasValue)
			{
				ApplyCashReceiptsFilter(query, filters.OnlyWithCashReceipts.Value, orderAlias);
			}

			// Фильтр "только заказы из МЛ"
			if(filters.OnlyOrdersFromRouteLists.HasValue)
			{
				if(filters.OnlyOrdersFromRouteLists.Value)
				{
					query.Where(() => routeListItemAlias.Id != null && carModelAlias.CarTypeOfUse != CarTypeOfUse.Truck);
				}
				else
				{
					query.Where(() => routeListItemAlias.Id == null);
				}
			}

			var phoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String,
					"CONCAT('+7', IF(?1 IS NULL, ?2, ?3))"),
				NHibernateUtil.String,
				Projections.Property(() => deliveryPointAlias.Id),
				Projections.Property(() => phoneClientAlias.DigitsNumber),
				Projections.Property(() => phoneDeliveryPointAlias.DigitsNumber));

			var totalCountProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, 
					"COALESCE(?1, ?2)"),
				NHibernateUtil.Decimal,
				Projections.Property(() => orderItemAlias.ActualCount),
				Projections.Property(() => orderItemAlias.Count));

			var totalSumProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal,
					"TRUNCATE(COALESCE(?1, ?2) * ?3 - COALESCE(?4, 0), 2)"),
				NHibernateUtil.Decimal,
				Projections.Property(() => orderItemAlias.ActualCount),
				Projections.Property(() => orderItemAlias.Count),
				Projections.Property(() => orderItemAlias.Price),
				Projections.Property(() => orderItemAlias.DiscountMoney));

			query.SelectList(list => list
				.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
				.SelectGroup(() => counterpartyAlias.Id).WithAlias(() => resultAlias.CounterpartyId)
				.SelectGroup(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
				.SelectGroup(() => counterpartyAlias.CounterpartyType).WithAlias(() => resultAlias.CounterpartyType)
				.SelectGroup(() => organizationAlias.Name).WithAlias(() => resultAlias.Organization)
				.SelectGroup(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.DeliveryPoint)
				.SelectGroup(() => orderAlias.PaymentType).WithAlias(() => resultAlias.PaymentType)
				.SelectGroup(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
				.SelectGroup(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteList)
				.SelectGroup(() => orderItemAlias.Id).WithAlias(() => resultAlias.OrderItemId)
				.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				.SelectGroup(() => nomenclatureAlias.OfficialName).WithAlias(() => resultAlias.NomenclatureName)
				.SelectGroup(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
				.SelectGroup(() => subdivisionAlias.Name).WithAlias(() => resultAlias.AuthorSubdivision)
				.SelectGroup(() => subdivisionAlias.Id).WithAlias(() => resultAlias.AuthorSubdivisionId)
				.SelectGroup(() => managerAlias.Name).WithAlias(() => resultAlias.SalesManagerName)
				.SelectGroup(() => authorAlias.Name).WithAlias(() => resultAlias.OrderAuthorName)
				.SelectGroup(() => promotionalSetAlias.Name).WithAlias(() => resultAlias.PromotionalSet)
				.SelectGroup(() => authorAlias.LastName).WithAlias(() => resultAlias.OrderAuthor)
				.SelectGroup(() => nomenclatureAlias.IsDisposableTare).WithAlias(() => resultAlias.IsDisposableTare)
				.Select(phoneProjection).WithAlias(() => resultAlias.Phones)
				.Select(totalCountProjection).WithAlias(() => resultAlias.TotalCount)
				.Select(totalSumProjection).WithAlias(() => resultAlias.TotalSum)
			)
			.TransformUsing(Transformers.AliasToBean<SalesReportDataNode>());

			var allResults = await query.ListAsync<SalesReportDataNode>(cancellationToken);

			return allResults;
		}

		/// <summary>
		/// Применить фильтр по типу оплаты
		/// </summary>
		/// <param name="query">Запрос для фильтрации</param>
		/// <param name="filters">Фильтры для применения</param>
		/// <param name="orderAlias">Алиас заказа</param>
		private void ApplyPaymentTypeFilters(
			IQueryOver<Order, Order> query,
			SalesReportFilters filters,
			Order orderAlias)
		{
			if(filters.PaymentTypeInclude is null || !filters.PaymentTypeInclude.Any())
			{
				return;
			}

			var disjunction = Restrictions.Disjunction();

			var mainConjunction = Restrictions.Conjunction();
			mainConjunction.Add(Restrictions.In(Projections.Property(() => orderAlias.PaymentType), filters.PaymentTypeInclude));

			if(filters.PaymentTypeExclude != null && filters.PaymentTypeExclude.Any())
			{
				mainConjunction.Add(Restrictions.Not(Restrictions.In(Projections.Property(() => orderAlias.PaymentType), filters.PaymentTypeExclude)));
			}
			disjunction.Add(mainConjunction);

			if(filters.PaymentTypeInclude.Contains(PaymentType.Terminal))
			{
				var terminalConjunction = Restrictions.Conjunction();
				terminalConjunction.Add(Restrictions.Where(() => orderAlias.PaymentType == PaymentType.Terminal));
				if(filters.PaymentByTerminalSourceInclude != null && filters.PaymentByTerminalSourceInclude.Any())
				{
					terminalConjunction.Add(Restrictions.In(
						Projections.Property(() => orderAlias.PaymentByTerminalSource), 
						filters.PaymentByTerminalSourceInclude));
				}
				if(filters.PaymentByTerminalSourceExclude != null && filters.PaymentByTerminalSourceExclude.Any())
				{
					terminalConjunction.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => orderAlias.PaymentByTerminalSource),
						filters.PaymentByTerminalSourceExclude.Cast<object>().ToArray())));
				}

				disjunction.Add(terminalConjunction);
			}

			if(filters.PaymentTypeInclude.Contains(PaymentType.PaidOnline))
			{
				var onlineConjunction = Restrictions.Conjunction();
				onlineConjunction.Add(Restrictions.Where(() => orderAlias.PaymentType == PaymentType.PaidOnline));
				if(filters.PaymentFromInclude != null && filters.PaymentFromInclude.Any())
				{
					onlineConjunction.Add(Restrictions.In(
						Projections.Property(() => orderAlias.PaymentByCardFrom.Id),
						filters.PaymentFromInclude.Cast<object>().ToArray()));
				}

				if(filters.PaymentFromExclude != null && filters.PaymentFromExclude.Any())
				{
					onlineConjunction.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => orderAlias.PaymentByCardFrom.Id),
						filters.PaymentFromExclude.Cast<object>().ToArray())));
				}

				disjunction.Add(onlineConjunction);
			}

			query.Where(disjunction);
		}

		/// <summary>
		/// Применить фильтр по классификациям контрагентов
		/// </summary>
		/// <param name="query">Запрос для фильтрации</param>
		/// <param name="filters">Фильтры для применения</param>
		/// <param name="counterpartyClassificationAlias">Алиас классификации контрагента</param>
		private void ApplyCounterpartyClassificationFilters(
			IQueryOver<Order, Order> query,
			SalesReportFilters filters,
			CounterpartyClassification counterpartyClassificationAlias)
		{
			if(filters.CounterpartyCompositeClassificationInclude == null || !filters.CounterpartyCompositeClassificationInclude.Any())
			{
				return;
			}

			var includeDisjunction = Restrictions.Disjunction();

			foreach(var classification in filters.CounterpartyCompositeClassificationInclude)
			{
				if(classification == CounterpartyCompositeClassification.New)
				{
					includeDisjunction.Add(
						Restrictions.Or(
							Restrictions.IsNull(Projections.Property(() => counterpartyClassificationAlias.ClassificationByBottlesCount)),
							Restrictions.IsNull(Projections.Property(() => counterpartyClassificationAlias.ClassificationByOrdersCount))
						)
					);
					continue;
				}

				var bottlesCount = CounterpartyClassification.ConvertToClassificationByBottlesCount(classification);
				var ordersCount = CounterpartyClassification.ConvertToClassificationByOrdersCount(classification);

				if(bottlesCount.HasValue && ordersCount.HasValue)
				{
					var conjunction = Restrictions.Conjunction();
					conjunction.Add(Restrictions.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == bottlesCount.Value));
					conjunction.Add(Restrictions.Where(() => counterpartyClassificationAlias.ClassificationByOrdersCount == ordersCount.Value));
					includeDisjunction.Add(conjunction);
				}
			}

			query.Where(includeDisjunction);

			if(filters.CounterpartyCompositeClassificationExclude != null && filters.CounterpartyCompositeClassificationExclude.Any())
			{
				var excludeDisjunction = Restrictions.Disjunction();

				foreach(var classification in filters.CounterpartyCompositeClassificationExclude)
				{
					if(classification == CounterpartyCompositeClassification.New)
					{
						excludeDisjunction.Add(
							Restrictions.And(
								Restrictions.IsNotNull(Projections.Property(() => counterpartyClassificationAlias.ClassificationByBottlesCount)),
								Restrictions.IsNotNull(Projections.Property(() => counterpartyClassificationAlias.ClassificationByOrdersCount))
							)
						);
						continue;
					}

					var bottlesCount = CounterpartyClassification.ConvertToClassificationByBottlesCount(classification);
					var ordersCount = CounterpartyClassification.ConvertToClassificationByOrdersCount(classification);

					if(bottlesCount.HasValue && ordersCount.HasValue)
					{
						var conjunction = Restrictions.Conjunction();
						conjunction.Add(Restrictions.Where(() => counterpartyClassificationAlias.ClassificationByBottlesCount == bottlesCount.Value));
						conjunction.Add(Restrictions.Where(() => counterpartyClassificationAlias.ClassificationByOrdersCount == ordersCount.Value));
						excludeDisjunction.Add(conjunction);
					}
				}

				query.Where(Restrictions.Not(excludeDisjunction));
			}
		}

		/// <summary>
		/// Применить фильтр по наличию кассовых чеков
		/// </summary>
		/// <param name="query">Запрос для фильтрации</param>
		/// <param name="onlyWithCashReceipts">Флаг, указывающий, нужно ли фильтровать только по наличию кассовых чеков</param>
		/// <param name="orderAlias">Алиас заказа</param>
		private void ApplyCashReceiptsFilter(
			IQueryOver<Order, Order> query,
			bool onlyWithCashReceipts,
			Order orderAlias)
		{
			var allowdStatuses = new[]
			{
				FiscalDocumentStatus.WaitForCallback,
				FiscalDocumentStatus.Printed,
				FiscalDocumentStatus.Completed
			};

			EdoFiscalDocument fiscalDocumentAlias = null;
			EdoTask edoTaskAlias = null;
			FormalEdoRequest formalRequestAlias = null;

			var subquery = QueryOver.Of(() => fiscalDocumentAlias)
				.JoinAlias(() => fiscalDocumentAlias.ReceiptEdoTask, () => edoTaskAlias)
				.JoinEntityAlias(() => formalRequestAlias, () => formalRequestAlias.Task.Id == edoTaskAlias.Id)
				.Where(() => formalRequestAlias.Order.Id == orderAlias.Id)
				.WhereRestrictionOn(() => fiscalDocumentAlias.Status).IsIn(allowdStatuses)
				.Select(Projections.Constant(1));

			if(onlyWithCashReceipts)
			{
				query.WithSubquery.WhereExists(subquery);
			}
			else
			{
				query.WithSubquery.WhereNotExists(subquery);
			}
		}

		public async Task<BottlesDataNode> GetBottlesData(
			IUnitOfWork uow,
			IEnumerable<int> orderIds,
			int defaultBottleNomenclatureId,
			CancellationToken cancellationToken)
		{
			Order orderAlias = null;
			RouteListItem routeListItemAlias = null;
			SelfDeliveryDocument selfDeliveryDocAlias = null;
			SelfDeliveryDocumentItem selfDeliveryItemAlias = null;
			BottlesDataNode resultAlias = null;

			var orderIdsArray = orderIds.ToArray();

			var routeListFactSubquery = QueryOver.Of(() => routeListItemAlias)
				.Where(() => routeListItemAlias.Order.Id == orderAlias.Id)
				.Where(() => routeListItemAlias.TransferedTo == null)
				.Select(Projections.Sum(() => routeListItemAlias.BottlesReturned));

			var selfDeliveryFactSubquery = QueryOver.Of(() => selfDeliveryDocAlias)
				.JoinAlias(() => selfDeliveryDocAlias.Items, () => selfDeliveryItemAlias)
				.Where(() => selfDeliveryDocAlias.Order.Id == orderAlias.Id)
				.Where(() => selfDeliveryItemAlias.Nomenclature.Id == defaultBottleNomenclatureId)
				.Select(Projections.Sum(() => selfDeliveryItemAlias.Amount));

			var query = uow.Session.QueryOver(() => orderAlias)
				.WhereRestrictionOn(o => o.Id).IsIn(orderIdsArray)
				.SelectList(list => list
					.SelectSum(() => orderAlias.BottlesReturn).WithAlias(() => resultAlias.Plan)
					.Select(
						Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.Int32, "ROUND(SUM(IFNULL(?1, IFNULL(?2, 0))), 0)"),
							NHibernateUtil.Int32,
							Projections.SubQuery(routeListFactSubquery),
							Projections.SubQuery(selfDeliveryFactSubquery)))
						.WithAlias(() => resultAlias.Fact)
				)
				.TransformUsing(Transformers.AliasToBean<BottlesDataNode>());

			var result = await query.SingleOrDefaultAsync<BottlesDataNode>(cancellationToken);

			return result ?? new BottlesDataNode { Plan = 0, Fact = 0 };
		}
	}
}

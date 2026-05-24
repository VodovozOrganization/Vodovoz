using Core.Infrastructure;
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
			string orderDateType,
			SalesReportFilters filters,
			CancellationToken cancellationToken)
		{
			var orderIds = GetOrderIdsByDateType(uow, startDate, endDate, orderDateType, filters);

			if(!orderIds.Any())
			{
				return new List<SalesReportDataNode>();
			}

			return await GetSalesDataByOrderIdsAsync(uow, orderIds, filters, cancellationToken);
		}


		private IEnumerable<int> GetOrderIdsByDateType(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			string orderDateType,
			SalesReportFilters filters)
		{
			switch(orderDateType)
			{
				case "DeliveryDate":
					return GetOrderIdsByDeliveryDate(uow, startDate, endDate, filters);
				case "CreationDate":
					return GetOrderIdsByCreationDate(uow, startDate, endDate, filters);
				case "PaymentDate":
					return GetOrderIdsByPaymentDate(uow, startDate, endDate, filters);
				default:
					return GetOrderIdsByDeliveryDate(uow, startDate, endDate, filters);
			}
		}

		private IEnumerable<int> GetOrderIdsByDeliveryDate(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			SalesReportFilters filters)
		{
			Order orderAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias)
				.Where(o => o.DeliveryDate >= startDate && o.DeliveryDate <= endDate)
				.Where(o => !o.IsContractCloser)
				.Select(Projections.Id());

			ApplyOrderFilters(query, filters);

			return query.List<int>();
		}

		private IEnumerable<int> GetOrderIdsByCreationDate(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			SalesReportFilters filters)
		{
			Order orderAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias)
				.Where(o => o.CreateDate >= startDate && o.CreateDate <= endDate)
				.Where(o => !o.IsContractCloser)
				.Select(Projections.Id());

			ApplyOrderFilters(query, filters);

			return query.List<int>();
		}

		private IEnumerable<int> GetOrderIdsByPaymentDate(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			SalesReportFilters filters)
		{
			var orderIds = new HashSet<int>();

			var cashlessIds = GetCashlessPaymentOrderIds(uow, startDate, endDate, filters);
			foreach(var id in cashlessIds)
			{
				orderIds.Add(id);
			}

			var sbpIds = GetSbpPaymentOrderIds(uow, startDate, endDate, filters);
			foreach(var id in sbpIds)
			{
				orderIds.Add(id);
			}

			var cashTerminalIds = GetCashAndTerminalPaymentOrderIds(uow, startDate, endDate, filters);
			foreach(var id in cashTerminalIds)
			{
				orderIds.Add(id);
			}

			var otherIds = GetOtherPaymentOrderIds(uow, startDate, endDate, filters);
			foreach(int id in otherIds)
			{
				orderIds.Add(id);
			}

			return orderIds;
		}

		private IEnumerable<int> GetCashlessPaymentOrderIds(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			SalesReportFilters filters)
		{
			Order orderAlias = null;
			PaymentItem paymentItemAlias = null;
			Payment paymentFromBankAlias = null;

			var subquery = QueryOver.Of<PaymentItem>(() => paymentItemAlias)
				.JoinAlias(() => paymentItemAlias.Payment, () => paymentFromBankAlias)
				.Where(() => paymentItemAlias.Order.Id == orderAlias.Id)
				.Where(() => paymentItemAlias.PaymentItemStatus == AllocationStatus.Accepted)
				.Where(() => paymentFromBankAlias.Date >= startDate)
				.Where(() => paymentFromBankAlias.Date <= endDate)
				.Select(Projections.Constant(1));

			var query = uow.Session.QueryOver(() => orderAlias)
				.Where(o => o.PaymentType == PaymentType.Cashless)
				.Where(o => o.OrderPaymentStatus == OrderPaymentStatus.Paid)
				.Where(o => !o.IsContractCloser)
				.WithSubquery.WhereExists(subquery)
				.Select(Projections.Id());

			ApplyOrderFilters(query, filters);

			return query.List<int>();
		}

		private IEnumerable<int> GetSbpPaymentOrderIds(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			SalesReportFilters filters)
		{
			Order orderAlias = null;
			FastPayment fastPaymentAlias = null;

			var sbpPaymentTypes = new[] { PaymentType.DriverApplicationQR, PaymentType.SmsQR };
			var onlinePaymentFromIds = new[] { 11, 12, 13 };

			var query = uow.Session.QueryOver(() => orderAlias)
				.JoinEntityAlias(() => fastPaymentAlias,
					() => fastPaymentAlias.Order.Id == orderAlias.Id,
					JoinType.LeftOuterJoin)
				.Where(o => !o.IsContractCloser)
				.Where(
					Restrictions.Disjunction()
						.Add(Restrictions.In(Projections.Property(() => orderAlias.PaymentType), sbpPaymentTypes))
						.Add(
							Restrictions.And(
								Restrictions.Where(() => orderAlias.PaymentType == PaymentType.PaidOnline),
								Restrictions.In(Projections.Property(() => orderAlias.PaymentByCardFrom.Id), onlinePaymentFromIds)
							)
						)
				)
				.Where(() => fastPaymentAlias.FastPaymentStatus == FastPaymentStatus.Performed)
				.Where(() => fastPaymentAlias.PaidDate >= startDate)
				.Where(() => fastPaymentAlias.PaidDate <= endDate)
				.Select(Projections.Id());

			ApplyOrderFilters(query, filters);

			return query.List<int>();
		}

		private IEnumerable<int> GetCashAndTerminalPaymentOrderIds(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			SalesReportFilters filters)
		{
			Order orderAlias = null;
			var cashPaymentTypes = new[] { PaymentType.Cash, PaymentType.Terminal };

			var query = uow.Session.QueryOver(() => orderAlias)
				.Where(o => o.DeliveryDate >= startDate && o.DeliveryDate <= endDate)
				.Where(o => !o.IsContractCloser)
				.Where(Restrictions.In(Projections.Property(() => orderAlias.PaymentType), cashPaymentTypes))
				.Select(Projections.Id());

			ApplyOrderFilters(query, filters);

			return query.List<int>();
		}

		private IEnumerable<int> GetOtherPaymentOrderIds(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			SalesReportFilters filters)
		{
			Order orderAlias = null;

			var otherPaymentTypes = new[] { PaymentType.Barter, PaymentType.ContractDocumentation };
			var onlinePaymentFromIds = new[] { 1, 2, 3, 9, 14, 15, 16 };

			var query = uow.Session.QueryOver(() => orderAlias)
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

			ApplyOrderFilters(query, filters);

			return query.List<int>();
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

		private async Task<IList<SalesReportDataNode>> GetSalesDataByOrderIdsAsync(
			IUnitOfWork uow,
			IEnumerable<int> orderIds,
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
			Employee authorAlias = null;
			Employee managerAlias = null;
			Subdivision subdivisionAlias = null;
			CounterpartyContract contractAlias = null;
			Organization organizationAlias = null;
			PromotionalSet promotionalSetAlias = null;
			CounterpartyClassification counterpartyClassificationAlias = null;
			CounterpartySubtype counterpartySubtypeAlias = null;
			GeoGroup geoGroupAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;

			var orderIdsArray = orderIds.ToArray();

			var query = uow.Session.QueryOver(() => orderAlias)
				.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(o => o.Author, () => authorAlias)
				.Left.JoinAlias(o => o.Contract, () => contractAlias)
				.Left.JoinAlias(() => contractAlias.Organization, () => organizationAlias)
				.Left.JoinAlias(() => counterpartyAlias.SalesManager, () => managerAlias)
				.Left.JoinAlias(() => authorAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => orderItemAlias.PromoSet, () => promotionalSetAlias)
				.Left.JoinAlias(() => counterpartyAlias.CounterpartySubtype, () => counterpartySubtypeAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District.GeographicGroup, () => geoGroupAlias)
				.JoinEntityAlias(() => routeListItemAlias,
					() => routeListItemAlias.Order.Id == orderAlias.Id,
					JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => counterpartyClassificationAlias,
					() => counterpartyClassificationAlias.CounterpartyId == counterpartyAlias.Id,
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.WhereRestrictionOn(o => o.Id).IsIn(orderIdsArray)
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
				query.WhereRestrictionOn(() => nomenclatureAlias.Category).Not.IsIn(filters.NomenclatureCategoryExclude);
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
				query.WhereRestrictionOn(() => organizationAlias.Id).Not.IsIn(filters.OrganizationExclude);
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
				query.WhereRestrictionOn(() => managerAlias.Id).Not.IsIn(filters.SalesManagerExclude);
			}

			// Фильтр по гео-группам
			if(filters.GeoGroupInclude != null && filters.GeoGroupInclude.Any())
			{
				query.WhereRestrictionOn(() => geoGroupAlias.Id).IsIn(filters.GeoGroupInclude);
			}
			if(filters.GeoGroupExclude != null && filters.GeoGroupExclude.Any())
			{
				query.WhereRestrictionOn(() => geoGroupAlias.Id).Not.IsIn(filters.GeoGroupExclude);
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
				query.WhereRestrictionOn(() => promotionalSetAlias.Id).Not.IsIn(filters.PromotionalSetExclude);
			}

			// Фильтр по группам товаров
			if(filters.ProductGroupInclude != null && filters.ProductGroupInclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.ProductGroup.Id).IsIn(filters.ProductGroupInclude);
			}
			if(filters.ProductGroupExclude != null && filters.ProductGroupExclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.ProductGroup.Id).Not.IsIn(filters.ProductGroupExclude);
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
					.Where(() => discountReasonAlias.Id.IsIn(filters.DiscountReasonInclude))
					.Select(Projections.Constant(1));

				query.WithSubquery.WhereExists(includeSubquery);
			}
			if(filters.DiscountReasonExclude != null && filters.DiscountReasonExclude.Any())
			{
				var excludeSubquery = QueryOver.Of<OrderItem>()
					.Where(oi => oi.Id == orderItemAlias.Id)
					.JoinQueryOver(oi => oi.DiscountReasons, () => discountReasonAlias)
					.Where(() => discountReasonAlias.Id.IsIn(filters.DiscountReasonExclude))
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

			query.SelectList(list => list
				.Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.CounterpartyId)
				.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
				.Select(() => counterpartyAlias.CounterpartyType).WithAlias(() => resultAlias.CounterpartyType)
				.Select(() => organizationAlias.Name).WithAlias(() => resultAlias.Organization)
				.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.DeliveryPoint)
				.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
				.Select(() => orderAlias.PaymentType).WithAlias(() => resultAlias.PaymentType)
				.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
				.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteList)
				.Select(() => orderItemAlias.Id).WithAlias(() => resultAlias.OrderItemId)
				.Select(() => nomenclatureAlias.OfficialName).WithAlias(() => resultAlias.NomenclatureName)
				.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
				.Select(() => nomenclatureAlias.IsDisposableTare).WithAlias(() => resultAlias.IsDisposableTare)
				.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.AuthorSubdivision)
				.Select(() => subdivisionAlias.Id).WithAlias(() => resultAlias.AuthorSubdivisionId)
				.Select(() => managerAlias.Name).WithAlias(() => resultAlias.SalesManagerName)
				.Select(() => authorAlias.Name).WithAlias(() => resultAlias.OrderAuthorName)
				.Select(() => promotionalSetAlias.Name).WithAlias(() => resultAlias.PromotionalSet)
				.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.OrderAuthor)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT('Заказ №', ?1)"),
					NHibernateUtil.String,
					Projections.Property(() => orderAlias.Id))).WithAlias(() => resultAlias.OrdDetails)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.Decimal, "COALESCE(?1, ?2)"),
					NHibernateUtil.Decimal,
					Projections.Property(() => orderItemAlias.ActualCount),
					Projections.Property(() => orderItemAlias.Count))).WithAlias(() => resultAlias.TotalCount)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.Decimal,
						"TRUNCATE(ROUND(COALESCE(?1, ?2) * ?3 - COALESCE(?4, 0), 2), 2)"),
					NHibernateUtil.Decimal,
					Projections.Property(() => orderItemAlias.ActualCount),
					Projections.Property(() => orderItemAlias.Count),
					Projections.Property(() => orderItemAlias.Price),
					Projections.Property(() => orderItemAlias.DiscountMoney))).WithAlias(() => resultAlias.TotalSum)
			)
			.TransformUsing(Transformers.AliasToBean<SalesReportDataNode>());

			const int pageSize = 10_000;
			var allResults = new List<SalesReportDataNode>();
			int offset = 0;

			while(true)
			{
				var page = await query.Skip(offset).Take(pageSize).ListAsync<SalesReportDataNode>(cancellationToken);
				if(page.Count == 0) break;
				allResults.AddRange(page);
				offset += pageSize;
			}

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

			var subquery = QueryOver.Of<Order>()
				.Where(o => o.Id == orderAlias.Id)
				.And(Subqueries.WhereExists(
					QueryOver.Of<EdoFiscalDocument>()
						.JoinAlias(efd => efd.ReceiptEdoTask, () => null)
						.Where(efd => efd.Status.IsIn(allowdStatuses))
						.Select(Projections.Constant(1))
				));

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
			BottlesDataNode resultAlias = null;
			Order orderAlias = null;
			RouteListItem routeListItemAlias = null;
			SelfDeliveryDocument selfDeliveryDocAlias = null;
			SelfDeliveryDocumentItem selfDeliveryItemAlias = null;

			var selfDeliveryFactSubquery = QueryOver.Of(() => selfDeliveryDocAlias)
				.JoinAlias(() => selfDeliveryDocAlias.Items, () => selfDeliveryItemAlias)
				.Where(() => selfDeliveryDocAlias.Order.Id == orderAlias.Id)
				.Where(() => selfDeliveryItemAlias.Nomenclature.Id == defaultBottleNomenclatureId)
				.Select(Projections.Sum(() => selfDeliveryItemAlias.Amount));

			var routeListFactSubquery = QueryOver.Of(() => routeListItemAlias)
				.Where(() => routeListItemAlias.Order.Id == orderAlias.Id)
				.Where(() => routeListItemAlias.Status == RouteListItemStatus.Completed)
				.Where(() => routeListItemAlias.TransferedTo == null)
				.Select(Projections.Sum(() => routeListItemAlias.BottlesReturned));

			var query = uow.Session.QueryOver(() => orderAlias)
				.WhereRestrictionOn(o => o.Id).IsIn(orderIds.ToArray())
				.SelectList(list => list
					.SelectSum(() => orderAlias.BottlesReturn).WithAlias(() => resultAlias.Plan)
					.SelectSubQuery(routeListFactSubquery).WithAlias(() => resultAlias.FactFromRouteList)
					.SelectSubQuery(selfDeliveryFactSubquery).WithAlias(() => resultAlias.FactFromSelfDelivery)
				)
				.TransformUsing(Transformers.AliasToBean<BottlesDataNode>())
				.Take(1);

			var result = await query.SingleOrDefaultAsync<BottlesDataNode>(cancellationToken);

			if(result is null)
			{
				return new BottlesDataNode
				{
					Plan = 0,
					FactFromRouteList = 0,
					FactFromSelfDelivery = 0
				};
			}

			return new BottlesDataNode
			{
				Plan = result.Plan,
				FactFromRouteList = result.FactFromRouteList,
				FactFromSelfDelivery = result.FactFromSelfDelivery
			};
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Nodes;
using Vodovoz.Services;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Goods
{
	public class NomenclatureRepository : INomenclatureRepository
	{
		private readonly INomenclatureParametersProvider nomenclatureParametersProvider;

		public NomenclatureRepository(INomenclatureParametersProvider nomenclatureParametersProvider) {
			this.nomenclatureParametersProvider = nomenclatureParametersProvider ?? 
				throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
		}
		
		public QueryOver<Nomenclature> NomenclatureForProductMaterialsQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForProductMaterial()))
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureEquipmentsQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.equipment)
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureForSaleQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForSale()))
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureByCategory(NomenclatureCategory category)
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == category)
							.Where(n => !n.IsArchive);
		}

		/// <summary>
		/// Запрос номенклатур которые можно использовать на складе
		/// </summary>
		public QueryOver<Nomenclature> NomenclatureOfGoodsOnlyQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForGoods()))
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureOfGoodsWithoutEmptyBottlesQuery()
		{
			return QueryOver.Of<Nomenclature>()
							.Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForGoodsWithoutEmptyBottles()))
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureWaterOnlyQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.water)
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureEquipOnlyQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.equipment)
							.Where(n => !n.IsArchive);
		}

		public Nomenclature GetDefaultBottleNomenclature(IUnitOfWork uow) =>
			nomenclatureParametersProvider.GetDefaultBottleNomenclature(uow);

		/// <summary>
		/// Возвращает список номенклатур, которые зависят от передаваемой номенклатуры.
		/// </summary>
		/// <returns>Список зависимых номенклатур.</returns>
		/// <param name="uow">uow - Unit of work</param>
		/// <param name="influentialNomenclature">influentialNomenclature - вляющая номенклатура</param>
		public IList<Nomenclature> GetDependedNomenclatures(IUnitOfWork uow, Nomenclature influentialNomenclature)
		{
			return uow.Session.QueryOver<Nomenclature>()
					  .Where(n => n.DependsOnNomenclature.Id == influentialNomenclature.Id)
					  .List();
		}

		public QueryOver<Nomenclature> NomenclatureOfItemsForService()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.equipment)
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureOfPartsForService()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.spare_parts)
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureOfServices()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.service)
							.Where(n => !n.IsArchive);
		}

		public IList<Nomenclature> GetNomenclatureOfDefectiveGoods(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Nomenclature>()
				.Where(n => n.IsDefectiveBottle).List();
		}

		public string GetNextCode1c(IUnitOfWork uow)
		{
			var lastCode1c = uow.Query<Nomenclature>()
								.Where(n => n.Code1c.IsLike(Nomenclature.PrefixOfCode1c, MatchMode.Start))
								.OrderBy(n => n.Code1c).Desc
								.Select(n => n.Code1c)
								.Take(1)
								.SingleOrDefault<string>();
			int id = 0;
			if(!String.IsNullOrEmpty(lastCode1c)) {
				id = int.Parse(lastCode1c.Replace(Nomenclature.PrefixOfCode1c, ""));//Тут специально падаем в эксепшен если не смогли распарсить, подума 5 раз, пережде чем заменить на TryParse
			}
			id++;
			string format = new String('0', Nomenclature.LengthOfCode1c - Nomenclature.PrefixOfCode1c.Length);
			return Nomenclature.PrefixOfCode1c + id.ToString(format);
		}

		public QueryOver<Nomenclature> NomenclatureInGroupsQuery(int[] groupsIds)
		{
			return QueryOver.Of<Nomenclature>()
							.Where(n => n.ProductGroup.Id.IsIn(groupsIds));
		}

		public Nomenclature GetNomenclatureToAddWithMaster(IUnitOfWork uow) =>
			nomenclatureParametersProvider.GetNomenclatureToAddWithMaster(uow);
		
		public Nomenclature GetForfeitNomenclature(IUnitOfWork uow) => nomenclatureParametersProvider.GetForfeitNomenclature(uow);
		
		public int[] GetSanitisationNomenclature(IUnitOfWork uow) => nomenclatureParametersProvider.GetSanitisationNomenclature(uow);
		
		public IList<Nomenclature> GetNomenclatureWithPriceForMobileApp(IUnitOfWork uow, params MobileCatalog[] catalogs)
		{
			return uow.Session.QueryOver<Nomenclature>()
							  .Where(n => !n.IsArchive)
							  .Where(n => n.MobileCatalog.IsIn(catalogs))
							  .List();
		}

		#region Rent

		/// <summary>
		/// Возвращает доступное оборудование указанного типа для аренды
		/// </summary>
		public Nomenclature GetAvailableNonSerialEquipmentForRent(IUnitOfWork uow, EquipmentKind kind, IEnumerable<int> excludeNomenclatures)
		{
			Nomenclature nomenclatureAlias = null;

			var nomenclatureList = GetAllNonSerialEquipmentForRent(uow, kind);
			
			//Выбираются только доступные на складе и еще не выбранные в диалоге
			var availableNomenclature = nomenclatureList.Where(x => x.Available > 0)
				.Where(x => !excludeNomenclatures.Contains(x.Id))
				.ToList();

			//Если есть дежурное оборудование выбираем сначала его
			var duty = uow.Session.QueryOver(() => nomenclatureAlias)
				.Where(() => nomenclatureAlias.IsDuty)
				.WhereRestrictionOn(() => nomenclatureAlias.Id)
				.IsIn(availableNomenclature.Select(x => x.Id).ToArray()).List();
			
			if(duty.Any()) 
			{
				return duty.First();
			}

			//Иначе если есть приоритетное оборудование выбираем его
			var priority = uow.Session.QueryOver(() => nomenclatureAlias)
				.Where(() => nomenclatureAlias.RentPriority)
				.WhereRestrictionOn(() => nomenclatureAlias.Id)
				.IsIn(availableNomenclature.Select(x => x.Id).ToArray()).List();
			
			if(priority.Any()) 
			{
				return priority.First();
			}

			//Выбираем любое доступное оборудование
			var any = uow.Session.QueryOver(() => nomenclatureAlias)
				.WhereRestrictionOn(() => nomenclatureAlias.Id)
				.IsIn(availableNomenclature.Select(x => x.Id).ToArray()).List();
			
			return any.FirstOrDefault();
		}
		
		/// <summary>
		/// Возвращает список всего оборудования определенного типа для аренды
		/// </summary>
		public IList<NomenclatureForRentNode> GetAllNonSerialEquipmentForRent(IUnitOfWork uow, EquipmentKind kind)
		{
			return QueryAvailableNonSerialEquipmentForRent(kind)
				.GetExecutableQueryOver(uow.Session)
				.List<NomenclatureForRentNode>();
		}
		
		/// <summary>
		/// Запрос выбирающий количество добавленное на склад, отгруженное со склада 
		/// и зарезервированное в заказах каждой номенклатуры по выбранному типу оборудования
		/// </summary>
		public QueryOver<Nomenclature, Nomenclature> QueryAvailableNonSerialEquipmentForRent(EquipmentKind kind)
		{
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation operationAddAlias = null;

			//Подзапрос выбирающий по номенклатуре количество добавленное на склад
			var subqueryAdded = QueryOver.Of(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.IncomingWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));
			
			//Подзапрос выбирающий по номенклатуре количество отгруженное со склада
			var subqueryRemoved = QueryOver.Of(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.WriteoffWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			//Подзапрос выбирающий по номенклатуре количество зарезервированное в заказах до отгрузки со склада
			Vodovoz.Domain.Orders.Order localOrderAlias = null;
			OrderEquipment localOrderEquipmentAlias = null;
			Equipment localEquipmentAlias = null;
			
			var subqueryReserved = QueryOver.Of(() => localOrderAlias)
				.JoinAlias(() => localOrderAlias.OrderEquipments, () => localOrderEquipmentAlias)
				.JoinAlias(() => localOrderEquipmentAlias.Equipment, () => localEquipmentAlias)
				.Where(() => localEquipmentAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(() => localOrderEquipmentAlias.Direction == Direction.Deliver)
				.Where(() => localOrderAlias.OrderStatus == OrderStatus.Accepted
					   || localOrderAlias.OrderStatus == OrderStatus.InTravelList
					   || localOrderAlias.OrderStatus == OrderStatus.OnLoading)
				.Select(Projections.Sum(() => localOrderEquipmentAlias.Count));

			NomenclatureForRentNode resultAlias = null;
			MeasurementUnits unitAlias = null;
			EquipmentKind equipmentKindAlias = null;

			//Запрос выбирающий количество добавленное на склад, отгруженное со склада 
			//и зарезервированное в заказах каждой номенклатуры по выбранному типу оборудования
			var query = QueryOver.Of(() => nomenclatureAlias)
							 .JoinAlias(() => nomenclatureAlias.Unit, () => unitAlias)
							 .JoinAlias(() => nomenclatureAlias.Kind, () => equipmentKindAlias);
			
			if(kind != null)
			{
				query = query.Where(() => equipmentKindAlias.Id == kind.Id);
			}

			query = query.SelectList(
				list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
		            .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
		            .Select(() => equipmentKindAlias.Id).WithAlias(() => resultAlias.TypeId)
		            .Select(() => equipmentKindAlias.Name).WithAlias(() => resultAlias.EquipmentKindName)
					.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
					.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
					.SelectSubQuery(subqueryAdded).WithAlias(() => resultAlias.Added)
					.SelectSubQuery(subqueryRemoved).WithAlias(() => resultAlias.Removed)
					.SelectSubQuery(subqueryReserved).WithAlias(() => resultAlias.Reserved))
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NomenclatureForRentNode>());
			return query;
		}

		#endregion

		/// <summary>
		/// Возврат словаря сертификатов для передаваемых номенклатур
		/// </summary>
		/// <returns>Словарь сертификатов</returns>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="nomenclatures">Список номенклатур</param>
		public Dictionary<Nomenclature, IList<Certificate>> GetDictionaryWithCertificatesForNomenclatures(IUnitOfWork uow, Nomenclature[] nomenclatures)
		{
			Dictionary<Nomenclature, IList<Certificate>> dict = new Dictionary<Nomenclature, IList<Certificate>>();
			foreach(var n in nomenclatures) {
				Nomenclature nomenclatureAlias = null;
				var certificates = uow.Session.QueryOver<Certificate>()
									   .Left.JoinAlias(c => c.Nomenclatures, () => nomenclatureAlias)
									   .Where(() => nomenclatureAlias.Id == n.Id)
									   .List()
									   ;
				if(certificates.Any()) {
					if(!dict.ContainsKey(n))
						dict.Add(n, certificates);
					else {
						foreach(Certificate certificate in certificates)
							dict[n].Add(certificate);
					}
				}
			}

			return dict;
		}

		/// <summary>
		/// Возвращает Dictionary где: 
		/// key - id номенклатуры
		/// value - массив id картинок
		/// </summary>
		/// <returns>The nomenclature images identifiers.</returns>
		public Dictionary<int, int[]> GetNomenclatureImagesIds(IUnitOfWork uow, params int[] nomenclatureIds)
		{
			return uow.Session.QueryOver<NomenclatureImage>()
						 .Where(n => n.Nomenclature.Id.IsIn(nomenclatureIds))
						 .SelectList(list => list
						 	.Select(i => i.Id)
							.Select(i => i.Nomenclature.Id)
						 )
						 .List<object[]>()
						 .GroupBy(x => (int)x[1])
						 .ToDictionary(g => g.Key, g => g.Select(x => (int)x[0]).ToArray());
		}

		#region Получение номенклатур воды

		public Nomenclature GetWaterSemiozerie(IUnitOfWork uow) => nomenclatureParametersProvider.GetWaterSemiozerie(uow);

		public Nomenclature GetWaterKislorodnaya(IUnitOfWork uow) => nomenclatureParametersProvider.GetWaterKislorodnaya(uow);

		public Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow) => nomenclatureParametersProvider.GetWaterSnyatogorskaya(uow);

		public Nomenclature GetWaterKislorodnayaDeluxe(IUnitOfWork uow) => nomenclatureParametersProvider.GetWaterKislorodnayaDeluxe(uow);

		public Nomenclature GetWaterStroika(IUnitOfWork uow) => nomenclatureParametersProvider.GetWaterStroika(uow);

		public Nomenclature GetWaterRuchki(IUnitOfWork uow) => nomenclatureParametersProvider.GetWaterRuchki(uow);

		#endregion

		public decimal GetWaterPriceIncrement => nomenclatureParametersProvider.GetWaterPriceIncrement;

		public int GetIdentifierOfOnlineShopGroup() => nomenclatureParametersProvider.GetIdentifierOfOnlineShopGroup();

		public decimal GetPurchasePrice(IUnitOfWork uow, int routeListId, DateTime date)
		{
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			NomenclaturePurchasePrice nomenclaturePurchasePriceAlias = null;
			NomenclatureCostPrice nomenclatureCostPriceAlias = null;

			var purchasePrice = QueryOver.Of(() => nomenclaturePurchasePriceAlias)
				.Where(() => nomenclaturePurchasePriceAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And(() => nomenclaturePurchasePriceAlias.StartDate <= date)
				.And(() => nomenclaturePurchasePriceAlias.EndDate == null || nomenclaturePurchasePriceAlias.EndDate > date)
				.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(
							NHibernateUtil.Decimal, "ROUND(?1 * IFNULL(?2, ?3), 2)"),
						NHibernateUtil.Decimal,
						Projections.Property(() => nomenclaturePurchasePriceAlias.PurchasePrice),
						Projections.Property(() => orderItemAlias.ActualCount),
						Projections.Property(() => orderItemAlias.Count)));
			
			var costPrice = QueryOver.Of(() => nomenclatureCostPriceAlias)
				.Where(() => nomenclatureCostPriceAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And(() => nomenclatureCostPriceAlias.StartDate <= date)
				.And(() => nomenclatureCostPriceAlias.EndDate == null || nomenclatureCostPriceAlias.EndDate > date)
				.Select(
					Projections.SqlFunction(
						new SQLFunctionTemplate(
							NHibernateUtil.Decimal, "ROUND(?1 * IFNULL(?2, ?3), 2)"),
						NHibernateUtil.Decimal,
						Projections.Property(() => nomenclatureCostPriceAlias.CostPrice),
						Projections.Property(() => orderItemAlias.ActualCount),
						Projections.Property(() => orderItemAlias.Count)));
			
			return uow.Session.QueryOver<RouteListItem>()
				.JoinAlias(rla => rla.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(rla => rla.RouteList.Id == routeListId)
				.AndRestrictionOn(rla => rla.Status).Not.IsInG(RouteListItem.GetNotDeliveredStatuses())
				.Select(
					Projections.Sum(
						Projections.Conditional(
							Restrictions.Where(() => nomenclatureAlias.UsingInGroupPriceSet),
								Projections.SubQuery(costPrice),
								Projections.SubQuery(purchasePrice)))
				)
				.SingleOrDefault<decimal>();
		}
		
		public decimal GetInnerDeliveryPrice(IUnitOfWork uow, int routeListId, DateTime date)
		{
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			NomenclatureInnerDeliveryPrice innerDeliveryPriceAlias = null;
			CarLoadDocument carLoadDocumentAlias = null;
			Warehouse warehouseAlias = null;

			var innerDeliveryPriceProjection =
				Projections.SqlFunction(
					new SQLFunctionTemplate(
						NHibernateUtil.Decimal, "ROUND(?1 * IFNULL(?2, ?3), 2)"),
					NHibernateUtil.Decimal,
					Projections.Property(() => innerDeliveryPriceAlias.Price),
					Projections.Property(() => orderItemAlias.ActualCount),
					Projections.Property(() => orderItemAlias.Count));
			
			var query = uow.Session.QueryOver<RouteListItem>()
				.JoinAlias(rla => rla.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.InnerDeliveryPrices, () => innerDeliveryPriceAlias)
				.JoinEntityAlias(() => carLoadDocumentAlias,
					() => carLoadDocumentAlias.RouteList.Id == routeListId, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => warehouseAlias,
					() => carLoadDocumentAlias.Warehouse.Id == warehouseAlias.Id, JoinType.LeftOuterJoin)
				.Where(rla => rla.RouteList.Id == routeListId)
				.AndRestrictionOn(rla => rla.Status).Not.IsInG(RouteListItem.GetNotDeliveredStatuses())
				.And(() => innerDeliveryPriceAlias.StartDate <= date)
				.And(() => innerDeliveryPriceAlias.EndDate == null || innerDeliveryPriceAlias.EndDate > date)
				.SelectList(list => list
					.SelectGroup(() => orderItemAlias.Id)
					.Select(Projections.Sum(
						Projections.Conditional(
							Restrictions.Where(() => warehouseAlias.TypeOfUse == WarehouseUsing.Production),
							Projections.Constant(0m),
							innerDeliveryPriceProjection
						))))
				.TransformUsing(Transformers.AliasToBean<(int Id, decimal InnerDeliveryPriceSum)>())
				.List<(int OrderItemId, decimal InnerDeliveryPriceSum)>();

			return query.Sum(x => x.InnerDeliveryPriceSum);
		}

		public RouteExpensesNode GetOtherRouteExpenses(
			IUnitOfWork uow, int routeListId, decimal administrativeExpenses, decimal routeExpenses)
		{
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			RouteExpensesNode resultAlias = null;

			return uow.Session.QueryOver<RouteListItem>()
				.JoinAlias(rla => rla.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(rla => rla.RouteList.Id == routeListId)
				.AndRestrictionOn(rla => rla.Status).Not.IsInG(RouteListItem.GetNotDeliveredStatuses())
				.SelectList(list => list
					.Select(Projections.Sum(
						Projections.SqlFunction(
							new SQLFunctionTemplate(
								NHibernateUtil.Decimal, "ROUND(?1 * ?2 * IFNULL(?3, ?4), 2)"),
							NHibernateUtil.Decimal,
							Projections.Constant(administrativeExpenses),
							Projections.Property(() => nomenclatureAlias.Weight),
							Projections.Property(() => orderItemAlias.ActualCount),
							Projections.Property(() => orderItemAlias.Count)))
					.WithAlias(() => resultAlias.AdministrativeExpenses))
					.Select(Projections.Sum(
						Projections.SqlFunction(
							new SQLFunctionTemplate(
								NHibernateUtil.Decimal, "ROUND(?1 * ?2 * IFNULL(?3, ?4), 2)"),
							NHibernateUtil.Decimal,
							Projections.Constant(routeExpenses),
							Projections.Property(() => nomenclatureAlias.Weight),
							Projections.Property(() => orderItemAlias.ActualCount),
							Projections.Property(() => orderItemAlias.Count)))
					.WithAlias(() => resultAlias.RouteListExpenses)
					))
				.TransformUsing(Transformers.AliasToBean<RouteExpensesNode>())
				.SingleOrDefault<RouteExpensesNode>();
		}
		
		public decimal GetWarehouseExpensesForRoute(IUnitOfWork uow, int routeListId, decimal warehouseExpenses)
		{
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			RouteListItem routeListItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			CarLoadDocument carLoadDocumentAlias = null;
			Warehouse warehouseAlias = null;

			var warehouseExpensesProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(
					NHibernateUtil.Decimal, "ROUND(?1 * ?2 * IFNULL(?3, ?4), 2)"),
				NHibernateUtil.Decimal,
				Projections.Constant(warehouseExpenses),
				Projections.Property(() => nomenclatureAlias.Weight),
				Projections.Property(() => orderItemAlias.ActualCount),
				Projections.Property(() => orderItemAlias.Count));

			var query = uow.Session.QueryOver(() => routeListItemAlias)
				.JoinAlias(rla => rla.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.JoinEntityAlias(() => carLoadDocumentAlias,
					() => carLoadDocumentAlias.RouteList.Id == routeListId, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => warehouseAlias,
					() => carLoadDocumentAlias.Warehouse.Id == warehouseAlias.Id, JoinType.LeftOuterJoin)
				.Where(rla => rla.RouteList.Id == routeListId)
				.AndRestrictionOn(rla => rla.Status).Not.IsInG(RouteListItem.GetNotDeliveredStatuses())
				.SelectList(list => list
					.SelectGroup(() => orderItemAlias.Id))
					.Select(Projections.Sum(
						Projections.Conditional(
							Restrictions.Where(() => warehouseAlias.TypeOfUse == WarehouseUsing.Production),
							Projections.Constant(0m),
							warehouseExpensesProjection)))
				.TransformUsing(Transformers.AliasToBean<(int Id, decimal warehouseExpensesSum)>())
				.List<(int OrderItemId, decimal WarehouseExpensesSum)>();

			return query.Sum(x => x.WarehouseExpensesSum);
		}

		public bool Has19LWater(IUnitOfWork uow, int[] siteNomenclaturesIds)
		{
			return uow.Session.QueryOver<Nomenclature>()
				.WhereRestrictionOn(n => n.Id).IsIn(siteNomenclaturesIds)
				.And(n => n.Category == NomenclatureCategory.water)
				.And(n => n.TareVolume == TareVolume.Vol19L)
				.List()
				.Any();
		}
	}

	public class NomenclatureAmountNode
	{
		public int NomenclatureId { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public decimal Amount { get; set; }
	}

	public class RouteExpensesNode
	{
		public decimal AdministrativeExpenses { get; set; }
		public decimal RouteListExpenses { get; set; }
	}
}

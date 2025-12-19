using NHibernate;
using NHibernate.Criterion;
using NHibernate.Criterion.Lambda;
using NHibernate.Linq;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Nodes;
using Vodovoz.Nodes;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Infrastructure.Persistance.Goods
{
	internal sealed class NomenclatureRepository : INomenclatureRepository
	{
		private readonly INomenclatureSettings _nomenclatureSettings;

		public NomenclatureRepository(INomenclatureSettings nomenclatureSettings)
		{
			_nomenclatureSettings = nomenclatureSettings ??
				throw new ArgumentNullException(nameof(nomenclatureSettings));
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
			if(!string.IsNullOrEmpty(lastCode1c))
			{
				id = int.Parse(lastCode1c.Replace(Nomenclature.PrefixOfCode1c, ""));//Тут специально падаем в эксепшен если не смогли распарсить, подума 5 раз, пережде чем заменить на TryParse
			}
			id++;
			string format = new string('0', Nomenclature.LengthOfCode1c - Nomenclature.PrefixOfCode1c.Length);
			return Nomenclature.PrefixOfCode1c + id.ToString(format);
		}

		public QueryOver<Nomenclature> NomenclatureInGroupsQuery(int[] groupsIds)
		{
			return QueryOver.Of<Nomenclature>()
							.Where(n => n.ProductGroup.Id.IsIn(groupsIds));
		}

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
			WarehouseBulkGoodsAccountingOperation operationAlias = null;

			var subqueryBalance = QueryOver.Of(() => operationAlias)
				.Where(() => operationAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseBulkGoodsAccountingOperation>(o => o.Warehouse)))
				.Select(Projections.Sum<WarehouseBulkGoodsAccountingOperation>(o => o.Amount));

			//Подзапрос выбирающий по номенклатуре количество зарезервированное в заказах до отгрузки со склада
			Domain.Orders.Order localOrderAlias = null;
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
					.SelectSubQuery(subqueryBalance).WithAlias(() => resultAlias.InStock)
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
			foreach(var n in nomenclatures)
			{
				Nomenclature nomenclatureAlias = null;
				var certificates = uow.Session.QueryOver<Certificate>()
									   .Left.JoinAlias(c => c.Nomenclatures, () => nomenclatureAlias)
									   .Where(() => nomenclatureAlias.Id == n.Id)
									   .List()
									   ;
				if(certificates.Any())
				{
					if(!dict.ContainsKey(n))
						dict.Add(n, certificates);
					else
					{
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

		public Nomenclature GetWaterSemiozerie(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(_nomenclatureSettings.WaterSemiozerieId);
		}

		public Nomenclature GetWaterKislorodnaya(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(_nomenclatureSettings.WaterKislorodnayaId);
		}

		public Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(_nomenclatureSettings.WaterSnyatogorskayaId);
		}

		public Nomenclature GetWaterKislorodnayaDeluxe(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(_nomenclatureSettings.WaterKislorodnayaDeluxeId);
		}

		public Nomenclature GetWaterStroika(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(_nomenclatureSettings.WaterStroikaId);
		}

		public Nomenclature GetWaterRuchki(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(_nomenclatureSettings.WaterRuchkiId);
		}

		public Nomenclature GetDefaultBottleNomenclature(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(_nomenclatureSettings.DefaultBottleNomenclatureId);
		}

		public async Task<int?> GetDefaultBottleNomenclatureId(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var defaultBottleNomenclatures =
				await uow.Session.Query<NomenclatureEntity>()
				.Where(x => x.Id == _nomenclatureSettings.DefaultBottleNomenclatureId)
				.Select(x => x.Id)
				.ToListAsync(cancellationToken);

			return defaultBottleNomenclatures.FirstOrDefault();
		}

		public Nomenclature GetNomenclatureToAddWithMaster(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(_nomenclatureSettings.NomenclatureToAddWithMasterId);
		}

		public Nomenclature GetForfeitNomenclature(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(_nomenclatureSettings.ForfeitId);
		}

		public Nomenclature GetFastDeliveryNomenclature(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(_nomenclatureSettings.FastDeliveryNomenclatureId);
		}
		public Nomenclature GetMasterCallNomenclature(IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature>(_nomenclatureSettings.MasterCallNomenclatureId);
		}

		#endregion

		public decimal GetWaterPriceIncrement => _nomenclatureSettings.GetWaterPriceIncrement;

		public int GetIdentifierOfOnlineShopGroup() => _nomenclatureSettings.IdentifierOfOnlineShopGroup;

		public Nomenclature GetNomenclature(IUnitOfWork uow, int nomenclatureId) => uow.GetById<Nomenclature>(nomenclatureId);

		public async Task<IList<int>> Get19LWaterNomenclatureIds(
			IUnitOfWork uow, 
			int[] siteNomenclaturesIds,
			CancellationToken cancellationToken
			)
		{
			return await uow.Session.QueryOver<Nomenclature>()
				.WhereRestrictionOn(n => n.Id).IsIn(siteNomenclaturesIds)
				.And(n => n.Category == NomenclatureCategory.water)
				.And(n => n.TareVolume == TareVolume.Vol19L)
				.Select(n => n.Id)
				.ListAsync<int>(cancellationToken);
		}

		public IList<NomenclatureOnlineParametersNode> GetActiveNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			Nomenclature nomenclatureAlias = null;
			NomenclatureOnlineParametersNode resultAlias = null;

			return uow.Session.QueryOver<NomenclatureOnlineParameters>()
				.Left.JoinAlias(p => p.Nomenclature, () => nomenclatureAlias)
				.Where(p => p.Type == parameterType)
				.And(p => p.NomenclatureOnlineAvailability != null)
				.And(() => !nomenclatureAlias.IsArchive)
				.SelectList(list => list
					.Select(p => p.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(p => p.NomenclatureOnlineAvailability).WithAlias(() => resultAlias.AvailableForSale)
					.Select(p => p.NomenclatureOnlineMarker).WithAlias(() => resultAlias.Marker)
					.Select(p => p.NomenclatureOnlineDiscount).WithAlias(() => resultAlias.PercentDiscount))
				.TransformUsing(Transformers.AliasToBean<NomenclatureOnlineParametersNode>())
				.List<NomenclatureOnlineParametersNode>();
		}

		public IList<NomenclatureOnlinePriceNode> GetNomenclaturesOnlinePricesByOnlineParameters(
			IUnitOfWork uow, IEnumerable<int> onlineParametersIds)
		{
			NomenclaturePriceBase nomenclaturePriceAlias = null;
			NomenclatureOnlinePriceNode resultAlias = null;

			return uow.Session.QueryOver<NomenclatureOnlinePrice>()
				.Left.JoinAlias(p => p.NomenclaturePrice, () => nomenclaturePriceAlias)
				.WhereRestrictionOn(p => p.NomenclatureOnlineParameters.Id).IsInG(onlineParametersIds)
				.SelectList(list => list
					.Select(p => p.Id).WithAlias(() => resultAlias.Id)
					.Select(p => p.NomenclatureOnlineParameters.Id).WithAlias(() => resultAlias.NomenclatureOnlineParametersId)
					.Select(p => p.PriceWithoutDiscount).WithAlias(() => resultAlias.PriceWithoutDiscount)
					.Select(() => nomenclaturePriceAlias.MinCount).WithAlias(() => resultAlias.MinCount)
					.Select(() => nomenclaturePriceAlias.Price).WithAlias(() => resultAlias.Price))
				.TransformUsing(Transformers.AliasToBean<NomenclatureOnlinePriceNode>())
				.List<NomenclatureOnlinePriceNode>();
		}

		public IList<OnlineNomenclatureNode> GetNomenclaturesForSend(IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			Nomenclature nomenclatureAlias = null;
			MobileAppNomenclatureOnlineCatalog mobileAppNomenclatureOnlineCatalogAlias = null;
			VodovozWebSiteNomenclatureOnlineCatalog vodovozWebSiteNomenclatureOnlineCatalogAlias = null;
			KulerSaleWebSiteNomenclatureOnlineCatalog kulerSaleWebSiteNomenclatureOnlineCatalogAlias = null;
			NomenclatureOnlineGroup nomenclatureOnlineGroupAlias = null;
			NomenclatureOnlineCategory nomenclatureOnlineCategoryAlias = null;
			NomenclatureOnlineParameters onlineParametersAlias = null;
			OnlineNomenclatureNode resultAlias = null;

			var query = uow.Session.QueryOver(() => nomenclatureAlias)
				.Left.JoinAlias(n => n.NomenclatureOnlineGroup, () => nomenclatureOnlineGroupAlias)
				.Left.JoinAlias(n => n.NomenclatureOnlineCategory, () => nomenclatureOnlineCategoryAlias)
				.Left.JoinAlias(n => n.MobileAppNomenclatureOnlineCatalog,
					() => mobileAppNomenclatureOnlineCatalogAlias)
				.Left.JoinAlias(n => n.VodovozWebSiteNomenclatureOnlineCatalog,
					() => vodovozWebSiteNomenclatureOnlineCatalogAlias)
				.Left.JoinAlias(n => n.KulerSaleWebSiteNomenclatureOnlineCatalog,
					() => kulerSaleWebSiteNomenclatureOnlineCatalogAlias)
				.JoinEntityAlias(
					() => onlineParametersAlias,
					() => onlineParametersAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And(() => onlineParametersAlias.NomenclatureOnlineAvailability != null)
				.Where(n => !n.IsArchive);

			var queryBuilder = new QueryOverProjectionBuilder<Nomenclature>()
				.Select(n => n.Id).WithAlias(() => resultAlias.ErpId)
				.Select(n => n.OnlineName).WithAlias(() => resultAlias.OnlineName)
				.Select(() => nomenclatureOnlineGroupAlias.Name).WithAlias(() => resultAlias.OnlineGroup)
				.Select(() => nomenclatureOnlineCategoryAlias.Name).WithAlias(() => resultAlias.OnlineCategory)
				.Select(n => n.TareVolume).WithAlias(() => resultAlias.TareVolume)
				.Select(n => n.IsDisposableTare).WithAlias(() => resultAlias.IsDisposableTare)
				.Select(n => n.IsNewBottle).WithAlias(() => resultAlias.IsNewBottle)
				.Select(n => n.IsSparklingWater).WithAlias(() => resultAlias.IsSparklingWater)
				.Select(n => n.EquipmentInstallationType).WithAlias(() => resultAlias.EquipmentInstallationType)
				.Select(n => n.EquipmentWorkloadType).WithAlias(() => resultAlias.EquipmentWorkloadType)
				.Select(n => n.PumpType).WithAlias(() => resultAlias.PumpType)
				.Select(n => n.CupHolderBracingType).WithAlias(() => resultAlias.CupHolderBracingType)
				.Select(n => n.HasHeating).WithAlias(() => resultAlias.HasHeating)
				.Select(n => n.NewHeatingPower).WithAlias(() => resultAlias.HeatingPower)
				.Select(n => n.HeatingProductivity).WithAlias(() => resultAlias.HeatingProductivity)
				.Select(n => n.ProtectionOnHotWaterTap).WithAlias(() => resultAlias.ProtectionOnHotWaterTap)
				.Select(n => n.HasCooling).WithAlias(() => resultAlias.HasCooling)
				.Select(n => n.NewCoolingPower).WithAlias(() => resultAlias.CoolingPower)
				.Select(n => n.CoolingProductivity).WithAlias(() => resultAlias.CoolingProductivity)
				.Select(n => n.NewCoolingType).WithAlias(() => resultAlias.CoolingType)
				.Select(n => n.LockerRefrigeratorType).WithAlias(() => resultAlias.LockerRefrigeratorType)
				.Select(n => n.LockerRefrigeratorVolume).WithAlias(() => resultAlias.LockerRefrigeratorVolume)
				.Select(n => n.TapType).WithAlias(() => resultAlias.TapType)
				.Select(n => n.GlassHolderType).WithAlias(() => resultAlias.GlassHolderType)
				.Select(n => n.HeatingTemperatureFromOnline).WithAlias(() => resultAlias.HeatingTemperatureFrom)
				.Select(n => n.HeatingTemperatureToOnline).WithAlias(() => resultAlias.HeatingTemperatureTo)
				.Select(n => n.CoolingTemperatureFromOnline).WithAlias(() => resultAlias.CoolingTemperatureFrom)
				.Select(n => n.CoolingTemperatureToOnline).WithAlias(() => resultAlias.CoolingTemperatureTo)
				.Select(n => n.LengthOnline).WithAlias(() => resultAlias.Length)
				.Select(n => n.WidthOnline).WithAlias(() => resultAlias.Width)
				.Select(n => n.HeightOnline).WithAlias(() => resultAlias.Height)
				.Select(n => n.WeightOnline).WithAlias(() => resultAlias.Weight)
				.Select(n => n.HeatingPowerUnits).WithAlias(() => resultAlias.HeatingPowerUnits)
				.Select(n => n.CoolingPowerUnits).WithAlias(() => resultAlias.CoolingPowerUnits)
				.Select(n => n.HeatingProductivityUnits).WithAlias(() => resultAlias.HeatingProductivityUnits)
				.Select(n => n.CoolingProductivityUnits).WithAlias(() => resultAlias.CoolingProductivityUnits)
				.Select(n => n.HeatingProductivityComparisionSign).WithAlias(() => resultAlias.HeatingProductivityComparisionSign)
				.Select(n => n.CoolingProductivityComparisionSign).WithAlias(() => resultAlias.CoolingProductivityComparisionSign);

			switch(parameterType)
			{
				case GoodsOnlineParameterType.ForMobileApp:
					query.And(n => n.MobileAppNomenclatureOnlineCatalog != null)
						.And(() => onlineParametersAlias.Type == GoodsOnlineParameterType.ForMobileApp);
					queryBuilder.Select(() => mobileAppNomenclatureOnlineCatalogAlias.ExternalId)
						.WithAlias(() => resultAlias.OnlineCatalogGuid);
					break;
				case GoodsOnlineParameterType.ForVodovozWebSite:
					query.And(n => n.VodovozWebSiteNomenclatureOnlineCatalog != null)
						.And(() => onlineParametersAlias.Type == GoodsOnlineParameterType.ForVodovozWebSite);
					queryBuilder.Select(() => vodovozWebSiteNomenclatureOnlineCatalogAlias.ExternalId)
						.WithAlias(() => resultAlias.OnlineCatalogGuid);
					break;
				case GoodsOnlineParameterType.ForKulerSaleWebSite:
					query.And(n => n.KulerSaleWebSiteNomenclatureOnlineCatalog != null)
						.And(() => onlineParametersAlias.Type == GoodsOnlineParameterType.ForKulerSaleWebSite);
					queryBuilder.Select(() => kulerSaleWebSiteNomenclatureOnlineCatalogAlias.ExternalId)
						.WithAlias(() => resultAlias.OnlineCatalogGuid);
					break;
			}

			query.SelectList(builder => queryBuilder)
			.TransformUsing(Transformers.AliasToBean<OnlineNomenclatureNode>());

			return query.List<OnlineNomenclatureNode>();
		}

		public IEnumerable<INamedDomainObject> GetPromoSetsWithNomenclature(
			IUnitOfWork unitOfWork, int nomenclatureId, bool notArchive = true)
		{
			PromotionalSet promoSetAlias = null;
			PromotionalSetItem promoSetItemAlias = null;
			NamedDomainObjectNode resultAlias = null;

			var query = unitOfWork.Session.QueryOver(() => promoSetAlias)
				.JoinAlias(p => p.PromotionalSetItems, () => promoSetItemAlias)
				.Where(() => promoSetItemAlias.Nomenclature.Id == nomenclatureId);

			if(notArchive)
			{
				query.Where(p => !p.IsArchive);
			}

			return query.SelectList(list => list
				.Select(Projections.Distinct(Projections.Property(() => promoSetAlias.Id)).WithAlias(() => resultAlias.Id))
				.Select(p => p.Name).WithAlias(() => resultAlias.Name))
			.TransformUsing(Transformers.AliasToBean<NamedDomainObjectNode>())
			.List<NamedDomainObjectNode>();
		}

		public bool CheckAnyOrderWithNomenclature(IUnitOfWork unitOfWork, int nomenclatureId)
		{
			if(nomenclatureId == 0) 
				return false;
			
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			
			return unitOfWork.Session.QueryOver(() => orderItemAlias)
				.JoinAlias(o => o.Nomenclature, () => nomenclatureAlias)
				.Where(() => orderItemAlias.Nomenclature.Id == nomenclatureId)
				.RowCount() > 0;
		}
	}
}

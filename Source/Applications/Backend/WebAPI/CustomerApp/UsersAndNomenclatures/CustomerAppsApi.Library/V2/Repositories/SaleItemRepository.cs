using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto;
using CustomerAppsApi.Library.V2.Dto.Goods;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Criterion.Lambda;
using NHibernate.Linq;
using NHibernate.Multi;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;

namespace CustomerAppsApi.Library.V2.Repositories
{
	public class SaleItemRepository : ISaleItemRepository
	{
		private const string _nomenclaturesBatchKey = "nomenclatures";
		private const string _nomenclatureParametersBatchKey = "nomenclatureParameters";
		private const string _nomenclaturePricesBatchKey = "nomenclaturePrices";
		private const string _nomenclatureStocksBatchKey = "nomenclatureStocks";
		private const string _promoSetsBatchKey = "promoSets";
		private const string _promoSetParametersBatchKey = "promoSetParameters";
		private const string _promoSetItemPricesBatchKey = "promoSetItemPrices";
		private const string _rentPackagesBatchKey = "rentPackages";
		private const string _rentPackagePricesBatchKey = "rentPackagePrices";
		
		/// <inheritdoc/>
		public async Task<AggregatedSaleItems> GetAggregatedSaleItemsAsync(IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			var batch = uow.Session.CreateQueryBatch();

			var nomenclaturesBatch = GetQueryOverNomenclaturesForSend(uow, parameterType);
			var promoSetsBatch = GetIQueryableActivePromotionalSetsForSend(uow, parameterType);
			var rentPackagesBatch = GetIQueryableFreeRentPackagesForSend(uow, parameterType);

			batch
				.Add<OnlineNomenclatureDto>(_nomenclaturesBatchKey, nomenclaturesBatch)
				.Add<PromotionalSetDto>(_promoSetsBatchKey, promoSetsBatch)
				.Add<FreeRentPackageDto>(_rentPackagesBatchKey, rentPackagesBatch)
				;

			await batch.ExecuteAsync();

			var nomenclatures = await batch.GetResultAsync<OnlineNomenclatureDto>(_nomenclaturesBatchKey);
			var promoSets = await batch.GetResultAsync<PromotionalSetDto>(_promoSetsBatchKey);
			var rentPackages = await batch.GetResultAsync<FreeRentPackageDto>(_rentPackagesBatchKey);

			return AggregatedSaleItems.Create(nomenclatures, promoSets, rentPackages);
		}
		
		/// <inheritdoc/>
		public async Task<AggregatedSaleItemPrices> GetAggregatedSaleItemPricesAsync(IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			var batch = uow.Session.CreateQueryBatch();

			var nomenclatureParametersBatch = GetIQueryableNomenclaturesOnlineParametersForSend(uow, parameterType);
			var nomenclaturePricesBatch = GetIQueryableNomenclaturesOnlinePricesForSend(uow, parameterType);
			var promoSetParametersBatch = GetIQueryablePromoSetOnlineParametersForSend(uow, parameterType);
			var rentPackagePricesBatch = GetIQueryableFreeRentPackagePricesForSend(uow, parameterType);

			batch
				.Add<NomenclatureOnlineParametersDto>(_nomenclatureParametersBatchKey, nomenclatureParametersBatch)
				.Add<NomenclatureOnlinePriceDto>(_nomenclaturePricesBatchKey, nomenclaturePricesBatch)
				.Add<SaleItemPricesDto>(_promoSetParametersBatchKey, promoSetParametersBatch)
				.Add<SaleItemPricesDto>(_rentPackagePricesBatchKey, rentPackagePricesBatch)
				;

			await batch.ExecuteAsync();

			var nomenclatureParameters = await batch.GetResultAsync<NomenclatureOnlineParametersDto>(_nomenclatureParametersBatchKey);
			var nomenclaturePrices = await batch.GetResultAsync<NomenclatureOnlinePriceDto>(_nomenclaturePricesBatchKey);
			var promoSetParameters = await batch.GetResultAsync<SaleItemPricesDto>(_promoSetParametersBatchKey);
			var rentPackagePrices = await batch.GetResultAsync<SaleItemPricesDto>(_rentPackagePricesBatchKey);

			return AggregatedSaleItemPrices.Create(
				nomenclatureParameters,
				nomenclaturePrices,
				promoSetParameters,
				rentPackagePrices
				);
		}

		/// <inheritdoc/>
		public async Task<IEnumerable<NomenclatureOnlineParametersDto>> GetNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType
			)
		{
			return await GetIQueryableNomenclaturesOnlineParametersForSend(uow, parameterType)
				.ToListAsync();
		}
		
		/// <inheritdoc/>
		public async Task<IEnumerable<NomenclatureOnlinePriceDto>> GetNomenclaturesOnlinePricesForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType
		)
		{
			return await GetIQueryableNomenclaturesOnlinePricesForSend(uow, parameterType)
				.ToListAsync();
		}
		
		/// <inheritdoc/>
		public async Task<IEnumerable<SaleItemPricesDto>> GetPromoSetOnlineParametersForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType
		)
		{
			return await GetIQueryablePromoSetOnlineParametersForSend(uow, parameterType)
				.ToListAsync();
		}
		
		/// <inheritdoc/>
		public async Task<IEnumerable<SaleItemPricesDto>> GetFreeRentPackagePricesForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType
		)
		{
			return await GetIQueryableFreeRentPackagePricesForSend(uow, parameterType)
				.ToListAsync();
		}
		
		/// <inheritdoc/>
		public async Task<IEnumerable<PromotionalSetItemBalanceDto>> GetPromotionalSetsItemsWithBalanceForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType,
			IEnumerable<int> warehouseIds
		)
		{
			return await GetQueryOverPromotionalSetsItemsWithBalanceForSend(parameterType, warehouseIds)
				.DetachedCriteria
				.GetExecutableCriteria(uow.Session)
				.ListAsync<PromotionalSetItemBalanceDto>();
		}

		/// <inheritdoc/>
		public async Task<IEnumerable<(int NomenclatureId, decimal Stock)>> GetNomenclaturesForSendInStock(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType,
			IEnumerable<int> warehouseIds
		)
		{
			return await IQueryOverNomenclaturesForSendInStock(parameterType, warehouseIds)
				.DetachedCriteria
				.GetExecutableCriteria(uow.Session)
				.ListAsync<(int NomenclatureId, decimal Stock)>();
		}
		
		private IQueryOver GetQueryOverNomenclaturesForSend(IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			Nomenclature nomenclatureAlias = null;
			MobileAppNomenclatureOnlineCatalog mobileAppNomenclatureOnlineCatalogAlias = null;
			VodovozWebSiteNomenclatureOnlineCatalog vodovozWebSiteNomenclatureOnlineCatalogAlias = null;
			KulerSaleWebSiteNomenclatureOnlineCatalog kulerSaleWebSiteNomenclatureOnlineCatalogAlias = null;
			NomenclatureOnlineGroup nomenclatureOnlineGroupAlias = null;
			NomenclatureOnlineCategory nomenclatureOnlineCategoryAlias = null;
			NomenclatureOnlineParameters onlineParametersAlias = null;
			OnlineNomenclatureDto resultAlias = null;

			var query = QueryOver.Of(() => nomenclatureAlias)
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
				.Select(n => n.Category).WithAlias(() => resultAlias.Category)
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
				.TransformUsing(Transformers.AliasToBean<OnlineNomenclatureDto>());

			return query;
		}
		
		private IQueryable<PromotionalSetDto> GetIQueryableActivePromotionalSetsForSend(IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			return 
				from onlineParameters in uow.Session.Query<PromotionalSetOnlineParameters>()
				join promoSet in uow.Session.Query<PromotionalSet>()
					on onlineParameters.PromotionalSet.Id equals promoSet.Id
				where onlineParameters.PromotionalSetOnlineAvailability != null
					&& onlineParameters.Type == parameterType
					&& !promoSet.IsArchive
				select new PromotionalSetDto
				{
					Id = promoSet.Id,
					OnlineName = promoSet.OnlineName,
					ForNewClients = promoSet.PromotionalSetForNewClients,
				};
		}
		
		private IQueryable<FreeRentPackageDto> GetIQueryableFreeRentPackagesForSend(IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			return 
				from rentPackage in uow.Session.Query<FreeRentPackage>()
				join onlineParameters in uow.Session.Query<FreeRentPackageOnlineParameters>()
					on rentPackage.Id equals onlineParameters.FreeRentPackage.Id
				join depositNomenclature in uow.Session.Query<Nomenclature>()
					on rentPackage.DepositService.Id equals depositNomenclature.Id
				where onlineParameters.PackageOnlineAvailability != null
					&& onlineParameters.Type == parameterType
				select new FreeRentPackageDto
				{
					ErpId = rentPackage.Id,
					OnlineName = rentPackage.OnlineName,
					MinWaterAmount = rentPackage.MinWaterAmount,
					OnlineAvailability = onlineParameters.PackageOnlineAvailability
				};
		}
		
		private IQueryable<NomenclatureOnlineParametersDto> GetIQueryableNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			return from onlineParameters in uow.Session.Query<NomenclatureOnlineParameters>()
				join nomenclature in uow.Session.Query<Nomenclature>()
					on onlineParameters.Nomenclature.Id equals nomenclature.Id
				where onlineParameters.Type == parameterType
					&& onlineParameters.NomenclatureOnlineAvailability != null
					&& !nomenclature.IsArchive
				select new NomenclatureOnlineParametersDto
				{
					Id = onlineParameters.Id,
					NomenclatureId = nomenclature.Id,
					Category = nomenclature.Category,
					AvailableForSale = onlineParameters.NomenclatureOnlineAvailability,
					Marker = onlineParameters.NomenclatureOnlineMarker,
					PercentDiscount = onlineParameters.NomenclatureOnlineDiscount
				};
		}
		
		private IQueryable<NomenclatureOnlinePriceDto> GetIQueryableNomenclaturesOnlinePricesForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			return from onlinePrice in uow.Session.Query<NomenclatureOnlinePrice>()
				join nomenclaturePrice in uow.Session.Query<NomenclaturePriceBase>()
					on onlinePrice.NomenclaturePrice.Id  equals nomenclaturePrice.Id
				join onlineParameters in uow.Session.Query<NomenclatureOnlineParameters>()
					on onlinePrice.NomenclatureOnlineParameters.Id equals onlineParameters.Id
				join nomenclature in uow.Session.Query<Nomenclature>()
					on onlineParameters.Nomenclature.Id equals nomenclature.Id
				where onlineParameters.Type == parameterType
					&& onlineParameters.NomenclatureOnlineAvailability != null
					&& !nomenclature.IsArchive
				select new NomenclatureOnlinePriceDto
				{
					Id = onlinePrice.Id,
					NomenclatureOnlineParametersId = onlinePrice.NomenclatureOnlineParameters.Id,
					PriceWithoutDiscount = onlinePrice.PriceWithoutDiscount,
					MinCount = nomenclaturePrice.MinCount,
					Price = nomenclaturePrice.Price
				}
				;
		}
		
		private QueryOver<Nomenclature> IQueryOverNomenclaturesForSendInStock(
			GoodsOnlineParameterType parameterType,
			IEnumerable<int> warehouseIds = null
		)
		{
			Nomenclature nomenclatureAlias = null;
			NomenclatureOnlineParameters onlineParameters = null;
			WarehouseBulkGoodsAccountingOperation operationAlias = null;

			var query = QueryOver.Of(() => nomenclatureAlias)
				.JoinEntityAlias(
					() => operationAlias,
					() => nomenclatureAlias.Id == operationAlias.Nomenclature.Id,
					JoinType.LeftOuterJoin)
				.JoinAlias(() => nomenclatureAlias.NomenclatureOnlineParameters, () => onlineParameters)
				.Where(() => onlineParameters.NomenclatureOnlineAvailability != null)
				.And(() => onlineParameters.Type == parameterType)
				.And(() => !nomenclatureAlias.IsArchive);

			if(warehouseIds != null && warehouseIds.Any())
			{
				query.AndRestrictionOn(() => operationAlias.Warehouse.Id).IsInG(warehouseIds);
			}

			return query.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id)
					.Select(Projections.Sum(() => operationAlias.Amount)
					)
				)
				.TransformUsing(Transformers.AliasToBeanConstructor(typeof(ValueTuple<int, decimal>).GetConstructors().First()));
		}
		
		private IQueryable<SaleItemPricesDto> GetIQueryablePromoSetOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			return from onlineParameters in uow.Session.Query<PromotionalSetOnlineParameters>()
				join promoSet in uow.Session.Query<PromotionalSet>()
					on onlineParameters.PromotionalSet.Id equals promoSet.Id
				where onlineParameters.Type == parameterType
					&& onlineParameters.PromotionalSetOnlineAvailability != null
					&& !promoSet.IsArchive
				select new SaleItemPricesDto
				{
					ErpId = promoSet.Id,
					AvailableForSale = onlineParameters.PromotionalSetOnlineAvailability,
					Marker = null,
					Type = SaleItemType.PromoSet,
					PercentDiscount = null
				};
		}
		
		private QueryOver<PromotionalSetOnlineParameters> GetQueryOverPromotionalSetsItemsWithBalanceForSend(
			GoodsOnlineParameterType parameterType,
			IEnumerable<int> warehouses)
		{
			PromotionalSet promotionalSetAlias = null;
			PromotionalSetItem promotionalSetItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			Nomenclature nomenclature2Alias = null;
			Nomenclature nomenclature3Alias = null;
			Nomenclature dependOnNomenclatureAlias = null;
			NomenclaturePrice nomenclaturePriceAlias = null;
			NomenclaturePrice dependOnNomenclaturePriceAlias = null;
			WarehouseBulkGoodsAccountingOperation operationAlias = null;
			PromotionalSetItemBalanceDto resultAlias = null;

			var discountProjection = Projections.Conditional(
				Restrictions.Where(() => promotionalSetItemAlias.IsDiscountInMoney),
				Projections.Property(() => promotionalSetItemAlias.DiscountMoney),
				Projections.Property(() => promotionalSetItemAlias.Discount));

			var balanceSubQuery = QueryOver.Of(() => nomenclature2Alias)
				.JoinEntityAlias(
					() => operationAlias,
					() => nomenclature2Alias.Id == operationAlias.Nomenclature.Id,
					JoinType.LeftOuterJoin)
				.Where(() => nomenclatureAlias.Id == nomenclature2Alias.Id)
				.AndRestrictionOn(() => operationAlias.Warehouse).IsInG(warehouses)
				.Select(Projections.Sum(() => operationAlias.Amount));
			
			var nomenclaturePriceSubquery = QueryOver.Of(() => nomenclaturePriceAlias)
				.Where(() => nomenclaturePriceAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And(() => nomenclaturePriceAlias.MinCount == 1)
				.Select(Projections.Property(() => nomenclaturePriceAlias.Price));
			
			var dependOnNomenclaturePriceSubquery = QueryOver.Of(() => dependOnNomenclaturePriceAlias)
				.Where(() => dependOnNomenclaturePriceAlias.Nomenclature.Id == nomenclatureAlias.DependsOnNomenclature.Id)
				.And(() => dependOnNomenclaturePriceAlias.MinCount == 1)
				.Select(Projections.Property(() => dependOnNomenclaturePriceAlias.Price));

			return QueryOver.Of<PromotionalSetOnlineParameters>()
				.Left.JoinAlias(p => p.PromotionalSet, () => promotionalSetAlias)
				.Left.JoinAlias(() => promotionalSetAlias.PromotionalSetItems, () => promotionalSetItemAlias)
				.Left.JoinAlias(() => promotionalSetItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(p => p.Type == parameterType)
				.And(p => p.PromotionalSetOnlineAvailability != null)
				.And(() => !promotionalSetAlias.IsArchive)
				.SelectList(list => list
					.Select(() => promotionalSetAlias.Id).WithAlias(() => resultAlias.PromotionalSetId)
					.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => promotionalSetItemAlias.Count).WithAlias(() => resultAlias.Count)
					.Select(discountProjection).WithAlias(() => resultAlias.Discount)
					.Select(() => promotionalSetItemAlias.IsDiscountInMoney).WithAlias(() => resultAlias.IsDiscountMoney)
					.SelectSubQuery(balanceSubQuery).WithAlias(() => resultAlias.Stock)
					.Select(Projections.Conditional(
						Restrictions.IsNull(Projections.Property(() => nomenclatureAlias.DependsOnNomenclature)),
						Projections.SubQuery(nomenclaturePriceSubquery),
						Projections.SubQuery(dependOnNomenclaturePriceSubquery)
						)
					).WithAlias(() => resultAlias.NomenclaturePrice)
				)
				.TransformUsing(Transformers.AliasToBean<PromotionalSetItemBalanceDto>());
		}
		
		private IQueryable<SaleItemPricesDto> GetIQueryableFreeRentPackagePricesForSend(IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			return 
				from onlineParameters in uow.Session.Query<FreeRentPackageOnlineParameters>()
				join rentPackage in uow.Session.Query<FreeRentPackage>()
					on onlineParameters.FreeRentPackage.Id equals rentPackage.Id
				where onlineParameters.PackageOnlineAvailability != null
					&& onlineParameters.Type == parameterType
				select new SaleItemPricesDto
				{
					ErpId = rentPackage.Id,
					AvailableForSale = onlineParameters.PackageOnlineAvailability,
					Marker = null,
					Type = SaleItemType.RentPackage,
					PercentDiscount = null,
					Prices = new []
					{
						new SaleItemPriceDto
						{
							MinCount = 1,
							Price = rentPackage.Deposit,
							PriceWithoutDiscount = null,
							SaleItemId = rentPackage.Id,
						}
					}
				};
		}
	}
}

using System.Collections.Generic;
using CustomerAppsApi.Library.V1.Dto;
using CustomerAppsApi.Library.V1.Dto.Goods;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Criterion.Lambda;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.V1.Repositories
{
	public class SaleItemRepository : ISaleItemRepository
	{
		/// <inheritdoc/>
		public IEnumerable<OnlineNomenclatureDto> GetNomenclaturesForSend(IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			Nomenclature nomenclatureAlias = null;
			MobileAppNomenclatureOnlineCatalog mobileAppNomenclatureOnlineCatalogAlias = null;
			VodovozWebSiteNomenclatureOnlineCatalog vodovozWebSiteNomenclatureOnlineCatalogAlias = null;
			KulerSaleWebSiteNomenclatureOnlineCatalog kulerSaleWebSiteNomenclatureOnlineCatalogAlias = null;
			NomenclatureOnlineGroup nomenclatureOnlineGroupAlias = null;
			NomenclatureOnlineCategory nomenclatureOnlineCategoryAlias = null;
			NomenclatureOnlineParameters onlineParametersAlias = null;
			OnlineNomenclatureDto resultAlias = null;

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
				.TransformUsing(Transformers.AliasToBean<OnlineNomenclatureDto>());

			return query.List<OnlineNomenclatureDto>();
		}
		
		/// <inheritdoc/>
		public IEnumerable<NomenclatureOnlineParametersDto> GetActiveNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			Nomenclature nomenclatureAlias = null;
			NomenclatureOnlineParametersDto resultAlias = null;

			return uow.Session.QueryOver<NomenclatureOnlineParameters>()
				.Left.JoinAlias(p => p.Nomenclature, () => nomenclatureAlias)
				.Where(p => p.Type == parameterType)
				.And(p => p.NomenclatureOnlineAvailability != null)
				.And(() => !nomenclatureAlias.IsArchive)
				.SelectList(list => list
					.Select(p => p.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(Projections.Conditional(
						Restrictions.Eq(Projections.Property(() => nomenclatureAlias.Category), NomenclatureCategory.master),
						Projections.Constant(true),
						Projections.Constant(false))
					).WithAlias(() => resultAlias.IsService)
					.Select(p => p.NomenclatureOnlineAvailability).WithAlias(() => resultAlias.AvailableForSale)
					.Select(p => p.NomenclatureOnlineMarker).WithAlias(() => resultAlias.Marker)
					.Select(p => p.NomenclatureOnlineDiscount).WithAlias(() => resultAlias.PercentDiscount))
				.TransformUsing(Transformers.AliasToBean<NomenclatureOnlineParametersDto>())
				.List<NomenclatureOnlineParametersDto>();
		}

		/// <inheritdoc/>
		public IEnumerable<NomenclatureOnlinePriceDto> GetNomenclaturesOnlinePricesByOnlineParameters(
			IUnitOfWork uow, IEnumerable<int> onlineParametersIds)
		{
			NomenclaturePriceBase nomenclaturePriceAlias = null;
			NomenclatureOnlinePriceDto resultAlias = null;

			return uow.Session.QueryOver<NomenclatureOnlinePrice>()
				.Left.JoinAlias(p => p.NomenclaturePrice, () => nomenclaturePriceAlias)
				.WhereRestrictionOn(p => p.NomenclatureOnlineParameters.Id).IsInG(onlineParametersIds)
				.SelectList(list => list
					.Select(p => p.Id).WithAlias(() => resultAlias.Id)
					.Select(p => p.NomenclatureOnlineParameters.Id).WithAlias(() => resultAlias.NomenclatureOnlineParametersId)
					.Select(p => p.PriceWithoutDiscount).WithAlias(() => resultAlias.PriceWithoutDiscount)
					.Select(() => nomenclaturePriceAlias.MinCount).WithAlias(() => resultAlias.MinCount)
					.Select(() => nomenclaturePriceAlias.Price).WithAlias(() => resultAlias.Price))
				.TransformUsing(Transformers.AliasToBean<NomenclatureOnlinePriceDto>())
				.List<NomenclatureOnlinePriceDto>();
		}
	}
}

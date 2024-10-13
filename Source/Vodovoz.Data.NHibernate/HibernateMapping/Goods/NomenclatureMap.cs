﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class NomenclatureMap : ClassMap<Nomenclature>
	{
		public NomenclatureMap()
		{
			Table("nomenclature");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreateDate).Column("create_date");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.CanPrintPrice).Column("can_print_price");
			Map(x => x.Name).Column("name");
			Map(x => x.OfficialName).Column("official_name");
			Map(x => x.Model).Column("model");
			Map(x => x.Weight).Column("weight");
			Map(x => x.Length).Column("length");
			Map(x => x.Width).Column("width");
			Map(x => x.Height).Column("height");
			Map(x => x.VAT).Column("vat");
			Map(x => x.DoNotReserve).Column("reserve");
			Map(x => x.RentPriority).Column("rent_priority");
			Map(x => x.IsDuty).Column("is_duty");
			Map(x => x.IsSerial).Column("serial");
			Map(x => x.Category).Column("category");
			Map(x => x.TareVolume).Column("tare_volume");
			Map(x => x.IsDisposableTare).Column("is_disposable_tare");
			Map(x => x.Code1c).Column("code_1c");
			Map(x => x.SumOfDamage).Column("sum_of_damage");
			Map(x => x.ShortName).Column("short_name");
			Map(x => x.Hide).Column("hide");
			Map(x => x.NoDelivery).Column("no_delivery");
			Map(x => x.IsNewBottle).Column("is_new_bottle");
			Map(x => x.IsDefectiveBottle).Column("is_defective_bottle");
			Map(x => x.IsShabbyBottle).Column("is_shabby_bottle");
			Map(x => x.IsDiler).Column("is_diler");
			Map(x => x.PercentForMaster).Column("percent_for_master");
			Map(x => x.TypeOfDepositCategory).Column("type_of_deposit");
			Map(x => x.MasterServiceType).Column("master_service_type");
			Map(x => x.SaleCategory).Column("subtype_of_equipment");
			Map(x => x.OnlineStoreGuid).Column("online_store_guid");
			Map(x => x.MinStockCount).Column("min_stock_count");
			Map(x => x.MobileCatalog).Column("mobile_catalog");
			Map(x => x.Description).Column("description");
			Map(x => x.BottleCapColor).Column("bottle_cap_color");
			Map(x => x.OnlineStoreExternalId).Column("online_store_external_id");
			Map(x => x.UsingInGroupPriceSet).Column("using_in_group_price_set");
			Map(x => x.HasInventoryAccounting).Column("has_inventory_accounting");
			Map(x => x.GlassHolderType).Column("glass_holder_type");

			//Характеристики товара
			Map(x => x.Color).Column("color");
			Map(x => x.Material).Column("material");
			Map(x => x.Liters).Column("liters");
			Map(x => x.Size).Column("size");
			Map(x => x.Package).Column("package");
			Map(x => x.DegreeOfRoast).Column("degree_of_roast");
			Map(x => x.Smell).Column("smell");
			Map(x => x.Taste).Column("taste");
			Map(x => x.RefrigeratorCapacity).Column("refrigerator_capacity");
			Map(x => x.CoolingType).Column("cooling_type");
			Map(x => x.HeatingPower).Column("heating_power");
			Map(x => x.CoolingPower).Column("cooling_power");
			Map(x => x.HeatingPerformance).Column("heating_performance");
			Map(x => x.CoolingPerformance).Column("cooling_performance");
			Map(x => x.NumberOfCartridges).Column("number_of_cartridges");
			Map(x => x.CharacteristicsOfCartridges).Column("characteristics_of_cartridges");
			Map(x => x.CountryOfOrigin).Column("country_of_origin");
			Map(x => x.AmountInAPackage).Column("amount_in_a_package");
			
			Map(x => x.OnlineName).Column("online_name");
			Map(x => x.IsSparklingWater).Column("is_sparkling_water");
			Map(x => x.EquipmentInstallationType).Column("equipment_installation_type");
			Map(x => x.EquipmentWorkloadType).Column("equipment_workload_type");
			Map(x => x.PumpType).Column("pump_type");
			Map(x => x.CupHolderBracingType).Column("cup_holder_bracing_type");
			Map(x => x.HasHeating).Column("has_heating");
			Map(x => x.NewHeatingPower).Column("new_heating_power");
			Map(x => x.HeatingProductivity).Column("heating_productivity");
			Map(x => x.ProtectionOnHotWaterTap).Column("protection_on_hot_water_tap");
			Map(x => x.HasCooling).Column("has_cooling");
			Map(x => x.NewCoolingPower).Column("new_cooling_power");
			Map(x => x.CoolingProductivity).Column("cooling_productivity");
			Map(x => x.NewCoolingType).Column("new_cooling_type");
			Map(x => x.LockerRefrigeratorType).Column("locker_refrigerator_type");
			Map(x => x.LockerRefrigeratorVolume).Column("locker_refrigerator_volume");
			Map(x => x.TapType).Column("tap_type");

			Map(x => x.StorageCell).Column("storage_cell");

			//Планирование продаж для КЦ
			Map(x => x.PlanDay).Column("plan_day");
			Map(x => x.PlanMonth).Column("plan_month");

			//Честный знак
			Map(x => x.IsAccountableInTrueMark).Column("is_accountable_in_chestniy_znak");
			Map(x => x.Gtin).Column("gtin");

			References(x => x.ShipperCounterparty).Column("shipper_counterparty_id");
			References(x => x.CreatedBy).Column("created_by");
			References(x => x.DependsOnNomenclature).Column("depends_on_nomenclature");
			References(x => x.Unit).Column("unit_id").Fetch.Join().Not.LazyLoad();
			References(x => x.EquipmentColor).Column("color_id");
			References(x => x.Kind).Column("kind_id");
			References(x => x.Manufacturer).Column("manufacturer_id");
			References(x => x.RouteListColumn).Column("route_column_id");
			References(x => x.Folder1C).Column("folder_1c_id");
			References(x => x.ProductGroup).Column("group_id");
			References(x => x.FuelType).Column("fuel_type_id");
			References(x => x.OnlineStore).Column("online_store_id");
			References(x => x.MobileAppNomenclatureOnlineCatalog)
				.Column("mobile_app_nomenclature_online_catalog_id");
			References(x => x.VodovozWebSiteNomenclatureOnlineCatalog)
				.Column("vodovoz_web_site_nomenclature_online_catalog_id");
			References(x => x.KulerSaleWebSiteNomenclatureOnlineCatalog)
				.Column("kuler_sale_web_site_nomenclature_online_catalog_id");
			References(x => x.NomenclatureOnlineGroup)
				.Column("nomenclature_online_group_id");
			References(x => x.NomenclatureOnlineCategory)
				.Column("nomenclature_online_category_id");

			HasMany(x => x.NomenclaturePrice)
				.Where($"type='{NomenclaturePriceBase.NomenclaturePriceType.General}'")
				.Inverse().Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("nomenclature_id");

			HasMany(x => x.AlternativeNomenclaturePrices)
				.Where($"type='{NomenclaturePriceBase.NomenclaturePriceType.Alternative}'")
				.Inverse().Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("nomenclature_id");

			HasMany(x => x.AttachedFileInformations).Cascade.AllDeleteOrphan().Inverse().KeyColumn("nomenclature_id");
			HasMany(x => x.CostPrices).Inverse().Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("nomenclature_id");
			HasMany(x => x.PurchasePrices).Inverse().Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("nomenclature_id");
			HasMany(x => x.InnerDeliveryPrices).Inverse().Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("nomenclature_id");
			HasMany(x => x.NomenclatureOnlineParameters)
				.Inverse().Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("nomenclature_id");
		}
	}
}

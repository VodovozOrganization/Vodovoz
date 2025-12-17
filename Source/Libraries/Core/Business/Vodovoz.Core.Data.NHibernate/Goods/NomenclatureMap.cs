using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.NHibernate.Goods
{
	public class NomenclatureMap : ClassMap<NomenclatureEntity>
	{
		public NomenclatureMap()
		{
			Table("nomenclature");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name)
				.Column("name");

			Map(x => x.Category)
				.Column("category");

			//References(x => x.VatRate)
			//	.Column("vat_rate_id");

			Map(x => x.CreateDate)
				.Column("create_date");

			Map(x => x.IsArchive)
				.Column("is_archive");

			Map(x => x.CanPrintPrice)
				.Column("can_print_price");

			Map(x => x.OfficialName)
				.Column("official_name");

			Map(x => x.Model)
				.Column("model");

			Map(x => x.Weight)
				.Column("weight");

			Map(x => x.DoNotReserve)
				.Column("reserve");

			Map(x => x.RentPriority)
				.Column("rent_priority");

			Map(x => x.IsDuty)
				.Column("is_duty");

			Map(x => x.IsSerial)
				.Column("serial");

			Map(x => x.TareVolume)
				.Column("tare_volume");

			Map(x => x.IsDisposableTare)
				.Column("is_disposable_tare");

			Map(x => x.Code1c)
				.Column("code_1c");

			Map(x => x.SumOfDamage)
				.Column("sum_of_damage");

			Map(x => x.ShortName)
				.Column("short_name");

			Map(x => x.Hide)
				.Column("hide");

			Map(x => x.NoDelivery)
				.Column("no_delivery");

			Map(x => x.IsDiler)
				.Column("is_diler");

			Map(x => x.PercentForMaster)
				.Column("percent_for_master");

			Map(x => x.TypeOfDepositCategory)
				.Column("type_of_deposit");

			Map(x => x.SaleCategory)
				.Column("subtype_of_equipment");

			Map(x => x.OnlineStoreGuid)
				.Column("online_store_guid");

			Map(x => x.MinStockCount)
				.Column("min_stock_count");

			Map(x => x.MobileCatalog)
				.Column("mobile_catalog");

			Map(x => x.Description)
				.Column("description");

			Map(x => x.BottleCapColor)
				.Column("bottle_cap_color");

			Map(x => x.OnlineStoreExternalId)
				.Column("online_store_external_id");

			Map(x => x.UsingInGroupPriceSet)
				.Column("using_in_group_price_set");

			Map(x => x.HasInventoryAccounting)
				.Column("has_inventory_accounting");

			Map(x => x.HasConditionAccounting)
				.Column("has_condition_accounting");

			Map(x => x.GlassHolderType)
				.Column("glass_holder_type");

			//Характеристики товара
			Map(x => x.Color)
				.Column("color");

			Map(x => x.Material)
				.Column("material");

			Map(x => x.Liters)
				.Column("liters");

			Map(x => x.Size)
				.Column("size");

			Map(x => x.Package)
				.Column("package");

			Map(x => x.DegreeOfRoast)
				.Column("degree_of_roast");

			Map(x => x.Smell)
				.Column("smell");

			Map(x => x.Taste)
				.Column("taste");

			Map(x => x.RefrigeratorCapacity)
				.Column("refrigerator_capacity");

			Map(x => x.CoolingType)
				.Column("cooling_type");

			Map(x => x.HeatingPower)
				.Column("heating_power");

			Map(x => x.CoolingPower)
				.Column("cooling_power");

			Map(x => x.HeatingPerformance)
				.Column("heating_performance");

			Map(x => x.CoolingPerformance)
				.Column("cooling_performance");

			Map(x => x.NumberOfCartridges)
				.Column("number_of_cartridges");

			Map(x => x.CharacteristicsOfCartridges)
				.Column("characteristics_of_cartridges");

			Map(x => x.CountryOfOrigin)
				.Column("country_of_origin");

			Map(x => x.AmountInAPackage)
				.Column("amount_in_a_package");

			Map(x => x.OnlineName)
				.Column("online_name");

			Map(x => x.IsSparklingWater)
				.Column("is_sparkling_water");

			Map(x => x.EquipmentInstallationType)
				.Column("equipment_installation_type");

			Map(x => x.EquipmentWorkloadType)
				.Column("equipment_workload_type");

			Map(x => x.PumpType)
				.Column("pump_type");

			Map(x => x.CupHolderBracingType)
				.Column("cup_holder_bracing_type");

			Map(x => x.HasHeating)
				.Column("has_heating");

			Map(x => x.NewHeatingPower)
				.Column("new_heating_power");

			Map(x => x.HeatingProductivity)
				.Column("heating_productivity");

			Map(x => x.ProtectionOnHotWaterTap)
				.Column("protection_on_hot_water_tap");

			Map(x => x.HasCooling)
				.Column("has_cooling");

			Map(x => x.NewCoolingPower)
				.Column("new_cooling_power");

			Map(x => x.CoolingProductivity)
				.Column("cooling_productivity");

			Map(x => x.NewCoolingType)
				.Column("new_cooling_type");

			Map(x => x.LockerRefrigeratorType)
				.Column("locker_refrigerator_type");

			Map(x => x.LockerRefrigeratorVolume)
				.Column("locker_refrigerator_volume");

			Map(x => x.TapType)
				.Column("tap_type");

			Map(x => x.HeatingTemperatureFromOnline)
				.Column("heating_temperature_from_online");

			Map(x => x.HeatingTemperatureToOnline)
				.Column("heating_temperature_to_online");

			Map(x => x.CoolingTemperatureFromOnline)
				.Column("cooling_temperature_from_online");

			Map(x => x.CoolingTemperatureToOnline)
				.Column("cooling_temperature_to_online");

			Map(x => x.LengthOnline)
				.Column("length_online");

			Map(x => x.WidthOnline)
				.Column("width_online");

			Map(x => x.HeightOnline)
				.Column("height_online");

			Map(x => x.WeightOnline)
				.Column("weight_online");

			Map(x => x.HeatingPowerUnits)
				.Column("heating_power_units");

			Map(x => x.HeatingProductivityUnits)
				.Column("heating_productivity_units");

			Map(x => x.CoolingPowerUnits)
				.Column("cooling_power_units");

			Map(x => x.CoolingProductivityUnits)
				.Column("cooling_productivity_units");

			Map(x => x.HeatingProductivityComparisionSign)
				.Column("heating_productivity_comparision_sign");

			Map(x => x.CoolingProductivityComparisionSign)
				.Column("cooling_productivity_comparision_sign");

			Map(x => x.StorageCell)
				.Column("storage_cell");
			
			Map(x => x.IsNeedSanitisation)
				.Column("is_need_sanitisation");
			
			//Планирование продаж для КЦ
			Map(x => x.PlanDay)
				.Column("plan_day");

			Map(x => x.PlanMonth)
				.Column("plan_month");

			//Честный знак
			Map(x => x.IsAccountableInTrueMark)
				.Column("is_accountable_in_chestniy_znak");

			Map(x => x.Gtin)
				.Column("gtin");
			
			//Мотивация
			Map(x => x.MotivationUnitType)
				.Column("motivation_unit_type");
			
			Map(x => x.MotivationCoefficient)
				.Column("motivation_coefficient");

			References(x => x.Unit)
				.Column("unit_id")
				.Fetch.Join()
				.Not.LazyLoad();

			References(x => x.DependsOnNomenclature)
				.Column("depends_on_nomenclature");


			HasMany(x => x.AttachedFileInformations)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.KeyColumn("nomenclature_id");

			HasMany(x => x.VatRateVersions)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.KeyColumn("nomenclature_id")
				.OrderBy("start_date DESC");
			
			HasMany(x => x.NomenclaturePrice)
				.Where($"type='{NomenclaturePriceGeneralBase.NomenclaturePriceType.General}'")
				.Inverse()
				.Cascade.AllDeleteOrphan()
				.LazyLoad()
				.KeyColumn("nomenclature_id");

			HasMany(x => x.AlternativeNomenclaturePrices)
				.Where($"type='{NomenclaturePriceGeneralBase.NomenclaturePriceType.Alternative}'")
				.Inverse()
				.Cascade.AllDeleteOrphan()
				.LazyLoad()
				.KeyColumn("nomenclature_id");
			
			HasMany(x => x.PurchasePrices)
				.KeyColumn("nomenclature_id")
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.LazyLoad();

			HasMany(x => x.Gtins)
				.KeyColumn("nomenclature_id")
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.LazyLoad();

			HasMany(x => x.GroupGtins)
				.KeyColumn("nomenclature_id")
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.LazyLoad();
		}
	}
}

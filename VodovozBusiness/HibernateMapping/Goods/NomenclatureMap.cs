using FluentNHibernate.Mapping;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping
{
	public class NomenclatureMap : ClassMap<Nomenclature>
	{
		public NomenclatureMap()
		{
			Table("nomenclature");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreateDate).Column("create_date");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.CanPrintPrice).Column("can_print_price");
			Map(x => x.Name).Column("name");
			Map(x => x.OfficialName).Column("official_name");
			Map(x => x.Model).Column("model");
			Map(x => x.Weight).Column("weight");
			Map(x => x.Volume).Column("volume");
			Map(x => x.VAT).Column("vat").CustomType<VATStringType>();
			Map(x => x.DoNotReserve).Column("reserve");
			Map(x => x.RentPriority).Column("rent_priority");
			Map(x => x.IsDuty).Column("is_duty");
			Map(x => x.IsSerial).Column("serial");
			Map(x => x.Category).Column("category").CustomType<NomenclatureCategoryStringType>();
			Map(x => x.TareVolume).Column("tare_volume").CustomType<TareVolumeStringType>();
			Map(x => x.IsDisposableTare).Column("is_disposable_tare");
			Map(x => x.Code1c).Column("code_1c");
			Map(x => x.SumOfDamage).Column("sum_of_damage");
			Map(x => x.ShortName).Column("short_name");
			Map(x => x.Hide).Column("hide");
			Map(x => x.NoDelivey).Column("no_delivery");
			Map(x => x.IsNewBottle).Column("is_new_bottle");
			Map(x => x.IsDefectiveBottle).Column("is_defective_bottle");
			Map(x => x.IsDiler).Column("is_diler");
			Map(x => x.PercentForMaster).Column("percent_for_master");
			Map(x => x.TypeOfDepositCategory).Column("type_of_deposit").CustomType<TypeOfDepositCategoryStringType>();
			Map(x => x.SubTypeOfEquipmentCategory).Column("subtype_of_equipment").CustomType<SubtypeOfEquipmentCategoryStringType>();
			Map(x => x.OnlineStoreGuid).Column("online_store_guid");
			Map(x => x.MinStockCount).Column("min_stock_count");
			Map(x => x.MobileCatalog).Column("mobile_catalog").CustomType<MobileCatalogStringType>();
			Map(x => x.Description).Column("description");

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

			References(x => x.CreatedBy).Column("created_by");
			References(x => x.DependsOnNomenclature).Column("depends_on_nomenclature");
			References(x => x.Unit).Column("unit_id").Not.LazyLoad();
			References(x => x.EquipmentColor).Column("color_id");
			References(x => x.Type).Column("type_id");
			References(x => x.Manufacturer).Column("manufacturer_id");
			References(x => x.RouteListColumn).Column("route_column_id");
			References(x => x.Warehouse).Column("warehouse_id");
			References(x => x.Folder1C).Column("folder_1c_id");
			References(x => x.ProductGroup).Column("group_id");

			HasMany(x => x.NomenclaturePrice).Inverse().Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("nomenclature_id");
			HasMany(x => x.Images).Inverse().Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("nomenclature_id");
		}
	}
}


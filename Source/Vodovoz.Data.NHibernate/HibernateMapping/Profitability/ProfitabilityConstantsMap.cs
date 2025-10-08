using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Profitability
{
	public class ProfitabilityConstantsMap : ClassMap<ProfitabilityConstants>
	{
		public ProfitabilityConstantsMap()
		{
			Table("profitability_constants");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.CalculatedMonth).Column("calculated_month");
			Map(x => x.AdministrativeExpenses).Column("administartive_expenses");
			Map(x => x.AdministrativeTotalShipped).Column("administartive_total_shipped");
			Map(x => x.AdministrativeExpensesPerKg).Column("administrative_expenses_per_kg");
			Map(x => x.WarehouseExpenses).Column("warehouse_expenses");
			Map(x => x.WarehousesTotalShipped).Column("warehouses_total_shipped");
			Map(x => x.WarehouseExpensesPerKg).Column("warehouse_expenses_per_kg");
			Map(x => x.DecreaseGazelleCostFor3Year).Column("decrease_gazelle_cost_for_3_year");
			Map(x => x.DecreaseLargusCostFor3Year).Column("decrease_largus_cost_for_3_year");
			Map(x => x.DecreaseMinivanCostFor3Year).Column("decrease_minivan_cost_for_3_year");
			Map(x => x.DecreaseTruckCostFor3Year).Column("decrease_truck_cost_for_3_year");
			Map(x => x.GazelleAverageMileage).Column("gazelle_average_mileage");
			Map(x => x.LargusAverageMileage).Column("largus_average_mileage");
			Map(x => x.MinivanAverageMileage).Column("minivan_average_mileage");
			Map(x => x.TruckAverageMileage).Column("truck_average_mileage");
			Map(x => x.GazelleAmortisationPerKm).Column("gazelle_amortisation");
			Map(x => x.LargusAmortisationPerKm).Column("largus_amortisation");
			Map(x => x.MinivanAmortisationPerKm).Column("minivan_amortisation");
			Map(x => x.TruckAmortisationPerKm).Column("truck_amortisation");
			Map(x => x.OperatingExpensesAllGazelles).Column("operating_expenses_all_gazelles");
			Map(x => x.OperatingExpensesAllLarguses).Column("operating_expenses_all_larguses");
			Map(x => x.OperatingExpensesAllMinivans).Column("operating_expenses_all_minivans");
			Map(x => x.OperatingExpensesAllTrucks).Column("operating_expenses_all_trucks");
			Map(x => x.AverageMileageAllGazelles).Column("average_mileage_all_gazelles");
			Map(x => x.AverageMileageAllLarguses).Column("average_mileage_all_larguses");
			Map(x => x.AverageMileageAllMinivans).Column("average_mileage_all_minivans");
			Map(x => x.AverageMileageAllTrucks).Column("average_mileage_all_trucks");
			Map(x => x.GazelleRepairCostPerKm).Column("gazelle_repair_cost");
			Map(x => x.LargusRepairCostPerKm).Column("largus_repair_cost");
			Map(x => x.MinivanRepairCostPerKm).Column("minivan_repair_cost");
			Map(x => x.TruckRepairCostPerKm).Column("truck_repair_cost");
			Map(x => x.CalculationSaved).Column("calculation_saved");

			References(x => x.CalculationAuthor).Column("calculation_author_id");

			HasManyToMany(x => x.AdministrativeProductGroupsFilter)
				.Table("profitability_constants_administrative_expenses_product_groups")
				.ParentKeyColumn("profitability_constant_id")
				.ChildKeyColumn("product_group_id")
				.LazyLoad();
			HasManyToMany(x => x.AdministrativeWarehousesFilter)
				.Table("profitability_constants_administrative_expenses_warehouses")
				.ParentKeyColumn("profitability_constant_id")
				.ChildKeyColumn("warehouse_id")
				.LazyLoad();
			HasManyToMany(x => x.WarehouseExpensesProductGroupsFilter)
				.Table("profitability_constants_warehouse_expenses_product_groups")
				.ParentKeyColumn("profitability_constant_id")
				.ChildKeyColumn("product_group_id")
				.LazyLoad();
			HasManyToMany(x => x.WarehouseExpensesWarehousesFilter)
				.Table("profitability_constants_warehouse_expenses_warehouses")
				.ParentKeyColumn("profitability_constant_id")
				.ChildKeyColumn("warehouse_id")
				.LazyLoad();
			HasManyToMany(x => x.RepairCostCarEventTypesFilter)
				.Table("profitability_constants_car_event_types")
				.ParentKeyColumn("profitability_constant_id")
				.ChildKeyColumn("car_event_type_id")
				.LazyLoad();
		}
	}
}

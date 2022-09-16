using FluentNHibernate.Mapping;
using Vodovoz.Domain.Profitability;

namespace Vodovoz.HibernateMapping.Profitability
{
	public class RouteListProfitabilityMap : ClassMap<RouteListProfitability>
	{
		public RouteListProfitabilityMap()
		{
			Table("route_lists_profitabilities");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Amortisation).Column("amortisation");
			Map(x => x.RepairCosts).Column("repair_costs");
			Map(x => x.FuelCosts).Column("fuel_costs");
			Map(x => x.DriverAndForwarderWages).Column("driver_and_forwarder_wages");
			Map(x => x.PaidDelivery).Column("paid_delivery");
			Map(x => x.RouteListExpenses).Column("route_list_expenses");
			Map(x => x.TotalGoodsWeight).Column("total_goods_weight");
			Map(x => x.RouteListExpensesPerKg).Column("route_list_expenses_per_kg");
			Map(x => x.ProfitabilityConstantsCalculatedMonth)
				.Column("profitability_constants_calculated_month");
			Map(x => x.SalesSum).Column("sales_sum");
			Map(x => x.ExpensesSum).Column("expenses_sum");
			Map(x => x.GrossMarginSum).Column("gross_margin_sum");
			Map(x => x.GrossMarginPercents).Column("gross_margin_percents");
		}
	}
}

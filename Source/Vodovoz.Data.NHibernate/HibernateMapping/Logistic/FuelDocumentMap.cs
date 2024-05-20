using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class FuelDocumentMap : ClassMap<FuelDocument>
	{
		public FuelDocumentMap()
		{
			Table("fuel_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Date).Column("date");
			Map(x => x.PayedForFuel).Column("payed_for_fuel");
			Map(x => x.FuelLimitLitersAmount).Column("gived_fuel_limits_amount");
			Map(x => x.LiterCost).Column("liter_cost");
			Map(x => x.LastEditDate).Column("last_edit_date");
			Map(x => x.FuelCardNumber).Column("fuel_card");
			Map(x => x.FuelPaymentType).Column("fuel_payment_type");

			References(x => x.Car).Column("car_id");
			References(x => x.Fuel).Column("fuel_type_id");
			References(x => x.Driver).Column("driver_id");
			References(x => x.RouteList).Column("route_list_id");
			References(x => x.FuelOperation).Column("fuel_operation_id").Cascade.All();
			References(x => x.FuelCashExpense).Column("fuel_cash_expense_id").Cascade.All();
			References(x => x.FuelExpenseOperation).Column("fuel_expense_operation_id").Cascade.All();
			References(x => x.Author).Column("author_id");
			References(x => x.LastEditor).Column("last_editor_id");
			References(x => x.FuelLimit).Column("fuel_limit_id").Cascade.All();
		}
	}
}


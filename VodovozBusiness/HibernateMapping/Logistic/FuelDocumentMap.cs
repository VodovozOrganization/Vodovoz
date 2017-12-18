using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	public class FuelDocumentMap: ClassMap<FuelDocument>
	{
		public FuelDocumentMap ()
		{
			Table ("fuel_documents");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Date)		 .Column ("date");
			Map (x => x.PayedForFuel).Column ("payed_for_fuel");
			Map (x => x.FuelCoupons).Column ("fuel_coupons");
			Map (x => x.LiterCost)	 .Column ("liter_cost");
			Map (x => x.LastEditDate).Column("last_edit_date");

			References (x => x.Car)				.Column ("car_id");
			References (x => x.Fuel)			.Column ("fuel_type_id");
			References (x => x.Driver)			.Column ("driver_id");
			References (x => x.Operation)		.Column ("fuel_operation_id").Cascade.All();
			References (x => x.FuelCashExpense)	.Column ("fuel_cash_expense_id").Cascade.All();
			References (x => x.Author)			.Column ("author_id");
			References (x => x.LastEditor)		.Column ("last_editor_id");
		}
	}
}


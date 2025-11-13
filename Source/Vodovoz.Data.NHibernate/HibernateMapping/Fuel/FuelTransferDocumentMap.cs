using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Fuel
{
	public class FuelTransferDocumentMap : ClassMap<FuelTransferDocument>
	{
		public FuelTransferDocumentMap()
		{
			Table("fuel_transfer_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreationTime).Column("creation_time");
			Map(x => x.Status).Column("status");
			Map(x => x.SendTime).Column("send_time");
			Map(x => x.ReceiveTime).Column("receive_time");
			Map(x => x.TransferedLiters).Column("transfered_liters");
			Map(x => x.Comment).Column("comment");

			References(x => x.Author).Column("author_id");
			References(x => x.Driver).Column("driver_id");
			References(x => x.Car).Column("car_id");
			References(x => x.CashierSender).Column("cashier_sender_id");
			References(x => x.CashierReceiver).Column("cashier_receiver_id");
			References(x => x.CashSubdivisionFrom).Column("cash_subdivision_from_id");
			References(x => x.CashSubdivisionTo).Column("cash_subdivision_to_id");
			References(x => x.FuelType).Column("fuel_type_id");
			References(x => x.FuelTransferOperation).Column("fuel_transfer_operation_id").Cascade.All();
			References(x => x.FuelExpenseOperation).Column("fuel_expense_operation_id").Cascade.All();
			References(x => x.FuelIncomeOperation).Column("fuel_income_operation_id").Cascade.All();
		}
	}
}

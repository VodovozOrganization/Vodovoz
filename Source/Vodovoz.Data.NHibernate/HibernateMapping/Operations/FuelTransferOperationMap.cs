using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class FuelTransferOperationMap : ClassMap<FuelTransferOperation>
	{
		public FuelTransferOperationMap()
		{
			Table("fuel_transfer_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.SendTime).Column("send_time");
			Map(x => x.ReceiveTime).Column("receive_time");
			Map(x => x.TransferedLiters).Column("transfered_liters");

			References(x => x.FuelType).Column("fuel_type_id");
			References(x => x.SubdivisionFrom).Column("subdivision_from_id");
			References(x => x.SubdivisionTo).Column("subdivision_to_id");
		}
	}
}

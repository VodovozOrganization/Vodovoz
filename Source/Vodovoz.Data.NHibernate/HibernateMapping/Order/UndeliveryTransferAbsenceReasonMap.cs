using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class UndeliveryTransferAbsenceReasonMap : ClassMap<UndeliveryTransferAbsenceReason>
	{
		public UndeliveryTransferAbsenceReasonMap()
		{
			Table("undelivery_transfer_absence_reasons");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CreateDate).Column("create_date").ReadOnly();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}

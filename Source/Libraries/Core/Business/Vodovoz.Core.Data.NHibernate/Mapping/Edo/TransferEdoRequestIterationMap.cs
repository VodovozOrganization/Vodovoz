using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TransferEdoRequestIterationMap : ClassMap<TransferEdoRequestIteration>
	{
		public TransferEdoRequestIterationMap()
		{
			Table("edo_transfer_request_iterations");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Time)
				.Column("time")
				.ReadOnly();

			References(x => x.OrderEdoTask)
				.Column("order_edo_task_id")
				.Cascade.All();

			Map(x => x.Initiator)
				.Column("initiator");

			Map(x => x.Status)
				.Column("status");

			HasMany(x => x.TransferEdoRequests)
				.KeyColumn("iteration_id")
				.Cascade.AllDeleteOrphan();
		}
	}
}

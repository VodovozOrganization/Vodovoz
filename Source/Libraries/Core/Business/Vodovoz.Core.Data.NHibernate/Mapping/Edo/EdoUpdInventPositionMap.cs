using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoUpdInventPositionMap : ClassMap<EdoUpdInventPosition>
	{
		public EdoUpdInventPositionMap()
		{
			Table("edo_upd_invent_positions");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.AssignedOrderItem)
				.Column("assigned_order_item_id");

			HasMany(x => x.Codes)
				.KeyColumn("edo_upd_invent_position_id")
				.Cascade.AllDeleteOrphan();
		}
	}
}

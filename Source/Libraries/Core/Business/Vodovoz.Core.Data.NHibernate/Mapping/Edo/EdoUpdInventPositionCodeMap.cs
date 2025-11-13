using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoUpdInventPositionCodeMap : ClassMap<EdoUpdInventPositionCode>
	{
		public EdoUpdInventPositionCodeMap()
		{
			Table("edo_upd_invent_position_codes");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Quantity)
				.Column("quantity");

			References(x => x.IndividualCode)
				.Column("individual_code_id");

			References(x => x.GroupCode)
				.Column("group_code_id");
		}
	}
}

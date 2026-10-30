using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.Upd;

namespace Vodovoz.Core.Data.NHibernate.Edo
{
	public class UpdProductCodeMap : ClassMap<UpdProductCode>
	{
		public UpdProductCodeMap()
		{
			Table("edo_upd_document_invent_position_codes");

			Id(x => x.Id).GeneratedBy.Native();

			References(x => x.IndividualCode)
				.Column("individual_code_id");

			References(x => x.GroupCode)
				.Column("group_code_id");

			Map(x => x.Quantity)
				.Column("quantity");

			Map(x => x.TransportCode)
				.Column("transport_code");

			Map(x => x.IdentificationCode)
				.Column("identification_code");
		}
	}
}

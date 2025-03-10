using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TransferOrderTrueMarkCodeMap : ClassMap<TransferOrderTrueMarkCode>
	{
		public TransferOrderTrueMarkCodeMap()
		{
			Table("edo_transfer_order_codes");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.TransferOrder)
				.Column("transfer_order_id");

			References(x => x.Nomenclature)
				.Column("nomenclature_id");

			Map(x => x.Quantity)
				.Column("quantity");

			References(x => x.IndividualCode)
				.Column("individual_code_id");

			References(x => x.GroupCode)
				.Column("group_code_id");
		}
	}
}

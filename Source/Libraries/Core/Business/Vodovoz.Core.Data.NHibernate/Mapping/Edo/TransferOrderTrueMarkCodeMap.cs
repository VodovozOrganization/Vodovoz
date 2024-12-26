using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TransferOrderTrueMarkCodeMap : ClassMap<TransferOrderTrueMarkCode>
	{
		public TransferOrderTrueMarkCodeMap()
		{
			Table("transfer_order_codes");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.TransferOrder)
				.Column("transfer_order_id");

			References(x => x.TrueMarkCode)
				.Column("true_mark_code_id");
		}
	}
}

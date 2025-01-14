using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	public class CashlessRequestMap : SubclassMap<CashlessRequest>
	{
		public CashlessRequestMap()
		{
			DiscriminatorValue(PayoutRequestDocumentType.CashlessRequest.ToString());
			Map(clr => clr.Sum).Column("sum");
			References(clr => clr.Counterparty).Column("counterparty_id");
			HasMany(clr => clr.AttachedFileInformations)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.KeyColumn("cashless_request_id");
		}
	}
}

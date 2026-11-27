using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Clients
{
	public class CounterpartyTrueMarkProductGroupMap : ClassMap<CounterpartyTrueMarkProductGroup>
	{
		public CounterpartyTrueMarkProductGroupMap()
		{
			Table("counterparty_true_mark_product_groups");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Counterparty)
				.Column("counterparty_id")
				.Not.Nullable();

			Map(x => x.ProductGroup)
				.Column("product_group");
		}
	}
}

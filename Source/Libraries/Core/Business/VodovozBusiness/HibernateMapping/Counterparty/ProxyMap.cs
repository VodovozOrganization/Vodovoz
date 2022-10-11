using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping
{
	public class ProxyMap : ClassMap<Proxy>
	{
		public ProxyMap ()
		{
			Table ("counterparty_proxies");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Number).Column ("number");
			Map (x => x.IssueDate).Column ("issue_date");
			Map (x => x.StartDate).Column ("start_date");
			Map (x => x.ExpirationDate).Column ("expiration_date");
			References (x => x.Counterparty).Column ("counterparty_id");

			HasManyToMany(x => x.DeliveryPoints).Table("counterparty_proxy_delivery_points")
				.ParentKeyColumn("counterparty_proxy_id")
				.ChildKeyColumn("delivery_point_id")
				.LazyLoad();
			HasMany (x => x.Persons).Cascade.AllDeleteOrphan ().LazyLoad ()
				.KeyColumn ("counterparty_proxy_id");
		}
	}
}
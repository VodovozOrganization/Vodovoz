using FluentNHibernate.Mapping;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Retail
{
	public class SalesChannelMap : ClassMap<Domain.Retail.SalesChannel>
	{
		public SalesChannelMap()
		{
			Table("sales_channels");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}

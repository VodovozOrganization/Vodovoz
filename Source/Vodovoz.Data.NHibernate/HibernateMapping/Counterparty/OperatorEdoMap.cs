using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class EdoOpeartorMap : ClassMap<EdoOperator>
	{
		public EdoOpeartorMap()
		{
			Table("edo_operators");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.BrandName).Column("brand_name");
			Map(x => x.Code).Column("code");
		}
	}
}

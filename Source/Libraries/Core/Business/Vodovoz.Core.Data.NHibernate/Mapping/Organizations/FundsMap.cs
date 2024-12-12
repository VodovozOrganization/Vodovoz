using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Organizations
{
	public class FundsMap : ClassMap<Funds>
	{
		public FundsMap()
		{
			Table("funds");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.DefaultAccountFillType).Column("default_account_fill_type");
		}
	}
}

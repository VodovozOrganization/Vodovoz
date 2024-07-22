using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Organizations
{
	public class BusinessAccountMap : ClassMap<BusinessAccount>
	{
		public BusinessAccountMap()
		{
			Table("business_accounts");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.Bank).Column("bank");
			Map(x => x.Number).Column("number");
			Map(x => x.AccountFillType).Column("account_fill_type");

			References(x => x.BusinessActivity).Column("business_activity_id");
			References(x => x.Funds).Column("funds_id");
		}
	}
}

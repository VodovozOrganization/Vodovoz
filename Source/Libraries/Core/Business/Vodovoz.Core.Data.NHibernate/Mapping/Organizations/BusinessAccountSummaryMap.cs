using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Organizations
{
	public class BusinessAccountSummaryMap : ClassMap<BusinessAccountSummary>
	{
		public BusinessAccountSummaryMap()
		{
			Table("business_accounts_summary");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Total).Column("total");

			References(x => x.BusinessActivitySummary).Column("business_activity_summary_id");
			References(x => x.BusinessAccount).Column("business_account_id");
		}
	}
}

using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Organizations
{
	public class FundsSummaryMap : ClassMap<FundsSummary>
	{
		public FundsSummaryMap()
		{
			Table("funds_summary");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Total).Column("total");

			References(x => x.CompanyBalanceByDay).Column("company_balance_by_day_id");
			References(x => x.Funds).Column("funds_id");

			HasMany(x => x.BusinessActivitySummary)
				.Inverse().Cascade.AllDeleteOrphan();
		}
	}
}

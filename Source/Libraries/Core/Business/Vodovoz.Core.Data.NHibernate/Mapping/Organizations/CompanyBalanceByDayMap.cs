using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Organizations
{
	public class CompanyBalanceByDayMap : ClassMap<CompanyBalanceByDay>
	{
		public CompanyBalanceByDayMap()
		{
			Table("company_balances_by_day");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Date).Column("date");
			Map(x => x.Total).Column("total");

			HasMany(x => x.FundsSummary)
				.Inverse().Cascade.AllDeleteOrphan();
		}
	}
}

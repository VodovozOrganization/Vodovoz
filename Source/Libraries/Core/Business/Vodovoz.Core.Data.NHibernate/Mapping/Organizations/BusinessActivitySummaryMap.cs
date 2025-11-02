using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Organizations
{
	public class BusinessActivitySummaryMap : ClassMap<BusinessActivitySummary>
	{
		public BusinessActivitySummaryMap()
		{
			Table("business_activities_summary");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Total).Column("total");

			References(x => x.FundsSummary).Column("funds_summary_id");
			References(x => x.BusinessActivity).Column("business_activity_id");

			HasMany(x => x.BusinessAccountsSummary)
				.Inverse().Cascade.AllDeleteOrphan();
		}
	}
}

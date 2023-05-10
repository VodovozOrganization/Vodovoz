using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
	public class LogisticsRequirementsMap : ClassMap<LogisticsRequirements>
	{
		public LogisticsRequirementsMap()
		{
			Table("logistics_requirements");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ForwarderRequired).Column("forwarder_required");
			Map(x => x.DocumentsRequired).Column("documents_required");
			Map(x => x.RussianDriverRequired).Column("russian_driver_required");
			Map(x => x.PassRequired).Column("pass_required");
			Map(x => x.LargusRequired).Column("lagrus_required");
		}
	}
}

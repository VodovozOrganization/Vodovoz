using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class PremiumTemplateMap : ClassMap<PremiumTemplate>
	{
		public PremiumTemplateMap()
		{
			Table("premium_templates");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Reason).Column("reason");
			Map(x => x.PremiumMoney).Column("fine_money");
		}
	}
}

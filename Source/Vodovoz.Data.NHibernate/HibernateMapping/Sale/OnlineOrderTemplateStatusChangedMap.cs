using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class OnlineOrderTemplateStatusChangedMap : ClassMap<OnlineOrderTemplateStatusChanged>
	{
		public OnlineOrderTemplateStatusChangedMap()
		{
			Table("online_orders_templates_status_changed");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.TemplateId).Column("template_id").Not.Nullable();
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.EmployeeId).Column("employee_id");
			Map(x => x.CreatedOn).Column("created_on").Not.Nullable();
			Map(x => x.Type).Column("type").Not.Nullable();
		}
	}
}

using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HibernateMapping.Employees
{
	public class ProxyDocumentMap : ClassMap<ProxyDocument>
	{
		public ProxyDocumentMap()
		{
			Table("proxy_documents");
			Not.LazyLoad();
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("type");
			Map(x => x.Date).Column("date");
			Map(x => x.ExpirationDate).Column("expiration_date");
			References(x => x.DocumentTemplate).Column("doc_template_id");
			Map(x => x.ChangedTemplateFile).Column("doc_changed_template").LazyLoad();
			References(x => x.Organization).Column("organization_id");
		}
	}

	public class CarProxyDocumentMap : SubclassMap<CarProxyDocument>
	{
		public CarProxyDocumentMap()
		{
			DiscriminatorValue("CarProxy");

			References(x => x.Driver).Column("employee_id");
			References(x => x.EmployeeDocument).Column("document_id");
			References(x => x.Car).Column("car_id");
		}
	}

	public class M2ProxyDocumentMap : SubclassMap<M2ProxyDocument>
	{
		public M2ProxyDocumentMap()
		{
			DiscriminatorValue("M2Proxy");

			Map(x => x.TicketDate).Column("ticket_date");
			Map(x => x.TicketNumber).Column("ticket_number");
			References(x => x.Order).Column("order_id");
			References(x => x.Employee).Column("employee_id");
			References(x => x.Supplier).Column("supplier_id");
		}
	}
}

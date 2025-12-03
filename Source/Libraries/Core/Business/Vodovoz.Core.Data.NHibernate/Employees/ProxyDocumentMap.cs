using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Data.NHibernate.Employees
{
	public class ProxyDocumentMap : ClassMap<ProxyDocumentEntity>
	{
		public ProxyDocumentMap()
		{
			Table("proxy_documents");

			Not.LazyLoad();

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Date)
				.Column("date");

			Map(x => x.ExpirationDate)
				.Column("expiration_date");

			Map(x => x.ChangedTemplateFile)
				.Column("doc_changed_template")
				.LazyLoad();

			References(x => x.DocumentTemplate)
				.Column("doc_template_id");

			References(x => x.Organization)
				.Column("organization_id");
		}
	}

	public class CarProxyDocumentMap : SubclassMap<CarProxyDocumentEntity>
	{
		public CarProxyDocumentMap()
		{
			DiscriminatorValue("CarProxy");

			References(x => x.Driver)
				.Column("employee_id");

			References(x => x.EmployeeDocument)
				.Column("document_id");

			References(x => x.Car)
				.Column("car_id");
		}
	}

	public class M2ProxyDocumentMap : SubclassMap<M2ProxyDocumentEntity>
	{
		public M2ProxyDocumentMap()
		{
			DiscriminatorValue("M2Proxy");

			Map(x => x.TicketDate)
				.Column("ticket_date");

			Map(x => x.TicketNumber)
				.Column("ticket_number");

			References(x => x.Order)
				.Column("order_id");

			References(x => x.Employee)
				.Column("employee_id");

			References(x => x.Supplier)
				.Column("supplier_id");
		}
	}
}

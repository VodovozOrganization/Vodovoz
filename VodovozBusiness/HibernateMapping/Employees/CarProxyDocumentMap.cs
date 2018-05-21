using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HibernateMapping.Employees
{
	public class CarProxyDocumentMap : ClassMap<CarProxyDocument>
	{
		public CarProxyDocumentMap()
		{
			Table("car_proxy_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Date).Column("date");
			References(x => x.Organization).Column("organization_id");
			References(x => x.Driver).Column("employee_id");
			References(x => x.Car).Column("car_id");
			References(x => x.CarProxyDocumentTemplate).Column("doc_template_id");
			Map(x => x.ChangedTemplateFile).Column("doc_changed_template").LazyLoad();
		}
	}
}

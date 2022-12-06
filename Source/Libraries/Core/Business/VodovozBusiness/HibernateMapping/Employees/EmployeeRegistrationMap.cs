using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HibernateMapping.Employees
{
	public class EmployeeRegistrationMap : ClassMap<EmployeeRegistration>
	{
		public EmployeeRegistrationMap()
		{
			Table("employees_registrations");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.RegistrationType).Column("registration_type");
			Map(x => x.PaymentForm).Column("payment_form");
			Map(x => x.TaxRate).Column("tax_rate");
		}
	}
}

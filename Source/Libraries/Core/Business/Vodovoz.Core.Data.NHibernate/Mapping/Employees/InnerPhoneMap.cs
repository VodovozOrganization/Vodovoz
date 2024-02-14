using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Employees
{
	public class InnerPhoneMap : ClassMap<InnerPhone>
	{
		public InnerPhoneMap()
		{
			Table("inner_phones");

			Id(x => x.PhoneNumber).Column("phone_number")
				.GeneratedBy.Assigned();
			Map(x => x.Description).Column("description");
		}
	}
}

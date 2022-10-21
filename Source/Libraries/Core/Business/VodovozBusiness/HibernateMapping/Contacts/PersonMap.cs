using FluentNHibernate.Mapping;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.HibernateMapping.Contacts
{
	public class PersonMap : ClassMap<Person>
	{
		public PersonMap ()
		{
			Table("persons");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.PatronymicName).Column("patronymic");
			Map(x => x.Lastname).Column("surname");
		}
	}
}


using FluentNHibernate.Mapping;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.HibernateMapping.Contacts
{
	public class PhoneMap : ClassMap<Phone>
	{
		public PhoneMap ()
		{
			Table("phones");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Number).Column("number");
			Map(x => x.DigitsNumber).Column("digits_number");
			Map(x => x.Additional).Column("additional");
			Map(x => x.Name).Column("name");

			References(x => x.NumberType).Column("type_id").Not.LazyLoad();
		}
	}
}


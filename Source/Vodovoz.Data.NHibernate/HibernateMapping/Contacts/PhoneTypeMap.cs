using FluentNHibernate.Mapping;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.HibernateMapping.Contacts
{
	public class PhoneTypeMap : ClassMap<PhoneType>
	{
		public PhoneTypeMap()
		{
			Table("phone_types");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.PhonePurpose).Column("purpose").CustomType<PhonePurposeStringType>();
		}
	}
}


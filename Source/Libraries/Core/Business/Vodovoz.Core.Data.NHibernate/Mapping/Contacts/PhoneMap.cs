using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Contacts;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Contacts
{
	public class PhoneMap : ClassMap<PhoneEntity>
	{
		public PhoneMap()
		{
			Table("phones");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Number)
				.Column("number");

			Map(x => x.DigitsNumber)
				.Column("digits_number");

			Map(x => x.Additional)
				.Column("additional");

			Map(x => x.Comment)
				.Column("comment");

			Map(x => x.IsArchive)
				.Column("is_archive");

			References(x => x.PhoneType)
				.Column("type_id")
				.Not.LazyLoad();
		}
	}
}

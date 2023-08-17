using FluentNHibernate.Mapping;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Contacts
{
	public class PostMap : ClassMap<Post>
	{
		public PostMap()
		{
			Table("post");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}
	}
}


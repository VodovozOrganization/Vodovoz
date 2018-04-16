using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping
{
	public class TagMap : ClassMap<Tag>
	{
		public TagMap()
		{
			Table("tags");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.ColorText).Column("color_text");
		}
	}
}

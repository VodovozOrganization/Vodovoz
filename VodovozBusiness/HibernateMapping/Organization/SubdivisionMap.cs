using System;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class SubdivisionMap : ClassMap<Subdivision>
	{
		public SubdivisionMap()
		{
			Table("subdivisions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			References(x => x.Chief).Column("chief_id");
		}
	}
}


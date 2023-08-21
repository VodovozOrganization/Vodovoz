﻿using FluentNHibernate.Mapping;
using FluentNHibernate.MappingModel;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.HibernateMapping.Complaints
{
	public class ComplaintResultBaseMap : ClassMap<ComplaintResultBase>
	{
		public ComplaintResultBaseMap()
		{
			Table("complaint_results");
			Not.LazyLoad();
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("type");
			
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}

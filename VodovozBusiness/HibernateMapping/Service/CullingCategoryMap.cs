using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HibernateMapping.Service
{
    public class CullingCategoryMap : ClassMap<CullingCategory>
    {
        public CullingCategoryMap()
        {
            Table("culling_category");
            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.Name).Column("name");
        }
    }
}

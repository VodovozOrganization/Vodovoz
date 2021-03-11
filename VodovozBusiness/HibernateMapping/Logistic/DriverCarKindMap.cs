using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
    public class DriverCarKindMap : ClassMap<DriverCarKind>
    {
        public DriverCarKindMap()
        {
            Table("driver_car_kinds");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.Name).Column("name");
            Map(x => x.IsArchive).Column("is_archive");
        }
    }
}

using System;
using FluentNHibernate.Mapping;

namespace Vodovoz.HibernateMapping.Counterparty
{
    public class DeliveryPointResponsiblePersonMap : ClassMap<Domain.Client.DeliveryPointResponsiblePerson>
    {
        public DeliveryPointResponsiblePersonMap()
        {
            Table("delivery_points_responsible_persons");

            Id(x => x.Id).Column("id").GeneratedBy.Native();
            Map(x => x.Phone).Column("phone");
            References(x => x.DeliveryPoint).Column("delivery_point_id");
            References(x => x.DeliveryPointResponsiblePersonType).Column("delivery_points_responsible_persons_type_id");
            References(x => x.Employee).Column("employee_id");
        }
    }
}

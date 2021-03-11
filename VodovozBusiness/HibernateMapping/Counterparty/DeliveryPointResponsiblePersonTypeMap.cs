using FluentNHibernate.Mapping;

namespace Vodovoz.HibernateMapping.Counterparty
{
    public class DeliveryPointResponsiblePersonTypeMap : ClassMap<Domain.Client.DeliveryPointResponsiblePersonType>
    {
        public DeliveryPointResponsiblePersonTypeMap()
        {
            Table("delivery_points_responsible_person_types");

            Id(x => x.Id).Column("id").GeneratedBy.Native();
            Map(x => x.Title).Column("title");
        }
    }
}

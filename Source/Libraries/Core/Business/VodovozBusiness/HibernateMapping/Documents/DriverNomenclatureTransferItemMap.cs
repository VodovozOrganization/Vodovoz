using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
    public class DriverNomenclatureTransferItemMap : ClassMap<DriverNomenclatureTransferItem>
    {
        public DriverNomenclatureTransferItemMap()
        {
            Table("driver_nomenclature_transfer_items");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.Amount).Column("amount");

            References(x => x.DocumentItem).Column("address_transfer_document_item_id");
            References(x => x.Nomenclature).Column("nomenclature_id");
            References(x => x.DriverFrom).Column("driver_from_id");
            References(x => x.DriverTo).Column("driver_to_id");
            References(x => x.EmployeeNomenclatureMovementOperationFrom)
                .Cascade.All().Column("employee_nomenclature_movement_operation_from_id");
            References(x => x.EmployeeNomenclatureMovementOperationTo)
                .Cascade.All().Column("employee_nomenclature_movement_operation_to_id");
        }
    }
}
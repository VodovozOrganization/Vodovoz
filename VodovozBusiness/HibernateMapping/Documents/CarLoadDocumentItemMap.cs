﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping
{
    public class CarLoadDocumentItemMap : ClassMap<CarLoadDocumentItem>
    {
        public CarLoadDocumentItemMap()
        {
            Table("store_car_load_document_items");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.Amount).Column("amount");
            Map(x => x.ExpireDatePercent).Column("item_expiration_date_percent");

            References(x => x.Nomenclature).Column("nomenclature_id");
            References(x => x.Equipment).Column("equipment_id");
            References(x => x.Document).Column("car_load_document_id");
            References(x => x.WarehouseMovementOperation).Column("warehouse_movement_operation_id").Cascade.All();
            References(x => x.EmployeeNomenclatureMovementOperation).Column("employee_nomenclature_movement_operation_id").Cascade.All();
        }
    }
}
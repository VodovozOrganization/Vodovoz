﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping
{
	//TODO поправить класс
	public class WriteOffDocumentItemMap : ClassMap<WriteOffDocumentItem>
	{
		public WriteOffDocumentItemMap ()
		{
			Table("store_writeoff_document_items");
			DiscriminateSubClassesOnColumn("accounting_type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Amount).Column("amount");
			Map(x => x.Comment).Column("comment");
			//Map(x => x.SumOfDamage).Column("sum_of_damage");
			
			References(x => x.Fine).Column("fine_id");
			References(x => x.Document).Column("write_off_document_id").Not.Nullable();
			//References(x => x.Equipment).Column("equipment_id");
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
			References(x => x.CullingCategory).Column("culling_category_id");
			References(x => x.WarehouseWriteOffOperation)
				.Column("write_off_goods_accounting_operation_id")
				.Cascade.All();
			//References(x => x.CounterpartyWriteoffOperation).Column("writeoff_counterparty_movement_operation_id").Cascade.All();
		}
	}
}

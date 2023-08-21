﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping
{
	public class GoodsAccountingOperationMap : ClassMap<GoodsAccountingOperation>
	{
		public GoodsAccountingOperationMap()
		{
			Table("goods_accounting_operations");
			DiscriminateSubClassesOnColumn("operation_type");

			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.OperationTime).Column("operation_time").Not.Nullable();
			Map(x => x.Amount).Column("amount");

			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
		}
	}
}


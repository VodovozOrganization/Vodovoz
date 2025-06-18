﻿using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class RegradingOfGoodsDocumentMap : ClassMap<RegradingOfGoodsDocument>
	{
		public RegradingOfGoodsDocumentMap()
		{
			Table("store_regrading_of_goods");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Map(x => x.Comment).Column("comment");
			Map(x => x.TimeStamp).Column("time_stamp");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			Map(x => x.AuthorId).Column("author_id");
			Map(x => x.LastEditorId).Column("last_editor_id");

			References(x => x.Warehouse).Column("warehouse_id");
			HasMany(x => x.Items).Cascade.AllDeleteOrphan().Inverse().KeyColumn("store_regrading_of_goods_id");
		}
	}
}

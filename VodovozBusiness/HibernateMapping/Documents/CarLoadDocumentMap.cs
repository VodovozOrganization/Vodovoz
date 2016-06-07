using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class CarLoadDocumentMap : ClassMap<CarLoadDocument>
	{
		public CarLoadDocumentMap ()
		{
			Table ("store_car_load_documents");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.TimeStamp).Column ("time_stamp");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			Map (x => x.Comment).Column ("comment");
			References (x => x.Author).Column ("author_id");
			References (x => x.LastEditor).Column ("last_editor_id");
			References (x => x.RouteList).Column ("route_list_id");
			References (x => x.Warehouse).Column ("warehouse_id");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("car_load_document_id");
		}
	}
}


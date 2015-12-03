using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class CarLoadDocumentMap : ClassMap<CarLoadDocument>
	{
		public CarLoadDocumentMap ()
		{
			Table ("car_load_documents");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.TimeStamp).Column ("time_stamp");
			References (x => x.Storekeeper).Column ("storekeeper_id");
			References (x => x.Order).Column ("order_id");
			References (x => x.RouteList).Column ("route_list_id");
			References (x => x.Warehouse).Column ("warehouse_id");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("car_load_document_id");
		}
	}
}


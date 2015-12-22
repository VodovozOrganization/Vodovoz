using System;
using Vodovoz.Domain.Documents;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class CarUnloadDocumentMap:ClassMap<CarUnloadDocument>
	{
		public CarUnloadDocumentMap ()
		{
			Table ("car_unload_documents");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.TimeStamp).Column ("time_stamp");
			References (x => x.Storekeeper).Column ("storekeeper_id");
			References (x => x.RouteList).Column ("route_list_id");
			References (x => x.Warehouse).Column ("warehouse_id");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("car_unload_document_id");

		}
	}
}


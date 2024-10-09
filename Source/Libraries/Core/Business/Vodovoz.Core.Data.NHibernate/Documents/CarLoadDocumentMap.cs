using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Documents
{
	public class CarLoadDocumentMap : ClassMap<CarLoadDocumentEntity>
	{
		public CarLoadDocumentMap()
		{
			Table("store_car_load_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.LoadOperationState).Column("load_operation_state");
		}
	}
}

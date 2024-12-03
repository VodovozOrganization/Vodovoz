using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Documents
{
	public class CarLoadDocumentLoadingProcessActionsMap : ClassMap<CarLoadDocumentLoadingProcessAction>
	{
		public CarLoadDocumentLoadingProcessActionsMap()
		{
			Table("store_car_load_document_loading_process_actions");
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CarLoadDocumentId).Column("store_car_load_document_id");
			Map(x => x.PickerEmployeeId).Column("employee_id");
			Map(x => x.ActionTime).Column("action_time");
			Map(x => x.ActionType).Column("action_type");
		}
	}
}

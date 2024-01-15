using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace Vodovoz.Core.Data.NHibernate.Logistics
{
	public interface IDriverWarehouseEventRepository
	{
		IEnumerable<DriverWarehouseEvent> GetActiveDriverWarehouseEventsForDocument(IUnitOfWork uow, EventQrDocumentType documentType);
		bool HasOtherActiveDriverWarehouseEventsForDocumentAndQrPosition(
			IUnitOfWork uow, int excludeEventId, EventQrDocumentType documentType, EventQrPositionOnDocument qrPositionOnDocument);
	}
}

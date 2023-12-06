using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Tools;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface IDriverWarehouseEventRepository
	{
		IEnumerable<DriverWarehouseEvent> GetActiveDriverWarehouseEventsForDocument(IUnitOfWork uow, EventQrDocumentType documentType);
		bool HasActiveDriverWarehouseEventsForDocumentAndQrPosition(
			IUnitOfWork uow, EventQrDocumentType documentType, EventQrPositionOnDocument qrPositionOnDocument);
	}
}

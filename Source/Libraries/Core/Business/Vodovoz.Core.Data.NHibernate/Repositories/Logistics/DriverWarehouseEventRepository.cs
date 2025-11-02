using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Interfaces.Logistics;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Logistics
{
	public class DriverWarehouseEventRepository : IDriverWarehouseEventRepository
	{
		public IEnumerable<DriverWarehouseEvent> GetActiveDriverWarehouseEventsForDocument(
			IUnitOfWork uow, EventQrDocumentType documentType)
		{
			return uow.Session.QueryOver<DriverWarehouseEvent>()
				.Where(e => e.DocumentType == documentType)
				.And(e => !e.IsArchive)
				.List();
		}

		public bool HasOtherActiveDriverWarehouseEventsForDocumentAndQrPosition(
			IUnitOfWork uow, int excludeEventId, EventQrDocumentType documentType, EventQrPositionOnDocument qrPositionOnDocument)
		{
			return uow.Session.QueryOver<DriverWarehouseEvent>()
				.Where(e => e.DocumentType == documentType)
				.And(e => e.Id != excludeEventId)
				.And(e => e.QrPositionOnDocument == qrPositionOnDocument)
				.And(e => !e.IsArchive)
				.RowCount() > 0;
		}
	}
}

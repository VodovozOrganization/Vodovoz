using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Tools;

namespace Vodovoz.EntityRepositories.Logistic
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

		public bool HasActiveDriverWarehouseEventsForDocumentAndQrPosition(
			IUnitOfWork uow, EventQrDocumentType documentType, EventQrPositionOnDocument qrPositionOnDocument)
		{
			return uow.Session.QueryOver<DriverWarehouseEvent>()
				.Where(e => e.DocumentType == documentType)
				.And(e => e.QrPositionOnDocument == qrPositionOnDocument)
				.And(e => !e.IsArchive)
				.RowCount() > 0;
		}
	}
}

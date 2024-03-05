using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.Additions
{
	public interface IEventsQrPlacer
	{
		bool AddQrEventForDocument(
			IUnitOfWork uow,
			int documentId,
			EventQrDocumentType eventQrDocumentType,
			ref string rdlPath,
			EventNamePosition eventNamePosition = EventNamePosition.Bottom);

		bool AddQrEventForDocument(
			IUnitOfWork uow,
			int documentId,
			ref string rdlReport,
			EventQrDocumentType eventQrDocumentType = EventQrDocumentType.RouteList,
			EventNamePosition eventNamePosition = EventNamePosition.Right);
	}
}

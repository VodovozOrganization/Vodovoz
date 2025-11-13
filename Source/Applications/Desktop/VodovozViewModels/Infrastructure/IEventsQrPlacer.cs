using QS.DomainModel.UoW;
using QS.Report;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.ViewModels.Infrastructure
{
	public interface IEventsQrPlacer
	{
		ReportInfo AddQrEventForPrintingDocument(
			IUnitOfWork uow,
			int documentId,
			string documentTitle,
			EventQrDocumentType eventQrDocumentType,
			EventNamePosition eventNamePosition = EventNamePosition.Bottom);
		
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

		string AddQrEventForWaterCarLoadDocument(
			IUnitOfWork uow,
			int documentId,
			string reportSource);
	}
}

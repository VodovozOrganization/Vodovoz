using System;
using Vodovoz.Core.Domain.Warehouses.Documents;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Documents.InventoryDocuments;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.WriteOffDocuments;
using VodovozBusiness.Domain.Documents;

namespace VodovozBusiness.Extensions
{
	public static class DocumentExtensions
	{
		public static Type ToDocType(this DocumentType docType)
		{
			switch(docType)
			{
				case DocumentType.IncomingInvoice:
					return typeof(IncomingInvoice);
				case DocumentType.IncomingWater:
					return typeof(IncomingWater);
				case DocumentType.MovementDocument:
					return typeof(MovementDocument);
				case DocumentType.WriteoffDocument:
					return typeof(WriteOffDocument);
				case DocumentType.SelfDeliveryDocument:
					return typeof(SelfDeliveryDocument);
				case DocumentType.CarLoadDocument:
					return typeof(CarLoadDocument);
				case DocumentType.CarUnloadDocument:
					return typeof(CarUnloadDocument);
				case DocumentType.InventoryDocument:
					return typeof(InventoryDocument);
				case DocumentType.ShiftChangeDocument:
					return typeof(ShiftChangeWarehouseDocument);
				case DocumentType.RegradingOfGoodsDocument:
					return typeof(RegradingOfGoodsDocument);
				case DocumentType.DeliveryDocument:
					return typeof(DeliveryDocument);
				case DocumentType.DriverTerminalMovement:
					return typeof(DriverAttachedTerminalDocumentBase);
				case DocumentType.DriverTerminalGiveout:
					return typeof(DriverAttachedTerminalGiveoutDocument);
				case DocumentType.DriverTerminalReturn:
					return typeof(DriverAttachedTerminalReturnDocument);
			}

			throw new NotSupportedException();
		}
	}
}

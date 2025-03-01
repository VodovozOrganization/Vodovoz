using System;
using EdoFiscalDocumentStatus = Vodovoz.Core.Domain.Edo.FiscalDocumentStatus;
using FiscalDocumentStatus = ModulKassa.DTO.FiscalDocumentStatus;

namespace Edo.Receipt
{
	public static class ReceiptConverters
	{
		public static EdoFiscalDocumentStatus ConvertFiscalDocumentStatus(FiscalDocumentStatus status)
		{
			switch(status)
			{
				case FiscalDocumentStatus.Queued:
					return EdoFiscalDocumentStatus.Queued;
				case FiscalDocumentStatus.Pending:
					return EdoFiscalDocumentStatus.Pending;
				case FiscalDocumentStatus.Printed:
					return EdoFiscalDocumentStatus.Printed;
				case FiscalDocumentStatus.WaitForCallback:
					return EdoFiscalDocumentStatus.WaitForCallback;
				case FiscalDocumentStatus.Completed:
					return EdoFiscalDocumentStatus.Completed;
				case FiscalDocumentStatus.Failed:
					return EdoFiscalDocumentStatus.Failed;
				default:
					throw new InvalidOperationException($"Неизвестный статус фискального документа {status}");
			}
		}
	}
}

using System;
using ModulKassa.DTO;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Receipt
{
	/// <summary>Обновляет фискальные реквизиты чека по ответу МодульКассы.</summary>
	public static class ReceiptFiscalDocumentUpdater
	{
		/// <summary>Переносит состояние и фискальные реквизиты из ответа кассы в чек.</summary>
		/// <param name="document">Чек</param>
		/// <param name="info">Ответ МодульКассы</param>
		public static void UpdateFiscalDocument(EdoFiscalDocument document, FiscalDocumentInfo info)
		{
			document.FiscalNumber = info.FiscalInfo.FnDocNumber.ToString();
			document.FiscalMark = info.FiscalInfo.FnDocMark.ToString();
			document.FiscalKktNumber = info.FiscalInfo.EcrRegistrationNumber;
			document.Status = ReceiptConverters.ConvertFiscalDocumentStatus(info.Status);
			document.StatusChangeTime = DateTime.Parse(info.TimeStatusChangedString);
			document.FiscalTime = DateTime.Parse(info.FiscalInfo.Date);
			document.Stage = FiscalDocumentStage.Completed;
		}
	}
}

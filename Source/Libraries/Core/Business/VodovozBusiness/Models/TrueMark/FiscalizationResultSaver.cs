using System;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Models.CashReceipts.DTO;
using ApiFiscalStatus = Vodovoz.Models.CashReceipts.DTO.FiscalDocumentStatus;
using DomainFiscalStatus = Vodovoz.Core.Domain.Edo.FiscalDocumentStatus;

namespace Vodovoz.Models.TrueMark
{
	public class FiscalizationResultSaver
	{
		public void SaveResult(CashReceipt cashReceipt, FiscalizationResult fiscalizationResult)
		{
			cashReceipt.FiscalDocumentStatus = ConvertStatus(fiscalizationResult.Status);
			cashReceipt.FiscalDocumentNumber = fiscalizationResult.FiscalDocumentNumber;
			cashReceipt.FiscalDocumentDate = fiscalizationResult.FiscalDocumentDate;
			cashReceipt.FiscalDocumentStatusChangeTime = fiscalizationResult.StatusChangedTime;
			if(fiscalizationResult.Status == ApiFiscalStatus.Failed)
			{
				if(string.IsNullOrWhiteSpace(cashReceipt.ErrorDescription))
				{
					cashReceipt.ErrorDescription = fiscalizationResult.FailDescription;
				}
				else
				{
					cashReceipt.ErrorDescription += $"\n{fiscalizationResult.FailDescription}";
				}
			}
		}

		private DomainFiscalStatus ConvertStatus(ApiFiscalStatus? status)
		{
			if(status == null)
			{
				return DomainFiscalStatus.None;
			}

			switch(status)
			{
				case ApiFiscalStatus.Queued:
					return DomainFiscalStatus.Queued;
				case ApiFiscalStatus.Pending:
					return DomainFiscalStatus.Pending;
				case ApiFiscalStatus.Printed:
					return DomainFiscalStatus.Printed;
				case ApiFiscalStatus.WaitForCallback:
					return DomainFiscalStatus.WaitForCallback;
				case ApiFiscalStatus.Completed:
					return DomainFiscalStatus.Completed;
				case ApiFiscalStatus.Failed:
					return DomainFiscalStatus.Failed;
				default:
					throw new InvalidOperationException($"Невозможно сконвертировать статус фискального документа. Неизвестный статус {status}");
			}
		}
	}
}

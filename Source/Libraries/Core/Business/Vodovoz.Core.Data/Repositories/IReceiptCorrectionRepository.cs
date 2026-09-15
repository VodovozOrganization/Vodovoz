using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IReceiptCorrectionRepository
	{
		EdoFiscalDocument GetLatestCompletedSaleDocumentForOrder(IUnitOfWork uow, int orderId);

		ReceiptCorrectionProcess GetLatestCompletedProcessForOrder(IUnitOfWork uow, int orderId);

		bool ProcessExistsByFingerprint(IUnitOfWork uow, int orderId, string changeFingerprint);

		IList<int> GetActiveProcessIds(IUnitOfWork uow);

		ReceiptCorrectionProcessDocument GetDocumentByGuid(IUnitOfWork uow, Guid documentGuid);

		IEnumerable<ReceiptCorrectionProcess> GetProcesses(
			IUnitOfWork uow,
			int? orderId,
			ReceiptCorrectionProcessStatus? status,
			ReceiptCorrectionScenarioType? scenarioType);

		IEnumerable<ReceiptCorrectionExplanatoryNote> GetExplanatoryNotes(
			IUnitOfWork uow,
			int? orderId,
			int? organizationId,
			ReceiptCorrectionProcessStatus? status = null,
			ReceiptCorrectionScenarioType? scenarioType = null);
	}
}

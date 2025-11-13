using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Models.CashReceipts
{
	public class FiscalDocumentRequeueService
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly CashboxClientProvider _cashboxClientProvider;
		private readonly FiscalizationResultSaver _fiscalizationResultSaver;

		public FiscalDocumentRequeueService(
			IUnitOfWorkFactory uowFactory,
			CashboxClientProvider cashboxClientProvider,
			FiscalizationResultSaver fiscalizationResultSaver)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
			_fiscalizationResultSaver = fiscalizationResultSaver ?? throw new ArgumentNullException(nameof(fiscalizationResultSaver));
		}

		public async Task RequeueDocForReceiptManually(int receiptId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Повторное проведение чека"))
			{
				var cashReceipt = uow.GetById<CashReceipt>(receiptId);
				if(cashReceipt == null)
				{
					var errorMessage = $"Чек c идентификатором {receiptId} не найден";
					throw new InvalidOperationException(errorMessage);
				}

				var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(cashReceipt, cancellationToken);
				var fiscalResult = await cashboxClient.RequeueFiscalDocument(cashReceipt.DocumentId, cancellationToken);

				if(fiscalResult?.SendStatus == DTO.SendStatus.Error)
				{
					throw new InvalidOperationException(fiscalResult?.FailDescription ?? "Непредвиденная ошибка");
				}

				if(fiscalResult?.Status != DTO.FiscalDocumentStatus.Queued)
				{
					throw new InvalidOperationException("В результате выполнения операции статус чека не изменился на \"В очереди\"");
				}

				_fiscalizationResultSaver.SaveResult(cashReceipt, fiscalResult);
				cashReceipt.FiscalDocumentStatus = FiscalDocumentStatus.Queued;

				uow.Save(cashReceipt);
				uow.Commit();
			}
		}
	}
}

using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Models.CashReceipts.DTO;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Models.CashReceipts
{
	public class CashReceiptsSender
	{
		private const int _maxReceiptsToSend = 30;

		private readonly ILogger<CashReceiptsSender> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FiscalDocumentPreparer _fiscalDocumentPreparer;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private readonly CashReceiptDistributor _cashReceiptDistributor;
		private readonly FiscalizationResultSaver _fiscalizationResultSaver;

		public CashReceiptsSender(
			ILogger<CashReceiptsSender> logger,
			IUnitOfWorkFactory uowFactory,
			FiscalDocumentPreparer fiscalDocumentPreparer,
			ICashReceiptRepository cashReceiptRepository,
			CashReceiptDistributor cashReceiptDistributor,
			FiscalizationResultSaver fiscalizationResultSaver
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_fiscalDocumentPreparer = fiscalDocumentPreparer ?? throw new ArgumentNullException(nameof(fiscalDocumentPreparer));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			_cashReceiptDistributor = cashReceiptDistributor ?? throw new ArgumentNullException(nameof(cashReceiptDistributor));
			_fiscalizationResultSaver = fiscalizationResultSaver ?? throw new ArgumentNullException(nameof(fiscalizationResultSaver));
		}

		public async Task PrepareAndSendAsync(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				_logger.LogInformation($"Получение чеков для отправки (макс. 30)");
				var receiptsData = PrepareReceiptsData(uow);

				_logger.LogInformation($"Общее количество чеков для отправки: {receiptsData.Count()}");
				var sendResuts = await _cashReceiptDistributor.SendReceipts(receiptsData, cancellationToken);

				_logger.LogInformation($"Сохранение результатов отправки");
				SaveResults(uow, sendResuts);

				uow.Commit();
			}
		}

		private IEnumerable<ReceiptSendData> PrepareReceiptsData(IUnitOfWork uow)
		{
			var receiptsToSend = new List<ReceiptSendData>();

			var receipts = _cashReceiptRepository.GetCashReceiptsForSend(uow, _maxReceiptsToSend);
			foreach(var receipt in receipts)
			{
				ReceiptSendData receiptToSend;
				try
				{
					receiptToSend = CreateReceiptNode(receipt);
				}
				catch(Exception ex)
				{
					RegisterException(uow, receipt, ex);
					continue;
				}

				receiptsToSend.Add(receiptToSend);
			}

			return receiptsToSend;
		}

		private void RegisterException(IUnitOfWork uow, CashReceipt receipt, Exception ex)
		{
			receipt.Status = CashReceiptStatus.ReceiptSendError;
			receipt.ErrorDescription= ex.Message;
			uow.Save(receipt);
		}

		private ReceiptSendData CreateReceiptNode(CashReceipt receipt)
		{
			var fiscalDocument = _fiscalDocumentPreparer.CreateDocument(receipt);

			var receiptData = new ReceiptSendData
			{
				CashReceipt = receipt,
				FiscalDocument = fiscalDocument
			};

			return receiptData;
		}

		private void SaveResults(IUnitOfWork uow, IEnumerable<ReceiptSendResult> sendResults)
		{
			foreach(var sendResult in sendResults)
			{
				var receipt = sendResult.CashReceipt;
				var fiscalizationResult = sendResult.FiscalizationResult;

				if(fiscalizationResult.SendStatus == SendStatus.Error)
				{
					receipt.Status = CashReceiptStatus.ReceiptSendError;
					receipt.ErrorDescription = fiscalizationResult.FailDescription;
				}
				else
				{
					receipt.Status = CashReceiptStatus.Sended;
					_fiscalizationResultSaver.SaveResult(receipt, fiscalizationResult);
				}

				uow.Save(receipt);
			}
		}
	}
}

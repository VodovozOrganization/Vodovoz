using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Models.CashReceipts;

namespace Vodovoz.Models.TrueMark
{
	public class FiscalDocumentRefresher
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly CashboxClientProvider _cashboxClientProvider;
		private readonly FiscalizationResultSaver _fiscalizationResultSaver;

		public FiscalDocumentRefresher(
			IUnitOfWorkFactory uowFactory,
			CashboxClientProvider cashboxClientProvider,
			FiscalizationResultSaver fiscalizationResultSaver)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
			_fiscalizationResultSaver = fiscalizationResultSaver ?? throw new ArgumentNullException(nameof(fiscalizationResultSaver));
		}

		public async Task RefreshDocForReceipt(int receiptId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var cashReceipt = uow.GetById<CashReceipt>(receiptId);
				if(cashReceipt == null)
				{
					return;
				}

				var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(cashReceipt, cancellationToken);
				var fiscalResult = await cashboxClient.CheckFiscalDocument(cashReceipt.DocumentId, cancellationToken);
				_fiscalizationResultSaver.SaveResult(cashReceipt, fiscalResult);

				uow.Save(cashReceipt);
				uow.Commit();

				return;
			}
		}

		public async Task RefreshDocForReceiptManually(int receiptId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Обновление статуса фискального документа"))
			{
				var cashReceipt = uow.GetById<CashReceipt>(receiptId);
				if(cashReceipt == null)
				{
					var errorMessage = $"Чек c идентификатором {receiptId} не найден";
					throw new InvalidOperationException(errorMessage);
				}

				var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(cashReceipt, cancellationToken);
				var fiscalResult = await cashboxClient.CheckFiscalDocument(cashReceipt.DocumentId, cancellationToken);

				if(fiscalResult?.SendStatus == CashReceipts.DTO.SendStatus.Error)
				{
					throw new InvalidOperationException(fiscalResult?.FailDescription ?? "Непредвиденная ошибка");
				}

				_fiscalizationResultSaver.SaveResult(cashReceipt, fiscalResult);

				uow.Save(cashReceipt);
				uow.Commit();

				return;
			}
		}
	}
}

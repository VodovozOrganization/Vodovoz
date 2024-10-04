using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Models.CashReceipts;

namespace VodovozBusiness.Models.CashReceipts
{
	public class FiscalDocumentRequeueService
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly CashboxClientProvider _cashboxClientProvider;

		public FiscalDocumentRequeueService(
			IUnitOfWorkFactory uowFactory,
			CashboxClientProvider cashboxClientProvider)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
		}

		public async Task RequeueDocForReceipt(int receiptId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var cashReceipt = uow.GetById<CashReceipt>(receiptId);
				if(cashReceipt == null)
				{
					return;
				}

				var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(cashReceipt, cancellationToken);
				var fiscalResult = await cashboxClient.RequeueFiscalDocument(cashReceipt.DocumentId, cancellationToken);

				if(fiscalResult.Status != Vodovoz.Models.CashReceipts.DTO.FiscalDocumentStatus.Queued)
				{
					return;
				}

				cashReceipt.FiscalDocumentStatus = FiscalDocumentStatus.Queued;

				uow.Save(cashReceipt);
				uow.Commit();
			}
		}
	}
}

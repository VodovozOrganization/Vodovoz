using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class ReceiptManualController
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FiscalizationResultSaver _fiscalizationResultSaver;

		public ReceiptManualController(IUnitOfWorkFactory uowFactory, FiscalizationResultSaver fiscalizationResultSaver)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_fiscalizationResultSaver = fiscalizationResultSaver ?? throw new ArgumentNullException(nameof(fiscalizationResultSaver));
		}

		public void ForceSendDuplicatedReceipt(int receiptId)
		{
			if(receiptId <= 0)
			{
				throw new ArgumentException("Должен быть указан валидный код чека", nameof(receiptId));
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var cashReceipt = uow.GetById<CashReceipt>(receiptId);
				if(cashReceipt.Status != CashReceiptStatus.DuplicateSum)
				{
					throw new InvalidOperationException("Принудительная отправка чека возможна только если это чек дубль");
				}

				cashReceipt.ManualSent = true;
				cashReceipt.Status = CashReceiptStatus.New;
				uow.Save(cashReceipt);
				uow.Commit();
			}
		}
	}
}

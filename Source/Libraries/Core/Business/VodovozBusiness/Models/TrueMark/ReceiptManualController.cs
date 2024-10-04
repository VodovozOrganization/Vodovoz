using CashReceiptApi;
using CashReceiptApi.Client.Framework;
using Grpc.Core;
using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class ReceiptManualController
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly CashReceiptClientChannelFactory _cashReceiptClientChannelFactory;
		private readonly IInteractiveMessage _interactiveMessage;

		public ReceiptManualController(IUnitOfWorkFactory uowFactory, CashReceiptClientChannelFactory cashReceiptClientChannelFactory, IInteractiveMessage interactiveMessage)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_cashReceiptClientChannelFactory = cashReceiptClientChannelFactory ?? throw new ArgumentNullException(nameof(cashReceiptClientChannelFactory));
			_interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
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
				if(cashReceipt.Status != CashReceiptStatus.DuplicateSum && cashReceipt.Status != CashReceiptStatus.ReceiptNotNeeded)
				{
					throw new InvalidOperationException(
						"Принудительная отправка чека возможна если это чек дубль или чек не требуется");
				}

				cashReceipt.ManualSent = true;
				cashReceipt.Status = CashReceiptStatus.New;
				uow.Save(cashReceipt);
				uow.Commit();
			}
		}

		public void RefreshFiscalDoc(int receiptId)
		{
			if(receiptId <= 0)
			{
				throw new ArgumentException("Должен быть указан валидный код чека", nameof(receiptId));
			}

			using(var receiptServiceChannel = _cashReceiptClientChannelFactory.OpenChannel())
			{
				var request = new RefreshReceiptRequest();
				request.CashReceiptId = receiptId;
				receiptServiceChannel.Client.RefreshFiscalDocument(request);
			}
		}

		public void RequeueFiscalDoc(int receiptId)
		{
			if(receiptId <= 0)
			{
				throw new ArgumentException("Должен быть указан валидный код чека", nameof(receiptId));
			}

			using(var receiptServiceChannel = _cashReceiptClientChannelFactory.OpenChannel())
			{
				var request = new RequeueDocumentRequest();
				request.CashReceiptId = receiptId;
				receiptServiceChannel.Client.RequeueFiscalDocument(request);
			}
		}
	}
}

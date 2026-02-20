using System;
using FastPaymentsApi.Contracts;
using QS.DomainModel.UoW;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.FastPayments;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Services.Orders;

namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentManager : IFastPaymentManager
	{
		private readonly IFastPaymentSettings _fastPaymentSettings;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderOnlinePaymentAcceptanceHandler _onlinePaymentAcceptanceHandler;

		public FastPaymentManager(
			IFastPaymentSettings fastPaymentSettings,
			IOrderSettings orderSettings,
			IOrderOnlinePaymentAcceptanceHandler onlinePaymentAcceptanceHandler)
		{
			_fastPaymentSettings =
				fastPaymentSettings ?? throw new ArgumentNullException(nameof(fastPaymentSettings));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_onlinePaymentAcceptanceHandler =
				onlinePaymentAcceptanceHandler ?? throw new ArgumentNullException(nameof(onlinePaymentAcceptanceHandler));
		}

		public bool IsTimeToCancelPayment(DateTime fastPaymentCreationDate, bool fastPaymentWithQRNotFromOnline, bool fastPaymentFromOnline)
		{
			var elapsedTime = (DateTime.Now - fastPaymentCreationDate).TotalMinutes;

			if(fastPaymentWithQRNotFromOnline)
			{
				if(elapsedTime > _fastPaymentSettings.GetQRLifetime)
				{
					return true;
				}
			}
			else if(fastPaymentFromOnline)
			{
				if(elapsedTime > _fastPaymentSettings.GetOnlinePayByQRLifetime)
				{
					return true;
				}
			}
			else
			{
				if(elapsedTime > _fastPaymentSettings.GetPayUrlLifetime)
				{
					return true;
				}
			}

			return false;
		}

		public void UpdateFastPaymentStatus(IUnitOfWork uow, FastPayment fastPayment, FastPaymentDTOStatus newStatus, DateTime statusDate)
		{
			switch(newStatus)
			{
				case FastPaymentDTOStatus.Processing:
					fastPayment.SetProcessingStatus();
					break;
				case FastPaymentDTOStatus.Performed:
					if(fastPayment.Order != null)
					{
						//Для старых быстрых платежей, которые могут остаться после обновления
						if(fastPayment.PaymentByCardFrom == null)
						{
							SetPaymentByCardFrom(uow, fastPayment);
						}

						_onlinePaymentAcceptanceHandler.AcceptOnlinePayment(
							uow,
							new[] { fastPayment.Order },
							fastPayment.ExternalId,
							fastPayment.PaymentType,
							fastPayment.PaymentByCardFrom);
					}

					fastPayment.SetPerformedStatusForOnlineOrder(statusDate);

					break;
				case FastPaymentDTOStatus.Rejected:
					fastPayment.SetRejectedStatus();
					break;
				default:
					throw new InvalidOperationException("Неизвестный статус оплаты");
			}
		}

		private void SetPaymentByCardFrom(IUnitOfWork uow, FastPayment fastPayment)
		{
			int paymentFromId;
			if(fastPayment.Order != null)
			{
				paymentFromId = fastPayment.FastPaymentPayType == FastPaymentPayType.ByCard
					? _orderSettings.GetPaymentByCardFromAvangardId
					: _orderSettings.GetPaymentByCardFromFastPaymentServiceId;
			}
			else
			{
				paymentFromId = _orderSettings.GetPaymentByCardFromSiteByQrCodeId;
			}

			fastPayment.PaymentByCardFrom = uow.GetById<PaymentFrom>(paymentFromId);
		}
	}
}

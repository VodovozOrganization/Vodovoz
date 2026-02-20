using System;
using System.Linq;
using FastPaymentsApi.Contracts;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Orders;

namespace FastPaymentsAPI.Library.Converters
{
	public class RequestFromConverter : IRequestFromConverter
	{
		private readonly IOrderSettings _orderSettings;

		public RequestFromConverter(IOrderSettings orderSettings)
		{
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
		}

		public PaymentFrom ConvertRequestFromTypeToPaymentFrom(IUnitOfWork uow, FastPaymentRequestFromType fastPaymentRequestFromType)
		{
			switch(fastPaymentRequestFromType)
			{
				case FastPaymentRequestFromType.FromDesktopByQr:
				case FastPaymentRequestFromType.FromDriverAppByQr:
					return uow.GetAll<PaymentFrom>()
						.SingleOrDefault(x => x.Id == _orderSettings.GetPaymentByCardFromFastPaymentServiceId);
				case FastPaymentRequestFromType.FromSiteByQr:
					return uow.GetAll<PaymentFrom>()
						.SingleOrDefault(x => x.Id == _orderSettings.GetPaymentByCardFromSiteByQrCodeId);
				case FastPaymentRequestFromType.FromDesktopByCard:
					return uow.GetAll<PaymentFrom>()
						.SingleOrDefault(x => x.Id == _orderSettings.GetPaymentByCardFromAvangardId);
				case FastPaymentRequestFromType.FromMobileAppByQr:
					return uow.GetAll<PaymentFrom>()
						.SingleOrDefault(x => x.Id == _orderSettings.GetPaymentByCardFromMobileAppByQrCodeId);
				case FastPaymentRequestFromType.FromAiBotByQr:
					return uow.GetAll<PaymentFrom>()
						.SingleOrDefault(x => x.Id == _orderSettings.GetPaymentByCardFromAiBotByQrCodeId);
				default:
					throw new ArgumentOutOfRangeException(nameof(fastPaymentRequestFromType), fastPaymentRequestFromType, null);
			}
		}
	}
}

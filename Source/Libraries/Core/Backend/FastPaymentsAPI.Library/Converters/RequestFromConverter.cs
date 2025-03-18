using System;
using System.Linq;
using FastPaymentsApi.Contracts;
using QS.DomainModel.UoW;
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

		public PaymentFrom ConvertRequestFromTypeToPaymentFrom(IUnitOfWork uow, RequestFromType requestFromType)
		{
			switch(requestFromType)
			{
				case RequestFromType.FromDesktopByQr:
				case RequestFromType.FromDriverAppByQr:
					return uow.GetAll<PaymentFrom>()
						.SingleOrDefault(x => x.Id == _orderSettings.GetPaymentByCardFromFastPaymentServiceId);
				case RequestFromType.FromSiteByQr:
					return uow.GetAll<PaymentFrom>()
						.SingleOrDefault(x => x.Id == _orderSettings.GetPaymentByCardFromSiteByQrCodeId);
				case RequestFromType.FromDesktopByCard:
					return uow.GetAll<PaymentFrom>()
						.SingleOrDefault(x => x.Id == _orderSettings.GetPaymentByCardFromAvangardId);
				case RequestFromType.FromMobileAppByQr:
					return uow.GetAll<PaymentFrom>()
						.SingleOrDefault(x => x.Id == _orderSettings.GetPaymentByCardFromMobileAppByQrCodeId);
				default:
					throw new ArgumentOutOfRangeException(nameof(requestFromType), requestFromType, null);
			}
		}
	}
}

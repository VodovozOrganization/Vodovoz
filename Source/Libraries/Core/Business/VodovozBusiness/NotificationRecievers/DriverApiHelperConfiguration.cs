using System;

namespace Vodovoz.NotificationRecievers
{
	public class DriverApiHelperConfiguration
	{
		public Uri ApiBase { get; set; }
		public string NotifyOfSmsPaymentStatusChangedURI { get; set; }
		public string NotifyOfFastDeliveryOrderAddedURI { get; set; }
		public string NotifyOfWaitingTimeChangedURI { get; set; }
		public string NotifyOfOrderWithGoodsTransferingIsTransferedUri { get; set; }
		public string NotifyOfCashRequestForDriverIsGivenForTakeUri { get; set; }
	}
}

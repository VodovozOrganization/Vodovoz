using System;

namespace Vodovoz.Settings.Logistics
{
	public interface IDriverApiSettings
	{
		string CompanyPhoneNumber { get; }
		int ComplaintSourceId { get; }
		Uri ApiBase { get; }
		string NotifyOfSmsPaymentStatusChangedUri { get; }
		string NotifyOfFastDeliveryOrderAddedUri { get; }
		string NotifyOfWaitingTimeChangedURI { get; }
		string NotifyOfRouteListChangedUri { get; }
		string NotifyOfCashRequestForDriverIsGivenForTakeUri { get;}
		int DriverApiUserId { get; }
	}
}

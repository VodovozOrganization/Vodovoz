using System;

namespace Vodovoz.Services
{
    public interface IDriverApiParametersProvider
    {
        string CompanyPhoneNumber { get; }
        int ComplaintSourceId { get; }
        Uri ApiBase { get; }
        string NotifyOfSmsPaymentStatusChangedUri { get; }
        string NotifyOfFastDeliveryOrderAddedUri { get; }
		string NotifyOfWaitingTimeChangedURI { get; }
	}
}

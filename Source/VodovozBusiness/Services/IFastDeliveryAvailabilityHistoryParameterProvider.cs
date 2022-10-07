using System;

namespace Vodovoz.Services
{
	public interface IFastDeliveryAvailabilityHistoryParameterProvider
	{
		int FastDeliveryHistoryStorageDays { get; }
		DateTime FastDeliveryHistoryClearDate { get; }
		void UpdateFastDeliveryHistoryClearDate(string value);
	}
}

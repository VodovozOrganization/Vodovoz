using System;

namespace Vodovoz.Settings.Delivery
{
	public interface IFastDeliveryAvailabilityHistorySettings
	{
		int FastDeliveryHistoryStorageDays { get; }
		DateTime FastDeliveryHistoryClearDate { get; }
		void UpdateFastDeliveryHistoryClearDate(string value);
	}
}

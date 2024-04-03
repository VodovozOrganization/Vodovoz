using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.Models
{
	public interface IFastDeliveryAvailabilityHistoryModel
	{
		void SaveFastDeliveryAvailabilityHistory(FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory);
		void ClearFastDeliveryAvailabilityHistory(IFastDeliveryAvailabilityHistorySettings fastDeliveryAvailabilityHistorySettings);

	}
}

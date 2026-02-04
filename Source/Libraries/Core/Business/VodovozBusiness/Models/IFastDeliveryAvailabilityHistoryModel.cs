using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.Models
{
	public interface IFastDeliveryAvailabilityHistoryModel
	{
		void SaveFastDeliveryAvailabilityHistory(FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory);

		Task SaveFastDeliveryAvailabilityHistoryAsync(
			FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory,
			CancellationToken cancellationToken
		);

		void ClearFastDeliveryAvailabilityHistory(IFastDeliveryAvailabilityHistorySettings fastDeliveryAvailabilityHistorySettings, TimeSpan? queryTimeoutTimeSpan = null);
	}
}

using Vodovoz.Domain.Roboats;
using Vodovoz.Models;

namespace Vodovoz.Factories
{
	public interface IRoboatsFileStorageFactory
	{
		RoboatsFileStorage CreateAddressStorage();
		RoboatsFileStorage CreateDeliveryScheduleStorage();
		RoboatsFileStorage CreateStorage(RoboatsEntityType roboatsEntityType);
		RoboatsFileStorage CreateWaterTypeStorage();
	}
}
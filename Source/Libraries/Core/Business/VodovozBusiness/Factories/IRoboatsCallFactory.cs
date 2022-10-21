using System;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.Factories
{
	public interface IRoboatsCallFactory
	{
		RoboatsCall GetNewRoboatsCall(string phone, Guid callGuid);
		RoboatsCallDetail GetNewRoboatsCallDetail(RoboatsCall call, RoboatsCallOperation operation, string description);
		RoboatsCallDetail GetNewRoboatsCallDetail(RoboatsCall call, RoboatsCallFailType failType, RoboatsCallOperation operation, string description);
	}
}

using System;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.Factories
{
	public class RoboatsCallFactory : IRoboatsCallFactory
	{
		public RoboatsCall GetNewRoboatsCall(string phone)
		{
			return new RoboatsCall
			{
				Phone = phone,
				CallTime = DateTime.Now
			};
		}

		public RoboatsCallDetail GetNewRoboatsCallDetail(RoboatsCall call, RoboatsCallOperation operation, string description)
		{
			return new RoboatsCallDetail
			{
				Call = call,
				OperationTime = DateTime.Now,
				Operation = operation,
				Description = description
			};
		}

		public RoboatsCallDetail GetNewRoboatsCallDetail(RoboatsCall call, RoboatsCallFailType failType, RoboatsCallOperation operation, string description)
		{
			var callDetail = GetNewRoboatsCallDetail(call, operation, description);
			callDetail.FailType = failType;
			return callDetail;
		}
	}
}

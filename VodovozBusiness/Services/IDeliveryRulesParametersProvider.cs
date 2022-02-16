using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Services
{
	public interface IDeliveryRulesParametersProvider
	{
		bool IsStoppedOnlineDeliveriesToday { get; }
		void UpdateOnlineDeliveriesTodayParameter(string value);
		TimeSpan MaxTrackPointTimeOffsetForOneHourDelivery { get; }
		double MaxDistanceToLatestTrackPointForOneHourDeliveryKm { get; }
		int OneHourDeliveryScheduleId { get; }
	}
}

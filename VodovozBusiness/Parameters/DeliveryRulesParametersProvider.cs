using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class DeliveryRulesParametersProvider : IDeliveryRulesParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;
		private const string _onlineDeliveriesTodayParameter = "is_stopped_online_deliveries_today";

		public DeliveryRulesParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public bool IsStoppedOnlineDeliveriesToday => _parametersProvider.GetBoolValue(_onlineDeliveriesTodayParameter);

		public void UpdateOnlineDeliveriesTodayParameter(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_onlineDeliveriesTodayParameter, value);

		public TimeSpan MaxTrackPointTimeOffsetForOneHourDelivery =>
			_parametersProvider.GetValue<TimeSpan>("max_track_point_time_offset_for_one_hour_delivery");

		public double MaxDistanceToLatestTrackPointForOneHourDeliveryKm =>
			_parametersProvider.GetValue<double>("max_distance_to_latest_track_point_for_one_hour_delivery_km");

		public int OneHourDeliveryScheduleId => _parametersProvider.GetValue<int>("one_hour_delivery_schedule_id");
	}
}

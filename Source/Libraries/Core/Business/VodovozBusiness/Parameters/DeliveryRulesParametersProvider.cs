using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class DeliveryRulesParametersProvider : IDeliveryRulesParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;
		private const string _onlineDeliveriesTodayParameter = "is_stopped_online_deliveries_today";
		private const string _bottlesCountForFlyerParameter = "bottles_count_for_flyer";
		private const string _additionalLoadingFlyerAdditionEnabledParameter = "additional_loading_flyer_addition_enabled";

		private const string _additionalLoadingFlyerForNewCounterpartyBottlesCountParameter =
			"additional_loading_flyer_for_new_counterparty_bottles_count";
		private const string _additionalLoadingFlyerForNewCounterpartyEnabledParameter =
			"additional_loading_flyer_for_new_counterparty_enabled";

		public DeliveryRulesParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public bool IsStoppedOnlineDeliveriesToday => _parametersProvider.GetBoolValue(_onlineDeliveriesTodayParameter);
		public int BottlesCountForFlyer => _parametersProvider.GetValue<int>(_bottlesCountForFlyerParameter);
		public bool AdditionalLoadingFlyerAdditionEnabled =>
			_parametersProvider.GetValue<bool>(_additionalLoadingFlyerAdditionEnabledParameter);

		public int FlyerForNewCounterpartyBottlesCount =>
			_parametersProvider.GetValue<int>(_additionalLoadingFlyerForNewCounterpartyBottlesCountParameter);

		public bool FlyerForNewCounterpartyEnabled =>
			_parametersProvider.GetValue<bool>(_additionalLoadingFlyerForNewCounterpartyEnabledParameter);

		public void UpdateOnlineDeliveriesTodayParameter(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_onlineDeliveriesTodayParameter, value);

		public void UpdateBottlesCountForFlyerParameter(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_bottlesCountForFlyerParameter, value);

		public void UpdateAdditionalLoadingFlyerAdditionEnabledParameter(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_additionalLoadingFlyerAdditionEnabledParameter, value);

		public void UpdateFlyerForNewCounterpartyBottlesCountParameter(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_additionalLoadingFlyerForNewCounterpartyBottlesCountParameter, value);

		public void UpdateFlyerForNewCounterpartyEnabledParameter(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_additionalLoadingFlyerForNewCounterpartyEnabledParameter, value);

		#region FastDelivery

		public double MaxDistanceToLatestTrackPointKm
		{
			get
			{
				using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("GetMaxDistanceToLatestTrackPointKm"))
				{
					return unitOfWork.Query<FastDeliveryMaxDistanceParameterVersion>()
						.Where(x => x.EndDate == null)
						.SingleOrDefault().Value;
				}
			}
		}

		public double GetMaxDistanceToLatestTrackPointKmFor(DateTime dateTime)
		{
			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("GetMaxDistanceToLatestTrackPointKm"))
			{
				FastDeliveryMaxDistanceParameterVersion fastDeliveryMaxDistanceParameterVersionAlias = null;

				return unitOfWork.Session.QueryOver<FastDeliveryMaxDistanceParameterVersion>(() => fastDeliveryMaxDistanceParameterVersionAlias)
					.Where(Restrictions.And(
						Restrictions.Le(Projections.Property(() => fastDeliveryMaxDistanceParameterVersionAlias.StartDate), dateTime),
						Restrictions.Or(
							Restrictions.Gt(Projections.Property(() => fastDeliveryMaxDistanceParameterVersionAlias.EndDate), dateTime),
							Restrictions.IsNull(Projections.Property(() => fastDeliveryMaxDistanceParameterVersionAlias.EndDate)))))
					.SingleOrDefault().Value;
			}
		}

		public int DriverGoodWeightLiftPerHandInKg => _parametersProvider.GetValue<int>("fast_delivery_driver_weight_lift_kg");
		public int MaxFastOrdersPerSpecificTime => _parametersProvider.GetValue<int>("fast_delivery_max_orders_per_time");
		public int FastDeliveryScheduleId => _parametersProvider.GetValue<int>("fast_delivery_schedule_id");
		public TimeSpan MaxTimeOffsetForLatestTrackPoint => _parametersProvider.GetValue<TimeSpan>("fast_delivery_time_offset");
		public TimeSpan MaxTimeForFastDelivery => _parametersProvider.GetValue<TimeSpan>("fast_delivery_time");
		public TimeSpan MinTimeForNewFastDeliveryOrder => _parametersProvider.GetValue<TimeSpan>("fast_delivery_min_new_order_time");
		public TimeSpan DriverUnloadTime => _parametersProvider.GetValue<TimeSpan>("fast_delivery_driver_unload_time");
		public TimeSpan SpecificTimeForMaxFastOrdersCount => _parametersProvider.GetValue<TimeSpan>("fast_delivery_time_for_max_orders");
		public void UpdateFastDeliveryMaxDistanceParameter(double value)
		{
			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("SetMaxDistanceToLatestTrackPointKm"))
			{
				var activationTime = DateTime.Now;

				var lastVersion = unitOfWork.Query<FastDeliveryMaxDistanceParameterVersion>()
					.Where(x => x.EndDate == null)
					.SingleOrDefault();

				if(lastVersion.Value == value)
				{
					return;
				}

				lastVersion.EndDate = activationTime;

				var newVersion = new FastDeliveryMaxDistanceParameterVersion
				{
					StartDate = activationTime,
					Value = value
				};

				unitOfWork.Save(lastVersion);
				unitOfWork.Save(newVersion);

				unitOfWork.Commit();
			}
		}

		#endregion
	}
}

using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class FastDeliveryAvailabilityHistoryParameterProvider: IFastDeliveryAvailabilityHistoryParameterProvider
	{
		private const string _fastDeliveryHistoryClearDate = "fast_delivery_availability_history_clear_date";
		private readonly IParametersProvider _parametersProvider;

		public FastDeliveryAvailabilityHistoryParameterProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}
		public void UpdateFastDeliveryHistoryClearDate(string value)
		{
			_parametersProvider.CreateOrUpdateParameter(_fastDeliveryHistoryClearDate, value);
		}

		public int FastDeliveryHistoryStorageDays => _parametersProvider.GetValue<int>("fast_delivery_availability_history_storage_days");
		public DateTime FastDeliveryHistoryClearDate => _parametersProvider.GetValue<DateTime>(_fastDeliveryHistoryClearDate);
	}
}

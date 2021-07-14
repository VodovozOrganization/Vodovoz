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
	}
}
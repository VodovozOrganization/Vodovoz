using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class DeliveryRulesParametersProvider : IDeliveryRulesParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public DeliveryRulesParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}
        
		public bool IsStoppedOnlineDeliveriesToday => _parametersProvider.GetBoolValue("is_stopped_online_deliveries_today");
	}
}
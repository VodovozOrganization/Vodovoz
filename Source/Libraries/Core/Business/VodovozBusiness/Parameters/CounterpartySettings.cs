using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class CounterpartySettings : ICounterpartySettings
	{
		private readonly IParametersProvider _parametersProvider;

		public CounterpartySettings(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider;
		}

		public string RevenueServiceClientAccessToken => _parametersProvider.GetStringValue("RevenueServiceClientAccessToken");
	}
}

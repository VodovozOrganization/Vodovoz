using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class GlobalSettings : IGlobalSettings
	{
		private readonly IParametersProvider _parametersProvider;

		public GlobalSettings(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}
		public string OsrmServiceUrl => _parametersProvider.GetStringValue("osrm_url");

		public bool ExcludeToll =>  _parametersProvider.GetBoolValue("osrm_exclude_toll");
	}
}

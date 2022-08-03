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

		#region Open Source Routing Machine (OSRM)

		public string OsrmServiceUrl => _parametersProvider.GetStringValue("osrm_url");

		public bool ExcludeToll =>  _parametersProvider.GetBoolValue("osrm_exclude_toll");

		#endregion Open Source Routing Machine (OSRM)

		#region Connection

		public bool SlaveConnectionEnabled => _parametersProvider.GetBoolValue("slave_connection_enabled");

		public string SlaveConnectionEnabledForThisDatabase => _parametersProvider.GetStringValue("slave_connection_enabled_for_this_database");

		public string SlaveConnectionHost => _parametersProvider.GetStringValue("slave_connection_host");

		public int SlaveConnectionPort => _parametersProvider.GetIntValue("slave_connection_port");

		#endregion Connection
	}
}

namespace Vodovoz.Settings.Common
{
	public interface IGlobalSettings
	{
		#region Open Source Routing Machine (OSRM)

		/// <summary>
		/// Ссылка на сервис расчета растояний
		/// </summary>
		string OsrmServiceUrl { get; }
		bool ExcludeToll { get; }

		#endregion Open Source Routing Machine (OSRM)

		#region Connection

		bool SlaveConnectionEnabled { get; }
		string SlaveConnectionEnabledForThisDatabase { get; }
		string SlaveConnectionHost { get; }
		int SlaveConnectionPort { get; }

		#endregion Connection
	}
}

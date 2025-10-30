namespace Vodovoz.Settings.Common
{
	public interface IOsrmSettings
	{
		#region Open Source Routing Machine (OSRM)

		/// <summary>
		/// Ссылка на сервис расчета растояний
		/// </summary>
		string OsrmServiceUrl { get; }
		bool ExcludeToll { get; }

		#endregion Open Source Routing Machine (OSRM)
	}
}

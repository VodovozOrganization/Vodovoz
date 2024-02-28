namespace Vodovoz.Settings.Common
{
	public interface IGlobalSettings
	{
		/// <summary>
		/// Ссылка на сервис расчета растояний
		/// </summary>
		string OsrmServiceUrl { get; }
		bool ExcludeToll { get; }
	}
}

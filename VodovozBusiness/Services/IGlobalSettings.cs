namespace Vodovoz.Services
{
	public interface IGlobalSettings
	{
		/// <summary>
		/// Ссылка на сервис расчета растояний
		/// </summary>
		string OsrmServiceUrl { get; }
	}
}

namespace Vodovoz.Settings.Warehouse
{
	public interface ILogisticsEventsSettings
	{
		/// <summary>
		/// Адрес API службы логистических событий
		/// </summary>
		string BaseUrl { get; }

		/// <summary>
		/// Id события начала сборки талона погрузки
		/// </summary>
		int CarLoadDocumentStartLoadEventId { get; }

		/// <summary>
		/// Id события окончания сборки талона погрузки
		/// </summary>
		int CarLoadDocumentEndLoadEventId { get; }
	}
}

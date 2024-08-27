namespace Vodovoz.Settings.Warehouse
{
	public interface ILogisticsEventsSettings
	{
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

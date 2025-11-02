namespace Vodovoz.Settings.Metrics
{
	/// <summary>
	/// Настройки для отправки метрик в Zabbix
	/// </summary>
	public interface IMetricSettings
	{
		/// <summary>
		/// Нужно ли отправлять метрики
		/// </summary>
		bool ZabbixNeedSendMetrics { get; }
		/// <summary>
		/// Хост для отправки
		/// </summary>
		string ZabbixHost { get; }
		/// <summary>
		/// Адрес отправки
		/// </summary>
		string ZabbixUrl { get; }
	}
}

namespace Mango.Contracts.V1.Options
{
	public class SyncOptions
	{
		public const string SectionName = "Sync";

		/// <summary>
		/// Частота запуска синхронизации, сек
		/// </summary>
		public int PollIntervalSeconds { get; set; } = 300;
		
		/// <summary>
		/// Промежуток для получения данных статистики, мин
		/// </summary>
		public int RangeMinutes  { get; set; } = 60;
		
		/// <summary>
		/// Количество попыток запроса статистики
		/// </summary>
		public int ResultRetryCount { get; set; } = 20;
		
		/// <summary>
		/// Задержка между запросами статистики
		/// </summary>
		public int ResultRetryDelaySeconds { get; set; } = 5;
	}
}

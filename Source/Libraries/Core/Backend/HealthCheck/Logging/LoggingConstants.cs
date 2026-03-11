namespace VodovozHealthCheck.Logging
{
	internal static class LoggingConstants
	{
		/// <summary>
		/// Имя кастомного фильтра, который подавляет логирование при health-check запросах.
		/// </summary>
		public const string HealthCheckSuppressLogFilterName = "HealthCheckSuppress";
	}
}

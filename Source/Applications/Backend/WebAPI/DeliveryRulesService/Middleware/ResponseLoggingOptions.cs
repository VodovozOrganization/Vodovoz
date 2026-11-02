using System;

namespace DeliveryRulesService.Middleware
{
	/// <summary>
	/// Настройки логирования тел ответов сервиса.
	/// </summary>
	public class ResponseLoggingOptions
	{
		/// <summary>
		/// Полные пути запросов без строки параметров. Пустой список отключает логирование ответов.
		/// </summary>
		public string[] Paths { get; set; } = Array.Empty<string>();
	}
}

using System.Threading;

namespace VodovozHealthCheck.Logging
{
	/// <summary>
	/// Хранение и передача флага подавления логирования 
	/// в рамках текущего асинхронного потока выполнения (execution context).
	/// </summary>
	/// <remarks>
	/// Используется для отключения логирования при обработке 
	/// health-check запросов (например, к эндпоинту /health или с заголовком X-Health-Check: true).	
	/// Флаг <see cref="SuppressLogging"/> сохраняется через все await и смену потоков благодаря <see cref="AsyncLocal{T}"/>.
	/// </remarks>
	internal static class LoggingContext
	{
		private static readonly AsyncLocal<bool> _suppressLogging = new();

		/// <summary>
		/// Флаг, указывающий, нужно ли подавлять (игнорировать) все сообщения логирования
		/// </summary>
		public static bool SuppressLogging
		{
			get => _suppressLogging.Value;
			set => _suppressLogging.Value = value;
		}
	}
}

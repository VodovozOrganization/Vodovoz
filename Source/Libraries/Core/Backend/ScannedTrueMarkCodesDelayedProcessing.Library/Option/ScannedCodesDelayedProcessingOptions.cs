using System;

namespace ScannedTrueMarkCodesDelayedProcessing.Library.Option
{
	/// <summary>
	/// Настройки работы воркера отложенной обработки отсканированных кодоы
	/// </summary>
	public class ScannedCodesDelayedProcessingOptions
	{
		/// <summary>
		/// Интервал работы
		/// </summary>
		public TimeSpan ScanInterval { get; set; } = TimeSpan.FromSeconds(30);
	}
}

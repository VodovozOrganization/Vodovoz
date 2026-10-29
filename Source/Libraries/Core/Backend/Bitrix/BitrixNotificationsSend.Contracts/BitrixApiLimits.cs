using System;

namespace BitrixNotificationsSend.Contracts
{
	/// <summary>
	/// Ограничения REST API Битрикс24
	/// </summary>
	public static class BitrixApiLimits
	{
		/// <summary>
		/// Максимальное количество команд в одном пакетном запросе batch.json
		/// </summary>
		public const int MaxBatchCommandsCount = 50;

		/// <summary>
		/// Лимит накопленного операционного времени метода в скользящем 10-минутном окне, сек
		/// </summary>
		public const double OperatingLimitSeconds = 480;

		/// <summary>
		/// Длительность скользящего окна операционного лимита
		/// </summary>
		public static readonly TimeSpan OperatingWindow = TimeSpan.FromMinutes(10);
	}
}

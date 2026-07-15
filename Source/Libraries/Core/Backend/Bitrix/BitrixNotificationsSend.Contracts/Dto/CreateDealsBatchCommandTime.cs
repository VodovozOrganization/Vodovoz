using System;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Данные о времени выполнения команды пакетного запроса из ответа Битрикс24
	/// </summary>
	public class CreateDealsBatchCommandTime
	{
		/// <summary>
		/// Накопленное операционное время метода, сек.
		/// Лимит Битрикс24 - 480 сек на метод в скользящем 10-минутном окне
		/// </summary>
		[JsonPropertyName("operating")]
		public double Operating { get; set; }

		/// <summary>
		/// Момент сброса операционного бюджета метода, unix-время в секундах,
		/// 0 - если Битрикс24 не прислал значение
		/// </summary>
		[JsonPropertyName("operating_reset_at")]
		public long OperatingResetAt { get; set; }

		/// <summary>
		/// Момент сброса операционного бюджета метода (UTC),
		/// null - если Битрикс24 не прислал значение
		/// </summary>
		[JsonIgnore]
		public DateTime? OperatingResetAtUtc =>
			OperatingResetAt > 0
			? DateTimeOffset.FromUnixTimeSeconds(OperatingResetAt).UtcDateTime
			: (DateTime?)null;
	}
}

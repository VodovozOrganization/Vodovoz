using System;

namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Запись идемпотентного запроса в хранилище: метка «в обработке» или сохранённый ответ
	/// </summary>
	public class IdempotencyRecord
	{
		/// <summary>
		/// Текущая версия формата записи
		/// </summary>
		public const int CurrentFormatVersion = 1;

		/// <summary>
		/// Версия формата записи - чтобы будущее изменение формата не ломало чтение старых записей
		/// </summary>
		public int FormatVersion { get; set; }

		/// <summary>
		/// Состояние записи
		/// </summary>
		public IdempotencyRecordState State { get; set; }

		/// <summary>
		/// Время начала обработки запроса, UTC (для <see cref="IdempotencyRecordState.Processing"/>)
		/// </summary>
		public DateTime? StartedAtUtc { get; set; }

		/// <summary>
		/// Имя экземпляра API, который обрабатывает запрос - для разбора инцидентов
		/// (для <see cref="IdempotencyRecordState.Processing"/>)
		/// </summary>
		public string Host { get; set; }

		/// <summary>
		/// Http-код сохранённого ответа (для <see cref="IdempotencyRecordState.Completed"/>)
		/// </summary>
		public int StatusCode { get; set; }

		/// <summary>
		/// Content-Type сохранённого ответа, если был (для <see cref="IdempotencyRecordState.Completed"/>)
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// Тело сохранённого ответа (для <see cref="IdempotencyRecordState.Completed"/>)
		/// </summary>
		public byte[] Body { get; set; }

		/// <summary>
		/// Время действия в мобильном приложении из заголовка запроса, UTC, если было передано.
		/// Только хранится: срок хранения записи от него не зависит
		/// </summary>
		public DateTime? ActionTimeUtc { get; set; }

		/// <summary>
		/// Время сохранения ответа, UTC (для <see cref="IdempotencyRecordState.Completed"/>)
		/// </summary>
		public DateTime? CompletedAtUtc { get; set; }

		/// <summary>
		/// Создание метки «в обработке»
		/// </summary>
		/// <param name="startedAtUtc">Время начала обработки, UTC</param>
		/// <param name="host">Имя экземпляра API</param>
		/// <returns>Запись в состоянии <see cref="IdempotencyRecordState.Processing"/></returns>
		public static IdempotencyRecord CreateProcessing(DateTime startedAtUtc, string host) =>
			new IdempotencyRecord
			{
				FormatVersion = CurrentFormatVersion,
				State = IdempotencyRecordState.Processing,
				StartedAtUtc = startedAtUtc,
				Host = host
			};

		/// <summary>
		/// Создание записи с сохранённым ответом
		/// </summary>
		/// <param name="statusCode">Http-код ответа</param>
		/// <param name="contentType">Content-Type ответа</param>
		/// <param name="body">Тело ответа</param>
		/// <param name="actionTimeUtc">Время действия в мобильном приложении, UTC</param>
		/// <param name="completedAtUtc">Время сохранения ответа, UTC</param>
		/// <returns>Запись в состоянии <see cref="IdempotencyRecordState.Completed"/></returns>
		public static IdempotencyRecord CreateCompleted(
			int statusCode,
			string contentType,
			byte[] body,
			DateTime? actionTimeUtc,
			DateTime completedAtUtc) =>
			new IdempotencyRecord
			{
				FormatVersion = CurrentFormatVersion,
				State = IdempotencyRecordState.Completed,
				StatusCode = statusCode,
				ContentType = contentType,
				Body = body,
				ActionTimeUtc = actionTimeUtc,
				CompletedAtUtc = completedAtUtc
			};
	}
}

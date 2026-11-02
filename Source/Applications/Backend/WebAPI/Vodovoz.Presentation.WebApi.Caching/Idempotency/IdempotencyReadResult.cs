using System;

namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Результат чтения записи идемпотентного запроса
	/// </summary>
	public sealed class IdempotencyReadResult
	{
		private IdempotencyReadResult(IdempotencyReadStatus status, IdempotencyRecord record)
		{
			Status = status;
			Record = record;
		}

		/// <summary>
		/// Статус чтения
		/// </summary>
		public IdempotencyReadStatus Status { get; }

		/// <summary>
		/// Прочитанная запись, заполнена только при <see cref="IdempotencyReadStatus.Found"/>
		/// </summary>
		public IdempotencyRecord Record { get; }

		/// <summary>
		/// Запись найдена
		/// </summary>
		/// <param name="record">Прочитанная запись</param>
		/// <returns>Результат со статусом <see cref="IdempotencyReadStatus.Found"/></returns>
		public static IdempotencyReadResult Found(IdempotencyRecord record) =>
			new IdempotencyReadResult(IdempotencyReadStatus.Found, record ?? throw new ArgumentNullException(nameof(record)));

		/// <summary>
		/// Записи нет
		/// </summary>
		public static IdempotencyReadResult NotFound { get; } = new IdempotencyReadResult(IdempotencyReadStatus.NotFound, null);

		/// <summary>
		/// Хранилище недоступно или запись нечитаема
		/// </summary>
		public static IdempotencyReadResult Unavailable { get; } = new IdempotencyReadResult(IdempotencyReadStatus.Unavailable, null);
	}
}

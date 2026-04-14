using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TransactionalOutbox.Domain;

namespace TransactionalOutbox.Abstractions
{
	/// <summary>
	/// Интерфейс репозитория для работы с сообщениями в таблице Outbox.
	/// </summary>

	public interface IOutboxRepository
	{
		/// <summary>
		/// Является ли указанный ключ дедупликации уникальным в аутбоксе.
		/// </summary>
		/// <param name="conn">Соединение БД</param>
		/// <param name="deduplicationKey">Ключ, который нужно проверить на уникальность.</param>
		/// <param name="transaction">Транзакция</param>
		Task<bool> IsUniqueAsync(IDbConnection conn, string deduplicationKey, IDbTransaction transaction = null);

		/// <summary>
		/// Сохраняет сообщение Outbox в базе данных
		/// </summary>
		/// <param name="conn">Соединение БД</param>
		/// <param name="message">Сообщение Outbox</param>
		/// <param name="transaction">Транзакция.</param>

		Task SaveAsync(IDbConnection conn, OutboxMessage message, IDbTransaction transaction = null);

		/// <summary>
		/// Получает список ожидающих отправки сообщений из таблицы Outbox.
		/// </summary>
		/// <param name="conn">Соединение БД</param>
		/// <param name="limit">Максимальное количество сообщений для получения</param>
		/// <param name="transaction">Транзакция.</param>
		/// <returns>Список сообщений Outbox.</returns>
		Task<List<OutboxMessage>> GetPendingMessagesAsync(IDbConnection conn, int limit = 50, IDbTransaction transaction = null);

		/// <summary>
		/// Помечает сообщение как отправленное.
		/// </summary>
		/// <param name="conn">Соединение БД</param>
		/// <param name="guid">Идентификатор сообщения</param>
		/// <param name="transaction">Транзакция.</param>
		Task MarkAsSentAsync(IDbConnection conn, Guid guid, IDbTransaction transaction = null);

		/// <summary>
		/// Увеличивает количество попыток отправки сообщения и сохраняет информацию об ошибке, если она указана.
		/// </summary>
		/// <param name="conn">Соединение БД</param>
		/// <param name="guid">Идентификатор сообщения, для которого нужно увеличить количество попыток отправки</param>
		/// <param name="error">Сообщение об ошибке, если оно имеется</param>
		/// <param name="transaction">Транзакция.</param>
		Task IncrementAttemptsAsync(IDbConnection conn, Guid guid, string error = null, IDbTransaction transaction = null);

		/// <summary>
		/// Очищает устаревшие или обработанные сообщения из таблицы Outbox.
		/// </summary>
		/// <param name="conn">Соединение БД</param>
		/// <param name="transaction">Транзакция.</param>
		Task CleanupAsync(IDbConnection conn, IDbTransaction transaction = null);

		/// <summary>
		/// Удаляет сообщение из таблицы Outbox по его идентификатору.
		/// </summary>
		/// <param name="conn">Соединение БД</param>
		/// <param name="guid">Идентификатор сообщения, которое нужно удалить</param>
		/// <param name="transaction">Транзакция.</param>
		Task DeleteAsync(IDbConnection conn, Guid guid, IDbTransaction transaction = null);
	}
}

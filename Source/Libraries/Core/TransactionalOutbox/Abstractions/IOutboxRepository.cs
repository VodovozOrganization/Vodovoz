using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TransactionalOutbox.Domain;

namespace TransactionalOutbox.Abstractions
{
	/// <summary>
	/// Работа с Outbox (Transactional Outbox)
	/// </summary>
	public interface IOutboxRepository
	{
		Task<bool> IsUniqueAsync(IDbConnection conn, string deduplicationKey, IDbTransaction transaction = null);
		Task SaveAsync(IDbConnection conn, OutboxMessage message, IDbTransaction transaction = null);
		Task<List<OutboxMessage>> GetPendingMessagesAsync(IDbConnection conn, int limit = 50, IDbTransaction transaction = null);
		Task MarkAsSentAsync(IDbConnection conn, Guid guid, IDbTransaction transaction = null);
		Task IncrementAttemptsAsync(IDbConnection conn, Guid guid, string error = null, IDbTransaction transaction = null);
		Task CleanupAsync(IDbConnection conn, IDbTransaction transaction = null);
		Task DeleteAsync(IDbConnection conn, Guid guid, IDbTransaction tx = null);
	}
}

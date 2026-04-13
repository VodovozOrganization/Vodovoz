using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Domain;

namespace TransactionalOutbox.Persistence
{
    public class OutboxRepository : IOutboxRepository
    {
        public async Task<bool> IsUniqueAsync(IDbConnection conn, string deduplicationKey, IDbTransaction transaction = null)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM outbox_messages 
                WHERE deduplication_key = @DeduplicationKey";

            var count = await conn.ExecuteScalarAsync<int>(sql, new { DeduplicationKey = deduplicationKey }, transaction);
            return count == 0;
        }

        public async Task SaveAsync(IDbConnection conn, OutboxMessage message, IDbTransaction transaction = null)
        {
            if (message == null)
            {
	            throw new ArgumentNullException(nameof(message));
            }

            const string sql = @"
                INSERT INTO outbox_messages (
                    payload, deduplication_key, message_type, created_at, attempts
                ) VALUES (
                    @Payload, @DeduplicationKey, @MessageType, @CreatedAt, @Attempts
                )";

            await conn.ExecuteAsync(sql, new
            {
                message.Payload,
                message.DeduplicationKey,
                message.MessageType,
                message.CreatedAt,
                message.Attempts
            }, transaction);
        }

        public async Task<List<OutboxMessage>> GetPendingMessagesAsync(IDbConnection conn, int limit = 50, IDbTransaction transaction = null)
        {
            var messages = (await conn.QueryAsync<OutboxMessage>(
                @"
                SELECT *
                FROM outbox_messages
                WHERE sent_at IS NULL
                  AND attempts < 5
                ORDER BY created_at
                LIMIT @Limit
                FOR UPDATE SKIP LOCKED",
                new { Limit = limit },
                transaction
            )).ToList();

            return messages;
        }

        public async Task MarkAsSentAsync(IDbConnection conn, Guid guid, IDbTransaction transaction = null)
        {
            await conn.ExecuteAsync(
                @"UPDATE outbox_messages
                  SET sent_at = UTC_TIMESTAMP(6)
                  WHERE guid = @Guid",
                new { Guid = guid },
                transaction
            );
        }

        public async Task IncrementAttemptsAsync(IDbConnection conn, Guid guid, string error = null, IDbTransaction transaction = null)
        {
            await conn.ExecuteAsync(
                @"
                UPDATE outbox_messages
                SET attempts = attempts + 1,
                    error = @Error
                WHERE guid = @Guid",
                new { Guid = guid, Error = error },
                transaction
            );
        }

        public async Task CleanupAsync(IDbConnection conn, IDbTransaction transaction = null)
        {
            await conn.ExecuteAsync(
                @"
                DELETE FROM outbox_messages
                WHERE sent_at IS NOT NULL
                  AND sent_at < UTC_TIMESTAMP() - INTERVAL 7 DAY",
                transaction: transaction
            );
        }
        
        public async Task DeleteAsync(
	        IDbConnection conn,
	        Guid guid,
	        IDbTransaction tx = null)
        {
	        const string sql = @"
		        DELETE FROM outbox_messages
		        WHERE guid = @Guid";

	        await conn.ExecuteAsync(sql, new { Guid = guid }, tx);
        }
    }
}

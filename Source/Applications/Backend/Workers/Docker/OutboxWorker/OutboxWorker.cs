using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Extensions;
using CustomerNotifications.Contracts;

namespace OutboxWorker
{
	public class OutboxWorker : BackgroundService
	{
		private readonly string _connectionString;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<OutboxWorker> _logger;

		// Кэш для ускорения разрешения типов.
		// Предполагается, что набор типов сообщений ограничен и известен заранее.
		// Если в будущем появятся новые типы, их нужно будет добавить в эту коллекцию.

		private static readonly Dictionary<string, Type> _knownTypes =
			typeof(CustomerNotificationIntegrationEvent).Assembly
				.GetTypes()
				.Where(t => t.FullName != null)
				.ToDictionary(t => t.FullName, t => t);

		public OutboxWorker(
			IConfiguration config,
			IServiceScopeFactory scopeFactory,
			ILogger<OutboxWorker> logger)
		{
			_connectionString = config.GetConnectionString("Default");
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		private Type ResolveType(string typeName)
		{
			if(_knownTypes.TryGetValue(typeName, out var type))
			{
				return type;
			}

			_logger.LogWarning("Тип не найден: {TypeName}", typeName);

			return null;
		}

		protected override async Task ExecuteAsync(CancellationToken token)
		{
			while(!token.IsCancellationRequested)
			{
				try
				{
					await using var conn = new MySqlConnection(_connectionString);
					await conn.OpenAsync(token);

					using var scope = _scopeFactory.CreateScope();

					var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
					var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
					
					await using var tx = await conn.BeginTransactionAsync(token);
					
					var messages = await outboxRepository.GetPendingMessagesAsync(conn, 50, tx);

					if(!messages.Any())
					{
						await tx.CommitAsync(token); // коммитим пустую транзакцию, чтобы снять блокировки
						await Task.Delay(1000, token);
						continue;
					}
					
					foreach(var msg in messages)
					{
						try
						{
							var type = ResolveType(msg.Type);

							if(type == null)
							{
								throw new Exception($"Type not found {msg.Type}");
							}

							var @event = msg.PayloadJson?.DeserializeFromOutbox(type);

							if(@event == null)
							{
								_logger.LogWarning("Десериализация вернула null для сообщения {Guid}", msg.Guid);

								await outboxRepository.IncrementAttemptsAsync(conn, msg.Guid, "Deserialization returned null", tx);

								continue;
							}
							
							await publishEndpoint.Publish(@event, type, token);
							
							await outboxRepository.MarkAsSentAsync(conn, msg.Guid, tx);
						}
						catch(Exception ex)
						{
							await outboxRepository.IncrementAttemptsAsync(conn, msg.Guid, ex.ToString(), tx);
							_logger.LogError(ex, "Outbox publish failed {Guid}", msg.Guid);
						}
					}
					
					await tx.CommitAsync(token);
					
					await outboxRepository.CleanupAsync(conn);

					await Task.Delay(100, token);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Outbox worker crash");
					await Task.Delay(5000, token);
				}
			}
		}
	}
}

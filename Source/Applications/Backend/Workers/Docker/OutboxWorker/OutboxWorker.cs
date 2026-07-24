using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Extensions;

namespace OutboxWorker
{
	public class OutboxWorker : BackgroundService
	{
		private readonly string _connectionString;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<OutboxWorker> _logger;
		private readonly IReadOnlyDictionary<string, Type> _knownTypes;
		private const int _messageBatchSize = 50;
		private const int _delayBeetweenMessagesInSeconds = 1;
		private const int _delayWhenErrorInSeconds = 5;

		public OutboxWorker(
			ILogger<OutboxWorker> logger,
			IConfiguration config,
			IServiceScopeFactory scopeFactory,
			IEnumerable<Assembly> outboxContractAssemblies)
		{
			if(config == null)
			{
				throw new ArgumentNullException(nameof(config));
			}

			if(outboxContractAssemblies == null)
			{
				throw new ArgumentNullException(nameof(outboxContractAssemblies));
			}

			_connectionString = config.GetConnectionString("Default");
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_knownTypes = BuildKnownTypes(outboxContractAssemblies, _logger);
		}

		private static IReadOnlyDictionary<string, Type> BuildKnownTypes(
			IEnumerable<Assembly> assemblies,
			ILogger logger)
		{
			var result = new Dictionary<string, Type>();

			foreach(var assembly in assemblies.Distinct())
			{
				foreach(var type in assembly.GetTypes().Where(t => t.FullName != null))
				{
					if(result.ContainsKey(type.FullName))
					{
						logger.LogWarning(
							"Коллизия полного имени типа {TypeFullName} между сборками при построении карты outbox-контрактов, используется первое найденное определение",
							type.FullName);

						continue;
					}

					result.Add(type.FullName, type);
				}
			}

			return result;
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

					var messages = await outboxRepository.GetPendingMessagesAsync(conn, _messageBatchSize, tx);

					if(!messages.Any())
					{
						await tx.CommitAsync(token);
						await Task.Delay(TimeSpan.FromSeconds(_delayBeetweenMessagesInSeconds), token);
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

							var @event = msg.Payload?.DeserializeFromOutbox(type);

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

					await Task.Delay(TimeSpan.FromSeconds(_delayBeetweenMessagesInSeconds), token);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Outbox worker crash");
					await Task.Delay(TimeSpan.FromSeconds(_delayWhenErrorInSeconds), token);
				}
			}
		}
	}
}

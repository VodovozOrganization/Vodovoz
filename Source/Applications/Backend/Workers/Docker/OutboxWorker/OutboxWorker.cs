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
using Vodovoz.Zabbix.Sender;

namespace OutboxWorker
{
	public class OutboxWorker : BackgroundService
	{
		private readonly string _connectionString;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IZabbixSender _zabbixSender;
		private readonly ILogger<OutboxWorker> _logger;
		private readonly IReadOnlyDictionary<string, (Type ClrType, Type BusType)> _messageDescriptors;
		private const int _messageBatchSize = 50;
		private const int _delayBeetweenMessagesInSeconds = 1;
		private const int _delayWhenErrorInSeconds = 5;
		private DateTime _lastHealthySentAt = DateTime.MinValue;

		public OutboxWorker(
			ILogger<OutboxWorker> logger,
			IConfiguration config,
			IServiceScopeFactory scopeFactory,
			IReadOnlyDictionary<Assembly, Type> assemblyToBus,
			IZabbixSender zabbixSender)
		{
			if(config == null)
			{
				throw new ArgumentNullException(nameof(config));
			}

			if(assemblyToBus == null || assemblyToBus.Count == 0)
			{
				throw new ArgumentException("Не передано ни одной сборки с контрактами событий/сообщений", nameof(assemblyToBus));
			}

			_connectionString = config.GetConnectionString("Default");
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_messageDescriptors = BuildMessageDescriptors(assemblyToBus, _logger);
		}

		private static IReadOnlyDictionary<string, (Type ClrType, Type BusType)> BuildMessageDescriptors(
			IReadOnlyDictionary<Assembly, Type> assemblyToBus,
			ILogger logger)
		{
			var result = new Dictionary<string, (Type, Type)>();

			foreach(var (assembly, busType) in assemblyToBus)
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

					result.Add(type.FullName, (type, busType));
				}
			}

			return result;
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

					await using var tx = await conn.BeginTransactionAsync(token);

					var messages = await outboxRepository.GetPendingMessagesAsync(conn, _messageBatchSize, tx);

					if(!messages.Any())
					{
						await tx.CommitAsync(token);						
						await SendIsHealthyThrottledAsync(token);
						await Task.Delay(TimeSpan.FromSeconds(_delayBeetweenMessagesInSeconds), token);
						continue;
					}

					foreach(var msg in messages)
					{
						try
						{
							if(!_messageDescriptors.TryGetValue(msg.Type, out var descriptor))
							{
								throw new Exception($"Type not found {msg.Type}");
							}

							var @event = msg.Payload?.DeserializeFromOutbox(descriptor.ClrType);

							if(@event == null)
							{
								_logger.LogWarning("Десериализация вернула null для сообщения {Guid}", msg.Guid);

								await outboxRepository.IncrementAttemptsAsync(conn, msg.Guid, "Deserialization returned null", tx);

								continue;
							}

							var publishEndpoint = (IPublishEndpoint)scope.ServiceProvider.GetRequiredService(descriptor.BusType);

							await publishEndpoint.Publish(@event, descriptor.ClrType, token);

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

					await SendIsHealthyThrottledAsync(token);

					await Task.Delay(TimeSpan.FromSeconds(_delayBeetweenMessagesInSeconds), token);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Outbox worker crash");
					await _zabbixSender.SendProblemMessageAsync(nameof(OutboxWorker), ZabixSenderMessageType.Problem, $"Outbox worker crash: {ex}", token);
					await Task.Delay(TimeSpan.FromSeconds(_delayWhenErrorInSeconds), token);
				}
			}
		}

		private async Task SendIsHealthyThrottledAsync(CancellationToken token)
		{
			if(DateTime.UtcNow - _lastHealthySentAt < TimeSpan.FromMinutes(1))
			{
				return;
			}

			await _zabbixSender.SendIsHealthyAsync(nameof(OutboxWorker), token);

			_lastHealthySentAt = DateTime.UtcNow;
		}
	}
}

using MassTransit;
using MessageTransport.MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Persistence;

namespace OutboxWorker
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Регистрирует издатель outbox-сообщений: подключение к RabbitMQ vhost'у
		/// уведомлений, IOutboxRepository и сам фоновый воркер публикации.
		/// </summary>
		/// <param name="contractAssemblies">
		/// Сборки с DTO интеграционных событий. Для публикации нового домена
		/// событий (например, нового вида алертов) достаточно добавить сюда его сборку.
		/// </param>
		/// <param name="transportSectionName">
		/// Имя секции конфигурации RabbitMQ vhost'а — должно совпадать с секцией,
		/// которую используют consumer-сервисы (например, "NotificationTransportSettings").
		/// </param>
		public static IServiceCollection AddOutboxWorker(
			this IServiceCollection services,
			IConfiguration configuration,
			Assembly[] contractAssemblies,
			string transportSectionName)
		{
			if(configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			if(contractAssemblies == null || contractAssemblies.Length == 0)
			{
				throw new ArgumentException("Не передано ни одной сборки с контрактами интеграционных событий", nameof(contractAssemblies));
			}

			if(string.IsNullOrEmpty(transportSectionName))
			{
				throw new ArgumentException("Не задано имя секции конфигурации транспорта", nameof(transportSectionName));
			}

			services.AddMassTransit(busConf =>
			{
				busConf.ConfigureRabbitMq(services, configuration, transportSectionName);
			});

			services.AddScoped<IOutboxRepository, OutboxRepository>();

			services.AddSingleton<IEnumerable<Assembly>>(contractAssemblies);

			services.AddHostedService<OutboxWorker>();

			return services;
		}
	}
}

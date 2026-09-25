using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Persistence;

namespace OutboxWorker
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Регистрирует outbox-воркер с поддержкой публикации в несколько транспортов
		/// (vhost'ов RabbitMQ) в зависимости от того, какой сборке принадлежит тип сообщения.
		/// </summary>
		public static IServiceCollection AddOutboxWorker(
			this IServiceCollection services,
			IConfiguration configuration,
			Action<OutboxTransportBuilder> configureTransports)
		{
			if(configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			if(configureTransports == null)
			{
				throw new ArgumentNullException(nameof(configureTransports));
			}

			var builder = new OutboxTransportBuilder(services, configuration);
			configureTransports(builder);

			if(!builder.AssemblyToBus.Any())
			{
				throw new ArgumentException("Не задано ни одного транспорта — вызовите Add<TBus>(...) хотя бы раз");
			}

			services.AddSingleton<IReadOnlyDictionary<Assembly, Type>>(builder.AssemblyToBus);

			services.AddScoped<IOutboxRepository, OutboxRepository>();
			services.AddHostedService<OutboxWorker>();

			return services;
		}
	}
}

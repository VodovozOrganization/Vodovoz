using MassTransit;
using MessageTransport.MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OutboxWorker
{
	public sealed class OutboxTransportBuilder
	{
		private readonly IServiceCollection _services;
		private readonly IConfiguration _configuration;

		internal readonly Dictionary<Assembly, Type> AssemblyToBus = new();

		internal OutboxTransportBuilder(IServiceCollection services, IConfiguration configuration)
		{
			_services = services;
			_configuration = configuration;
		}

		/// <summary>
		/// Регистрирует транспорт (vhost RabbitMQ, секция конфигурации) и привязывает к нему сборки
		/// контрактов: сообщения из этих сборок будут публиковаться именно в этот транспорт.
		/// </summary>
		public OutboxTransportBuilder Add<TBus>(
			string transportSectionName,
			Assembly[] contractAssemblies,
			Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configureTopology = null)
			where TBus : class, IBus
		{
			if(contractAssemblies == null || contractAssemblies.Length == 0)
			{
				throw new ArgumentException($"Не передано ни одной сборки контрактов для транспорта {transportSectionName}");
			}

			_services.AddMassTransit<TBus>(busConf =>
			{
				busConf.ConfigureRabbitMq(_services, _configuration, transportSectionName, configureTopology);
			});

			foreach(var assembly in contractAssemblies.Distinct())
			{
				AssemblyToBus[assembly] = typeof(TBus);
			}

			return this;
		}
	}
}

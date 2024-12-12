using MassTransit;
using MassTransit.Configuration;
using MassTransit.DependencyInjection.Registration;
using MassTransit.Monitoring;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Pacs.Core.Messages.Events;
using Pacs.Core.Messages.Filters;
using QS.DomainModel.Entity;
using RabbitMQ.Client;
using System;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using Vodovoz.Settings.Pacs;

namespace Pacs.Core
{
	public static class TransportConfiguration
	{
		public static void AddPacsBaseTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			cfg.Message<OperatorStateEvent>(x => x.SetEntityName("pacs.event.operator_state.publish"));
			cfg.Publish<OperatorStateEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Topic;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<SettingsEvent>(x => x.SetEntityName("pacs.event.settings.publish"));
			cfg.Publish<SettingsEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<GlobalBreakAvailabilityEvent>(x => x.SetEntityName("pacs.event.global_break_availability.publish"));
			cfg.Publish<GlobalBreakAvailabilityEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<OperatorsOnBreakEvent>(x => x.SetEntityName("pacs.event.operators_on_break.publish"));
			cfg.Publish<OperatorsOnBreakEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<PacsCallEvent>(x => x.SetEntityName("pacs.event.call.publish"));
			cfg.Publish<PacsCallEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.UsePublishFilter(typeof(PublishTimeToLiveFilter<>), context);

			//Исключение базовых классов
			cfg.Publish<IDomainObject>(x => x.Exclude = true);
			cfg.Publish<PropertyChangedBase>(x => x.Exclude = true);
		}

		public static IServiceCollection AddPacsMassTransit(
			this IServiceCollection services, 
			Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configureRabbit,
			Action<IBusRegistrationConfigurator> configureBus = null)
		{
			services.AddMassTransit(busCfg =>
			{
				configureBus?.Invoke(busCfg);

				busCfg.UsingRabbitMq((context, rabbitCfg) =>
				{
					var messageSettings = context.GetRequiredService<IMessageTransportSettings>();
					rabbitCfg.Host(messageSettings.Host, (ushort)messageSettings.Port, messageSettings.VirtualHost,
						rabbitHostCfg =>
						{
							rabbitHostCfg.Username(messageSettings.Username);
							rabbitHostCfg.Password(messageSettings.Password);
							if(messageSettings.UseSSL)
							{
								rabbitHostCfg.UseSsl(ssl =>
								{
									if(Enum.TryParse<SslPolicyErrors>(messageSettings.AllowSslPolicyErrors, out var allowedPolicyErrors))
									{
										ssl.AllowPolicyErrors(allowedPolicyErrors);
									}

									ssl.Protocol = SslProtocols.Tls12;
								});
							}
						}
					);

					rabbitCfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
					configureRabbit?.Invoke(context, rabbitCfg);

					rabbitCfg.ConfigureEndpoints(context);
				});
			});

			return services;
		}

		public static IServiceCollection AddPacsMassTransitNotHosted(
			this IServiceCollection services,
			Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configureRabbit,
			Action<IBusRegistrationConfigurator> configureBus = null,
			Action<IRegistrationFilterConfigurator> configureFilter = null,
			ConfigureEndpointsProviderCallback configureEndpointsCallback = null)
		{
			services.AddMassTransitServices(busCfg =>
			{
				configureBus?.Invoke(busCfg);

				if(configureEndpointsCallback != null)
				{					
					busCfg.AddConfigureEndpointsCallback(configureEndpointsCallback);
				}

				busCfg.UsingRabbitMq((context, rabbitCfg) =>
				{
					var messageSettings = context.GetRequiredService<IMessageTransportSettings>();
					rabbitCfg.Host(messageSettings.Host, (ushort)messageSettings.Port, messageSettings.VirtualHost,
						rabbitHostCfg =>
						{
							rabbitHostCfg.Username(messageSettings.Username);
							rabbitHostCfg.Password(messageSettings.Password);
							if(messageSettings.UseSSL)
							{
								rabbitHostCfg.UseSsl(ssl =>
								{
									if(Enum.TryParse<SslPolicyErrors>(messageSettings.AllowSslPolicyErrors, out var allowedPolicyErrors))
									{
										ssl.AllowPolicyErrors(allowedPolicyErrors);
									}

									ssl.Protocol = SslProtocols.Tls12;
								});
							}
						}
					);

					configureRabbit?.Invoke(context, rabbitCfg);

					rabbitCfg.ConfigureEndpoints(context, configureFilter);
				});
			});

			return services;
		}

		public static IServiceCollection AddMassTransitServices(this IServiceCollection collection, Action<IBusRegistrationConfigurator> configure = null)
		{
			if(collection.Any(d => d.ServiceType == typeof(IBus)))
			{
				throw new ConfigurationException(
					"AddMassTransit() was already called and may only be called once per container. To configure additional bus instances, refer to the documentation: https://masstransit-project.com/usage/containers/multibus.html");
			}

			collection.AddOptions();
			collection.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<HealthCheckServiceOptions>, ConfigureBusHealthCheckServiceOptions>());
			collection.AddOptions<MassTransitHostOptions>();

			var configurator = new ServiceCollectionBusConfigurator(collection);
			configure?.Invoke(configurator);
			configurator.Complete();

			return collection;
		}
	}
}

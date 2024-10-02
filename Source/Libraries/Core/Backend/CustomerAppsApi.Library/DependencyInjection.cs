using System;
using System.Net.Security;
using System.Security.Authentication;
using CustomerAppsApi.Factories;
using CustomerAppsApi.Library.Configs;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Factories;
using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Repositories;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Library.Validators;
using CustomerAppsApi.Models;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QS.Utilities.Numeric;
using RabbitMQ.Client;
using RabbitMQ.MailSending;
using Vodovoz.Controllers;
using Vodovoz.Controllers.ContactsForExternalCounterparty;
using Vodovoz.Factories;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Validation;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Database.Common;
using Vodovoz.Settings.Database.Delivery;
using Vodovoz.Settings.Database.Logistics;
using Vodovoz.Settings.Database.Roboats;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Pacs;
using Vodovoz.Settings.Roboats;
using VodovozInfrastructure.Cryptography;

namespace CustomerAppsApi.Library
{
	/// <summary>
	/// Методы расширения коллекции сервисов дял регистрации в контейнере зависимостей
	/// </summary>
	public static class DependencyInjection
	{
		/// <summary>
		/// Добавление сервисов библиотеки
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddCustomerApiLibrary(this IServiceCollection services)
		{
			services
				.AddScoped<ISendingService, SendingService>()
				.AddSingleton<PhoneFormatter>(_ => new PhoneFormatter(PhoneFormat.DigitsTen))
				.AddScoped<IRoboatsSettings, RoboatsSettings>()
				.AddScoped<IGlobalSettings, GlobalSettings>()
				.AddScoped<ICachedBottlesDebtRepository, CachedBottlesDebtRepository>()
				.AddScoped<IRegisteredNaturalCounterpartyDtoFactory, RegisteredNaturalCounterpartyDtoFactory>()
				.AddScoped<IExternalCounterpartyMatchingFactory, ExternalCounterpartyMatchingFactory>()
				.AddScoped<IExternalCounterpartyFactory, ExternalCounterpartyFactory>()
				.AddScoped<ICounterpartyModelFactory, CounterpartyModelFactory>()
				.AddScoped<ICounterpartyContractFactory, CounterpartyContractFactory>()
				.AddScoped<ICounterpartyFactory, CounterpartyFactory>()
				.AddScoped<INomenclatureFactory, NomenclatureFactory>()
				.AddScoped<IPromotionalSetFactory, PromotionalSetFactory>()
				.AddScoped<ICallTaskFactory, CallTaskSingletonFactory>()
				.AddScoped<IRentPackageFactory, RentPackageFactory>()
				.AddScoped<IDeliveryPointFactory, DeliveryPointFactory>()
				.AddScoped<ICameFromConverter, CameFromConverter>()
				.AddScoped<ISourceConverter, SourceConverter>()
				.AddScoped<ContactFinderForExternalCounterpartyFromOne>()
				.AddScoped<ContactFinderForExternalCounterpartyFromTwo>()
				.AddScoped<ContactFinderForExternalCounterpartyFromMany>()
				.AddScoped<IContactManagerForExternalCounterparty, ContactManagerForExternalCounterparty>()
				.AddScoped<IGoodsOnlineParametersController, GoodsOnlineParametersController>()
				.AddScoped<ICounterpartyModel, CounterpartyModel>()
				.AddScoped<INomenclatureModel, NomenclatureModel>()
				.AddScoped<IOrderModel, OrderModel>()
				.AddScoped<IPromotionalSetModel, PromotionalSetModel>()
				.AddScoped<ICallTaskWorker, CallTaskWorker>()
				.AddScoped<FastDeliveryHandler>()
				.AddScoped<IDriverApiSettings, DriverApiSettings>()
				.AddScoped<IDeliveryRulesSettings, DeliveryRulesSettings>()
				.AddScoped<IRouteListAddressKeepingDocumentController, RouteListAddressKeepingDocumentController>()
				.AddScoped<IFastDeliveryValidator, FastDeliveryValidator>()
				.AddScoped<IErrorReporter>(context => ErrorReporter.Instance)
				.AddScoped<IWarehouseModel, WarehouseModel>()
				.AddScoped<IRentPackageModel, RentPackageModel>()
				.AddScoped<IDeliveryPointService, DeliveryPointService>()
				.AddScoped<ICounterpartyModelValidator, CounterpartyModelValidator>()
				.AddScoped<IDeliveryPointModelValidator, DeliveryPointModelValidator>()
				.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>()
				.AddSingleton<SelfDeliveriesAddressesFrequencyRequestsHandler>()
				.AddSingleton<PricesFrequencyRequestsHandler>()
				.AddSingleton<NomenclaturesFrequencyRequestsHandler>()
				.AddSingleton<RentPackagesFrequencyRequestsHandler>();

			return services;
		}
		
		public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<RabbitOptions>(config.GetSection(RabbitOptions.Path));
			
			return services;
		}
		
		public static IBusRegistrationConfigurator ConfigureRabbitMq(this IBusRegistrationConfigurator busConf)
		{
			busConf.UsingRabbitMq((context, configurator) =>
			{
				var messageSettings = context.GetRequiredService<IMessageTransportSettings>();
				var rabbitOptions = context.GetRequiredService<IOptions<RabbitOptions>>().Value;

				configurator.Host(
					messageSettings.Host,
					(ushort)messageSettings.Port,
					messageSettings.VirtualHost, hostConfigurator =>
					{
						hostConfigurator.Username(messageSettings.Username);
						hostConfigurator.Password(messageSettings.Password);

						if(messageSettings.UseSSL)
						{
							hostConfigurator.UseSsl(ssl =>
							{
								if(Enum.TryParse<SslPolicyErrors>(messageSettings.AllowSslPolicyErrors, out var allowedPolicyErrors))
								{
									ssl.AllowPolicyErrors(allowedPolicyErrors);
								}

								ssl.Protocol = SslProtocols.Tls12;
							});
						}
					});
								
				//configurator.Send<OnlineOrderInfoDto>();
				configurator.Message<SendEmailMessage>(x => x.SetEntityName(rabbitOptions.SendExchange));
				configurator.Publish<SendEmailMessage>(x =>
				{
					x.ExchangeType = ExchangeType.Fanout;
					x.Durable = true;
					x.AutoDelete = false;
					x.BindQueue(
						rabbitOptions.SendExchange,
						rabbitOptions.SendQueue,
						conf =>
						{
							conf.ExchangeType = ExchangeType.Fanout;
						});
				});
								
				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}
	}
}

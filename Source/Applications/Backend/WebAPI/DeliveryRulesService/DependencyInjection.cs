using System;
using DeliveryRulesService.Cache;
using DeliveryRulesService.HealthChecks;
using DeliveryRulesService.Workers;
using Fias.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Osrm;
using QS.Project.Core;
using QS.Services;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DeliveryRulesService.Factories;
using DeliveryRulesService.Options;
using DeliveryRulesService.V2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vodovoz.Core.Application.Orders.Delivery;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Core.Domain.Interfaces.Orders;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Models;
using Vodovoz.Presentation.WebApi;
using Vodovoz.Settings.Common;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using VodovozHealthCheck;

namespace DeliveryRulesService
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDeliveryRulesService(
			this IServiceCollection services,
			IConfiguration configuration
			)
		{
			services
				.AddSwaggerGen(opt =>
					opt.CustomSchemaIds(type => type.FullName))
				.AddMvc()
				.AddControllersAsServices();

			services
				.AddControllers()
				//TODO после отключения первой версии можно убрать настройки, чтобы по умолчанию был CamelCase
				.AddJsonOptions(options =>
				{
					// глобальные настройки
					options.JsonSerializerOptions.Converters.Add(
						new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
				})
				
				.AddJsonOptions("v1", options => {
					// Настройки для версии 1: PascalCase для имён свойств
					options.JsonSerializerOptions.PropertyNamingPolicy = null;
				})
				;

			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(EmployeeWithLoginMap).Assembly,
					typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()

				.ConfigureHealthCheckService<DeliveryRulesServiceHealthCheck, ServiceInfoProvider>()
				.AddHttpClient()
				.AddFiasClient()
				.AddOsrm()
				.AddVersioning()
				;

			services.Replace(ServiceDescriptor.Scoped(typeof(IOsrmSettings), typeof(DeliveryRulesOsrmSettings)));

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);

			services
				.AddSingleton<DistrictCacheService>()
				.AddScoped<IErrorReporter, ErrorReporter>(_ => ErrorReporter.Instance)
				.AddScoped<IUserService, UserService>()
				.AddScoped<ICallTaskWorker, CallTaskWorker>()
				.AddScoped<IFastDeliveryAvailabilityHistoryModel, FastDeliveryAvailabilityHistoryModel>()
				.AddInfrastructure()
				.AddHostedServices()
				.AddScoped<ICartItemFactory, CartItemFactory>()
				.AddScoped<IDeliveryPriceGetter<DeliveryRulesRequestDeliveryPriceGetterContext>, DeliveryRulesRequestDeliveryPriceGetter>()
				.AddScoped<CustomerCartWaterCounts>()
				.AddAuthentication("Basic")
				.AddScheme<BasicAuthenticationOptions, CustomAuthenticationHandler>(
					"Basic",
					conf => configuration.GetSection(BasicAuthenticationOptions.Path).Bind(conf))
				;

			return services;
		}

		public static IServiceCollection AddHandlers(this IServiceCollection services)
		{
			var typesToRegister = Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.Name.EndsWith("Handler"));

			return services;
		}

		public static IServiceCollection AddHostedServices(this IServiceCollection services) =>
			services.AddHostedService<DistrictCacheWorker>();
		
		private static IMvcBuilder AddJsonOptions(
			this IMvcBuilder builder,
			string settingsName,
			Action<JsonOptions> configure)
		{
			builder.Services.Configure(settingsName, configure);
			builder.Services.AddSingleton<IConfigureOptions<MvcOptions>>(sp =>
			{
				var options = sp.GetRequiredService<IOptionsMonitor<JsonOptions>>();
				var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
				return new ConfigureMvcJsonOptions(settingsName, options, loggerFactory);
			});
			return builder;
		}
	}
}

using Autofac.Extensions.DependencyInjection;
using CustomerOnlineOrdersRegistrar.Configs;
using CustomerOnlineOrdersRegistrar.V3.Factories;
using CustomerOnlineOrdersRegistrar.V4.Factories;
using CustomerOnlineOrdersRegistrar.V5.Factories;
using CustomerOrdersApi.Library;
using DriverApi.Notifications.Client;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Osrm;
using QS.HistoryLog;
using QS.Project.Core;
using Vodovoz;
using Vodovoz.Core.Application.Logistics;
using Vodovoz.Core.Application.Orders.Services;
using Vodovoz.Core.Application;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Services.Logistics;
using Vodovoz.Trackers;
using VodovozBusiness.Services.Orders;

namespace CustomerOnlineOrdersRegistrar
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureLogging((ctx, builder) => {
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddMappingAssemblies(
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
						.AddOrderTrackerFor1c()
						.AddBusiness(hostContext.Configuration)
						.AddDriverApiNotificationsSenders()
						.AddInfrastructure()
						.AddVersion3()
						.AddVersion4()
						.AddVersion5()
						.AddCoreApplicationOrderServices()
						.AddOsrm()

						.AddScoped<IRouteListService, RouteListService>()
						.AddScoped<IRouteListSpecialConditionsService, RouteListSpecialConditionsService>()
						.AddScoped<IOnlineOrderService, OnlineOrderService>()
						.AddScoped<IOnlineOrderFactoryV3, OnlineOrderFactoryV3>()
						.AddScoped<IOnlineOrderFactoryV4, OnlineOrderFactoryV4>()
						.AddScoped<IOnlineOrderFactoryV5, OnlineOrderFactoryV5>()

						.AddMessageTransportSettings()
						.AddHostedService<OnlineOrderFromTemplateRegistrar>()
						.AddMassTransit(busConf =>
						{
							//busConf.AddConsumer<OnlineOrderRegisteredConsumer, OnlineOrderRegisteredConsumerDefinition>();
							//busConf.AddConsumer<CreatingOnlineOrderConsumer, CreatingOnlineOrderConsumerDefinition>();
							//busConf.AddConsumer<CreatingOnlineOrderWithTemplateConsumer, CreatingOnlineOrderWithTemplateConsumerDefinition>();
							busConf.ConfigureRabbitMq();
						});
					
					services
						.AddDatabaseConfigurationExposer(config =>
						{
							config.LinqToHqlGeneratorsRegistry<LinqToHqlGeneratorsRegistry>();
						});
					
					services.Configure<OnlineOrderFromTemplateRegistrarOptions>(
						hostContext.Configuration.GetSection(OnlineOrderFromTemplateRegistrarOptions.SectionName));

					services.AddStaticScopeForEntity();
					services.AddStaticHistoryTracker();
				});
	}
}

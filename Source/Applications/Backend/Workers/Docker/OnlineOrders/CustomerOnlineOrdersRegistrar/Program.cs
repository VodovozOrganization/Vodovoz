using Autofac.Extensions.DependencyInjection;
using CustomerOnlineOrdersRegistrar.Consumers;
using CustomerOnlineOrdersRegistrar.Factories.V3;
using CustomerOnlineOrdersRegistrar.Factories.V4;
using CustomerOrdersApi.Library;
using DriverApi.Notifications.Client;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Osrm;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.Domain;
using QS.Project.HibernateMapping;
using Vodovoz;
using Vodovoz.Core.Application.Logistics;
using Vodovoz.Core.Application.Orders.Services;
using Vodovoz.Core.Application;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Services.Logistics;
using Vodovoz.Trackers;
using VodovozBusiness.Services.Orders;
using AssemblyFinder = Vodovoz.Data.NHibernate.AssemblyFinder;

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
				.ConfigureLogging((ctx, builder) =>
				{
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddMappingAssemblies(
							typeof(UserBaseMap).Assembly,
							typeof(AssemblyFinder).Assembly,
							typeof(Bank).Assembly,
							typeof(HistoryMain).Assembly,
							typeof(TypeOfEntity).Assembly,
							typeof(Attachment).Assembly,
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
						.AddCoreApplicationOrderServices()
						.AddOsrm()

						.AddScoped<IRouteListService, RouteListService>()
						.AddScoped<IRouteListSpecialConditionsService, RouteListSpecialConditionsService>()
						.AddScoped<IOnlineOrderFactoryV3, OnlineOrderFactoryV3>()
						.AddScoped<IOnlineOrderFactoryV4, OnlineOrderFactoryV4>()

						.AddMessageTransportSettings()
						.AddMassTransit(busConf =>
						{
							busConf.AddConsumer<OnlineOrderRegisteredConsumer, OnlineOrderRegisteredConsumerDefinition>();
							busConf.AddConsumer<CreatingOnlineOrderConsumer, CreatingOnlineOrderConsumerDefinition>();
							busConf.ConfigureRabbitMq();
						})
						;

					services.AddStaticScopeForEntity();
					services.AddStaticHistoryTracker();
				});
	}
}

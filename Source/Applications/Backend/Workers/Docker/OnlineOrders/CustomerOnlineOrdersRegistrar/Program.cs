using Autofac.Extensions.DependencyInjection;
using CustomerOnlineOrdersRegistrar.Consumers;
using CustomerOnlineOrdersRegistrar.Factories;
using CustomerOrdersApi.Library.V4;
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
using Vodovoz.Application;
using Vodovoz.Application.Logistics;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Orders;
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
						.AddBusiness(hostContext.Configuration)
						.AddDriverApiNotificationsSenders()
						.AddInfrastructure()
						.AddDependenciesGroup()
						.AddApplicationOrderServices()
						.AddOsrm()

						.AddScoped<IRouteListService, RouteListService>()
						.AddScoped<IRouteListSpecialConditionsService, RouteListSpecialConditionsService>()
						.AddScoped<IOnlineOrderService, OnlineOrderService>()
						.AddScoped<IOnlineOrderFactory, OnlineOrderFactory>()

						.AddMessageTransportSettings()
						.AddMassTransit(busConf =>
						{
							busConf.AddConsumer<OnlineOrderRegisteredConsumer, OnlineOrderRegisteredConsumerDefinition>();
							busConf.AddConsumer<CreatingOnlineOrderConsumer, CreatingOnlineOrderConsumerDefinition>();
							busConf.ConfigureRabbitMq();
						});

					services.AddStaticScopeForEntity();
					services.AddStaticHistoryTracker();
				});
	}
}

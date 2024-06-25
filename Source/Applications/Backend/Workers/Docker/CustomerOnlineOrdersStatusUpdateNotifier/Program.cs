using System;
using System.Text.Json;
using Autofac.Extensions.DependencyInjection;
using CustomerOnlineOrdersStatusUpdateNotifier.Converters;
using CustomerOnlineOrdersStatusUpdateNotifier.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using Vodovoz;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Infrastructure.Persistance;

namespace CustomerOnlineOrdersStatusUpdateNotifier
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
				.ConfigureServices((hostContext, services) =>
				{
					services.AddLogging(logging =>
						{
							logging.ClearProviders();
							logging.AddNLog();
							logging.AddConfiguration(hostContext.Configuration.GetSection(nameof(NLog)));
						})

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
						.AddBusiness(hostContext.Configuration)
						.AddInfrastructure()
						
						.AddScoped<IOnlineOrderStatusUpdatedNotificationRepository, OnlineOrderStatusUpdatedNotificationRepository>()
						.AddScoped<IExternalOrderStatusConverter, ExternalOrderStatusConverter>()
						.AddSingleton(_ => new JsonSerializerOptions
						{
							PropertyNamingPolicy = JsonNamingPolicy.CamelCase
						})
						.AddHostedService<OnlineOrdersStatusUpdatedNotifier>()
						.AddHttpClient<IOnlineOrdersStatusUpdatedNotificationService, OnlineOrdersStatusUpdatedNotificationService>(client =>
						{
							client.Timeout = TimeSpan.FromSeconds(15);
						});

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
					services.AddStaticHistoryTracker();
				});
	}
}

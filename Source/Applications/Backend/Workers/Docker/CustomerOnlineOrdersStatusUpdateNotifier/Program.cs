﻿using System;
using System.Text.Json;
using Autofac.Extensions.DependencyInjection;
using CustomerOnlineOrdersStatusUpdateNotifier.Converters;
using CustomerOnlineOrdersStatusUpdateNotifier.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using Vodovoz;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Zabbix.Sender;
using DriverApi.Notifications.Client;

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
				.ConfigureLogging((ctx, builder) => {
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.ConfigureServices((hostContext, services) =>
				{
					services
						.ConfigureZabbixSenderFromDataBase(nameof(OnlineOrdersStatusUpdatedNotifier))

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
						.AddDriverApiNotificationsClient()
						.AddInfrastructure()

						.AddScoped<IExternalOrderStatusConverter, ExternalOrderStatusConverter>()
						.AddSingleton(_ => new JsonSerializerOptions
						{
							PropertyNamingPolicy = JsonNamingPolicy.CamelCase
						})

						.AddHostedService<OnlineOrdersStatusUpdatedNotifier>()
						.AddHttpClient<IOnlineOrdersStatusUpdatedNotificationService, OnlineOrdersStatusUpdatedNotificationService>(
							client =>
							{
								client.Timeout = TimeSpan.FromSeconds(15);
							});

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
				});
	}
}

using ExternalCounterpartyAssignNotifier.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NLog.Extensions.Logging;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.DB;
using QS.Project.Domain;
using System;
using System.Reflection;
using System.Text.Json;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Settings.Database;

namespace ExternalCounterpartyAssignNotifier
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.AddLogging(logging =>
					{
						logging.ClearProviders();
						logging.AddNLog();
						logging.AddConfiguration(hostContext.Configuration.GetSection("NLog"));
					})

					.AddMappingAssemblies(
						typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
						typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
						typeof(QS.Banks.Domain.Bank).Assembly,
						typeof(QS.HistoryLog.HistoryMain).Assembly,
						typeof(QS.Project.Domain.TypeOfEntity).Assembly,
						typeof(QS.Attachments.Domain.Attachment).Assembly,
						typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
					)
					.AddDatabaseConnection()
					.AddCore()
					.AddTrackedUoW()

					.AddHostedService<ExternalCounterpartyAssignNotifier>()
					.AddSingleton<IExternalCounterpartyAssignNotificationRepository, ExternalCounterpartyAssignNotificationRepository>()

					.AddSingleton(provider => new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase
					})
					.AddHttpClient<INotificationService, NotificationService>(client =>
					{
						client.Timeout = TimeSpan.FromSeconds(15);
					});

					services.AddStaticHistoryTracker();
				});
		}
	}
}

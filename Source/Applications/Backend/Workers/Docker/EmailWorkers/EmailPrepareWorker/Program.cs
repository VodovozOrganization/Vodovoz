﻿using Autofac.Extensions.DependencyInjection;
using EmailPrepareWorker.Prepares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using RabbitMQ.Client;
using RabbitMQ.Infrastructure;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Common;

namespace EmailPrepareWorker
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
						logging.AddConfiguration(hostContext.Configuration.GetSection("NLog"));
					});

					services.AddTransient<RabbitMQConnectionFactory>();

					services.AddTransient((sp) =>
						sp.GetRequiredService<RabbitMQConnectionFactory>()
							.CreateConnection(sp.GetRequiredService<IConfiguration>()));

					services.AddTransient((sp) =>
					{
						var channel = sp.GetRequiredService<IConnection>().CreateModel();
						channel.BasicQos(0, 1, false);
						return channel;
					});

					services.AddMappingAssemblies(
						typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
						typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
						typeof(QS.Banks.Domain.Bank).Assembly,
						typeof(QS.HistoryLog.HistoryMain).Assembly,
						typeof(QS.Project.Domain.TypeOfEntity).Assembly,
						typeof(QS.Attachments.Domain.Attachment).Assembly,
						typeof(EmployeeWithLoginMap).Assembly,
						typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
					);
					services.AddDatabaseConnection();
					services.AddCore();
					services.AddTrackedUoW();
					services.AddStaticHistoryTracker();
					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);

					services.AddSingleton<ISettingsController, SettingsController>();
					services.AddSingleton<IEmailSettings, EmailSettings>();
					services.AddSingleton<IEmailRepository, EmailRepository>();
					services.AddSingleton<IEmailDocumentPreparer, EmailDocumentPreparer>();
					services.AddSingleton<IEmailSendMessagePreparer, EmailSendMessagePreparer>();

					services.AddHostedService<EmailPrepareWorker>();
				});
	}
}

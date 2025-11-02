using Autofac.Extensions.DependencyInjection;
using EmailPrepareWorker.Prepares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Report;
using RabbitMQ.Client;
using RabbitMQ.Infrastructure;
using Vodovoz.Application.Clients;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Common;
using VodovozBusiness.Controllers;

namespace EmailPrepareWorker
{
	public class Program
	{
		private const string _nLogSectionName = nameof(NLog);

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((hostBuilderContext, loggingBuilder) =>
				{
					loggingBuilder.ClearProviders();
					loggingBuilder.AddNLog();
					loggingBuilder.AddConfiguration(hostBuilderContext.Configuration.GetSection(_nLogSectionName));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
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
						typeof(EmployeeWithLoginMap).Assembly
					);

					services.AddDatabaseConnection();
					services.AddCore()
						.AddInfrastructure();
					services.AddTrackedUoW();
					services.AddStaticHistoryTracker();
					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);

					services.AddScoped<ISettingsController, SettingsController>()
						.AddScoped<IEmailSettings, EmailSettings>()
						.AddScoped<ISettingsController, SettingsController>()
						.AddScoped<IEmailDocumentPreparer, EmailDocumentPreparer>()
						.AddScoped<IReportInfoFactory, DefaultReportInfoFactory>()
						.AddScoped<IEmailSendMessagePreparer, EmailSendMessagePreparer>()
						.AddScoped<ICounterpartyEdoAccountController, CounterpartyEdoAccountController>();

					services.AddHostedService<EmailPrepareWorker>();
				});
	}
}

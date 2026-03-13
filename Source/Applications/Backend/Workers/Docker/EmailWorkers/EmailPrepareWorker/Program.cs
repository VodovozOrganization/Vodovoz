using Autofac.Extensions.DependencyInjection;
using EmailPrepareWorker.Prepares;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.Domain;
using QS.Project.HibernateMapping;
using QS.Report;
using RabbitMQ.MailSending;
using Vodovoz.Application.Clients;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Common;
using Vodovoz.Settings.Database.Organizations;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Controllers;
using AssemblyFinder = Vodovoz.Data.NHibernate.AssemblyFinder;
using DependencyInjection = Vodovoz.Data.NHibernate.DependencyInjection;

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
					services
						.AddMassTransit(busConf =>
						{
							var transportSettings = new ConfigTransportSettings();
							hostContext.Configuration.Bind("MessageBroker", transportSettings);

							busConf.ConfigureRabbitMq((rabbitMq, context) =>
							{
								rabbitMq.AddSendEmailMessageTopology(context);
							},
							transportSettings);
						});

					services.AddMappingAssemblies(
						typeof(UserBaseMap).Assembly,
						typeof(AssemblyFinder).Assembly,
						typeof(Bank).Assembly,
						typeof(HistoryMain).Assembly,
						typeof(TypeOfEntity).Assembly,
						typeof(Attachment).Assembly,
						typeof(EmployeeWithLoginMap).Assembly
					);

					services.AddDatabaseConnection();
					services.AddCore();
					services.AddInfrastructure();
					services.AddTrackedUoW();
					services.AddStaticHistoryTracker();
					DependencyInjection.AddStaticScopeForEntity(services);

					services.AddScoped<ISettingsController, SettingsController>()
						.AddScoped<IEmailSettings, EmailSettings>()
						.AddScoped<ISettingsController, SettingsController>()
						.AddScoped<IEmailDocumentPreparer, EmailDocumentPreparer>()
						.AddScoped<IReportInfoFactory, DefaultReportInfoFactory>()
						.AddScoped<IEmailSendMessagePreparer, EmailSendMessagePreparer>()
						.AddScoped<ICounterpartyEdoAccountController, CounterpartyEdoAccountController>()
						;

					services.AddHostedService<EmailPrepareWorker>();
				});
	}
}

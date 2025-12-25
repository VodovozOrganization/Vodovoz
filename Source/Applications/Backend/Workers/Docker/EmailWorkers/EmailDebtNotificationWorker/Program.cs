using Autofac.Extensions.DependencyInjection;
using EdoService.Library.Services;
using EmailDebtNotificationWorker.Services;
using MassTransit;
using MessageTransport;
using NLog.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Project.Core;
using QS.Report;
using RabbitMQ.MailSending;
using System.Text;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Settings.Database.Common;
using Vodovoz.Settings.Database.Counterparty;
using Vodovoz.Zabbix.Sender;
using AssemblyFinder = Vodovoz.Data.NHibernate.AssemblyFinder;

namespace EmailDebtNotificationWorker
{
	public class Program
	{

		private const string _nLogSectionName = nameof(NLog);

		public static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
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
					Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

					services
						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(AssemblyFinder).Assembly,
							typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly,
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(Vodovoz.Data.NHibernate.HibernateMapping.Counterparty.BulkEmailEventMap).Assembly
						);

					services.AddDatabaseConnection();
					services.AddCore();
					services.AddInfrastructure();
					services.AddRepositories();
					services.AddTrackedUoW();
					services.ConfigureZabbixSenderFromDataBase(nameof(EmailDebtNotificationWorker));
					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);

					services.AddScoped((sp) => sp.GetRequiredService<IUnitOfWorkFactory>()
											   .CreateWithoutRoot("Воркер по рассылке писем о задолженности"));

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

					services.AddScoped<IWorkingDayService, WorkingDayService>();
					services.AddScoped<IDebtorsSettings, DebtorsSettings>();
					services.AddScoped<IEmailSettings, EmailSettings>();
					services.AddScoped<PrintableDocumentSaver>();
					services.AddScoped<IReportInfoFactory, DefaultReportInfoFactory>();
					services.AddScoped<IEmailDebtNotificationService, EmailDebtNotificationService>();
					services.AddHostedService<EmailDebtNotificationWorker>();
				});
	}
}

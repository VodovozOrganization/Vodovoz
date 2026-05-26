using Autofac.Extensions.DependencyInjection;
using BitrixApi.Library.Services;
using EmailDebtNotificationWorker.Options;
using EmailDebtNotificationWorker.Repositories;
using EmailDebtNotificationWorker.Services;
using EmailDebtNotificationWorker.Services.ClosingDeliveries;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using QS.Report;
using RabbitMQ.MailSending;
using System;
using System.Text;
using Vodovoz.Core.Application.Orders.Services;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Settings.Database.Common;
using Vodovoz.Settings.Database.Counterparty;
using Vodovoz.Zabbix.Sender;
using VodovozBusiness.Services.Orders;
using AssemblyFinder = Vodovoz.Data.NHibernate.AssemblyFinder;
using QS.HistoryLog;

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

					services
						.AddDatabaseConfigurationExposer(config =>
						{
							config.LinqToHqlGeneratorsRegistry<LinqToHqlGeneratorsRegistry>();
						});

					services.AddDatabaseConnection();
					services.AddCore();
					services.AddInfrastructure();
					services.AddRepositories();
					services.AddCoreDataRepositories();
					services.AddTrackedUoW();
					services.ConfigureZabbixSenderFromDataBase(nameof(EmailDebtNotificationWorker));
					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
					services.AddStaticHistoryTracker();

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

					services.AddScoped<IDatabaseRepository, DataBaseRepositiry>();

					services.AddScoped<IWorkingDayService, WorkingDayService>();
					services.AddScoped<IDebtorsSettings, DebtorsSettings>();
					services.AddScoped<IEmailSettings, EmailSettings>();
					services.AddScoped<IReportInfoFactory, DefaultReportInfoFactory>();
					services.AddScoped<IEmailAttachmentsCreateService, EmailAttachmentsCreateService>();
					services.AddScoped<IEmailDebtNotificationService, EmailDebtNotificationService>();
					services.AddHostedService<EmailDebtNotificationWorker>();

					services
						.ConfigureOptions<ConfigureEmailClaimLettersOptions>()
						.AddScoped<IClaimLetterBillWithoutShipmentService, ClaimLetterBillWithoutShipmentService>()
						.AddScoped<IEmailClaimLettersService, EmailClaimLettersService>()
						.AddHostedService<EmailClaimLettersWorker>()
						.ConfigureZabbixSenderFromDataBase(nameof(EmailClaimLettersWorker));

					// Пока отключаем до реализации других задач
					//services
					//	.Configure<EmailClosingDeliveriesOptions>(hostContext.Configuration.GetSection(EmailClosingDeliveriesOptions.SectionName))
					//	.AddScoped<IClosingDeliveriesService, ClosingDeliveriesService>()
					//	.AddScoped<IOrderWithoutShipmentForDebtPreparer, OrderWithoutShipmentForDebtPreparer>()
					//	.AddScoped<IClosingDeliveriesNotificationSender, ClosingDeliveriesNotificationSender>()
					//	.AddScoped<IClientClosingDeliveriesEmailPreparer, ClientClosingDeliveriesEmailPreparer>()
					//	.AddScoped<ISummaryClosingDeliveriesEmailPreparer, SummaryClosingDeliveriesEmailPreparer>()
					//	.AddHostedService<EmailClosingDeliveriesWorker>()
					//	.ConfigureZabbixSenderFromDataBase(nameof(EmailClosingDeliveriesWorker))
					//	;
				});
	}
}

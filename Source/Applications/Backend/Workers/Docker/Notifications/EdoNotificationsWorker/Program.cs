using Autofac.Extensions.DependencyInjection;
using EdoNotificationsWorker.Services.Bitrix;
using EdoNotificationsWorker.Services.Email;
using Email.Infrastructure;
using MassTransit;
using MessageTransport;
using MessageTransport.MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using QS.Project.Core;
using RabbitMQ.MailSending;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;

namespace EdoNotificationsWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((ctx, builder) =>
				{
					builder.AddNLog(ctx.Configuration.GetSection("NLog"));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services.AddMappingAssemblies(
						typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
						typeof(QS.Banks.Domain.Bank).Assembly,
						typeof(QS.Project.Domain.TypeOfEntity).Assembly,
						typeof(QS.HistoryLog.HistoryMain).Assembly,
						typeof(EmployeeWithLoginMap).Assembly,
						typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly);

					services.AddDatabaseConnection();
					services.AddCore();
					services.AddNotTrackedUoW();

					services.AddHttpClient();

					services.AddMassTransit(x =>
					{
						x.AddConsumer<EdoNotificationsConsumer, EdoNotificationsConsumerDefinition>();
						x.ConfigureRabbitMq(services, hostContext.Configuration, "NotificationTransportSettings");
					});

					services
						.AddMassTransit<IEmailBus>(busConf =>
						{
							var transportSettings = new ConfigTransportSettings();
							hostContext.Configuration.Bind("EmailTransportSettings", transportSettings);

							busConf.ConfigureRabbitMq((rabbitMq, context) =>
							{
								rabbitMq.AddSendEmailMessageTopology(context);
							},
							transportSettings);
						});

					services.AddEmailInfrastructure();

					services.AddEdoNotificationsSettingsProvider();
					services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
					services.AddScoped<IEdoNotificationBitrixService, EdoNotificationBitrixService>();
					services.AddScoped<IEdoNotificationEmailService, EdoNotificationEmailService>();

					services
						.AddOptions<EdoNotificationsOptions>()
						.Bind(hostContext.Configuration.GetSection(EdoNotificationsOptions.SectionName));
				});
	}
}

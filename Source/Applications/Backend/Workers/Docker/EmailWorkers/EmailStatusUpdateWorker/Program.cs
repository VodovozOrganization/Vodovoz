using Autofac.Extensions.DependencyInjection;
using EmailStatusUpdateWorker.Consumers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using RabbitMQ.MailSending;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using TransactionalOutbox.EntityFrameworkCore;
using TransactionalOutbox.MassTransit;

namespace EmailStatusUpdateWorker
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

					services.AddConfig(hostContext.Configuration)
						.AddMassTransit(busConf =>
						{
							busConf.AddConsumer<EmailStatusUpdateConsumer>();

							// Outbox MassTransit. Этот сервис отправляет
							// и сообщения email_*, записанные другими приложениями
							busConf.AddVodovozOutbox(OutboxBusNames.Email);

							busConf.ConfigureRabbitMq((rabbitMq, context) => rabbitMq.AddUpdateEmailStatusTopology(context));
						});
				});
	}
}

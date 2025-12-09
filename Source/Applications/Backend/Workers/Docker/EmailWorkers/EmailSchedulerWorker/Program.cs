using Autofac.Extensions.DependencyInjection;
using EmailSchedulerWorker.Consumers;
using EmailSchedulerWorker.Services;
using MassTransit;
using MassTransit.Middleware;
using MessageTransport;
using NLog.Extensions.Logging;
using QS.Project.Core;
using System.Text;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;
using static EmailSchedulerWorker.Services.EmailSchedulingService;

namespace EmailSchedulerWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((ctx, builder) => {
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(AssemblyFinder).Assembly,
							typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly
						)
						.AddDatabaseConnection()
						.AddNHibernateConventions()
						.AddCore()
						.AddTrackedUoW()
						.AddMessageTransportSettings()

						.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
						;

					services.AddMassTransit(x =>
					{
						x.AddConsumer<ProcessClientEmailConsumer>()
							.Endpoint(e =>
							{
								e.Name = "process-client-email";
								e.PrefetchCount = 1;
								e.ConcurrentMessageLimit = 1;
							});

						x.UsingRabbitMq((context, cfg) =>
						{
							cfg.Host(hostContext.Configuration["RabbitMQ:Host"], h =>
							{
								h.Username(hostContext.Configuration["RabbitMQ:Username"]);
								h.Password(hostContext.Configuration["RabbitMQ:Password"]);
							});

							cfg.ReceiveEndpoint("process-client-email", e =>
							{
								e.ConfigureConsumer<ProcessClientEmailConsumer>(context);

								e.UseRateLimit(10, TimeSpan.FromMinutes(1));

								e.UseConsumeFilter(typeof(RateLimitFilter<>), context);
							});
						});
					});

					services.AddScoped<IWorkingDayService, WorkingDayService>();
					services.AddScoped<IEmailSchedulingService, EmailSchedulingService>();
					services.AddHostedService<EmailSchedulerWorker>();
				});
	}
}

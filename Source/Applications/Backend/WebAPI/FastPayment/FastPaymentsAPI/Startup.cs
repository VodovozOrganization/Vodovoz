using FastPaymentsAPI.HealthChecks;
using FastPaymentsAPI.Library;
using FastPaymentsAPI.Library.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Services;
using System;
using MassTransit;
using MessageTransport;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using VodovozHealthCheck;
using RabbitMQ.MailSending;
using Vodovoz.Trackers;

namespace FastPaymentsAPI
{
	public class Startup
	{
		private const string _nLogSectionName = "NLog";

		private ILogger<Startup> _logger;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				});

			_logger = new Logger<Startup>(LoggerFactory.Create(logging =>
				logging.AddConfiguration(Configuration.GetSection(_nLogSectionName))));

			services.AddHttpClient()
				.AddControllers()
				.AddXmlSerializerFormatters();

			// Подключение к БД
			services.AddScoped((provider) => provider.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot("Сервис быстрых платежей"));
			
			// Конфигурация Nhibernate
			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(EmployeeWithLoginMap).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddInfrastructure()
				.AddTrackedUoW()
				.AddOrderTrackerFor1c()
				;

			services.AddOpenTelemetry()
				.ConfigureResource(resource => resource.AddService("fastpayment.api"))
				.WithTracing(tracing =>
				{
					tracing
						.AddHttpClientInstrumentation()
						.AddAspNetCoreInstrumentation()
						.AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);

					tracing.AddOtlpExporter();
				});

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
			services.AddStaticHistoryTracker();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1",
					new OpenApiInfo
					{
						Title = "FastPaymentsAPI", Version = "v1"
					});
			});

			services.AddHttpClient<IOrderService, OrderService>(c =>
			{
				c.BaseAddress = new Uri(Configuration.GetSection("OrderService").GetValue<string>("ApiBase"));
				c.DefaultRequestHeaders.Add("Accept", "application/x-www-form-urlencoded");
			});

			services.AddDependencyGroup();

			services
				.AddMassTransit(busConf =>
				{
					var transportSettings = new ConfigTransportSettings();
					Configuration.Bind("MessageBroker", transportSettings);

					busConf.ConfigureRabbitMq((rabbitMq, context) =>
					{
						rabbitMq.AddSendEmailMessageTopology(context);
					},
					transportSettings);
				});

			services.ConfigureHealthCheckService<FastPaymentsHealthCheck, ServiceInfoProvider>();
		}
		
		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<IUserService>();
			app.UseSwagger();

			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FastPaymentsAPI v1"));
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
			});

			app.UseVodovozHealthCheck();
		}
	}
}


using CustomerNotifications.Application.Builders;
using CustomerNotifications.Contracts;
using CustomerNotifications.Transport;
using CustomerOrdersApi.HealthCheck;
using CustomerOrdersApi.Services;
using DriverApi.Notifications.Client;
using MassTransit;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notifications.Infrastructure;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Services;
using System;
using CustomerOrdersApi.Library;
using CustomerOrdersApi.Library.Config;
using Osrm;
using TransactionalOutbox.Abstractions;
using Vodovoz;
using Vodovoz.Core.Application;
using Vodovoz.Core.Application.Logistics;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Data.NHibernate;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Presentation.WebApi;
using Vodovoz.Services.Logistics;
using Vodovoz.Trackers;
using VodovozHealthCheck;

namespace CustomerOrdersApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services
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
				.AddDatabaseConfigurationExposer(config =>
				{
					config.LinqToHqlGeneratorsRegistry<LinqToHqlGeneratorsRegistry>();
				})
				.AddCore()
				.AddTrackedUoW()
				.AddOrderTrackerFor1c()
				.AddBusiness(Configuration)
				.AddDriverApiNotificationsSenders()
				.AddCoreApplicationOrderServices()
				.AddInfrastructure()
				.AddCoreDataRepositories()
				.AddConfig(Configuration)
				.AddVersion3()
				.AddVersion4()
				.AddVersion5()
				.AddVersion6()
				.AddVersioning()
				.AddOsrm()
				.AddSwaggerGen(opt =>
					opt.CustomSchemaIds(type => type.FullName))

				.AddScoped<IRouteListService, RouteListService>()
				.AddScoped<IRouteListSpecialConditionsService, RouteListSpecialConditionsService>()
				.AddPaymentApiClients(Configuration)
				.AddPaymentRefundServices();

			services.AddStaticScopeForEntity();
			services.AddStaticHistoryTracker();

			services
				.AddMemoryCache()
				.AddMessageTransportSettings()
				.AddMassTransit(busConf =>
				{
					busConf.AddRequestClient<Library.V4.Dto.Orders.CreatingOnlineOrder>(
						new Uri($"exchange:{Library.V4.Dto.Orders.CreatingOnlineOrder.ExchangeAndQueueName}"));
					busConf.AddRequestClient<Library.V5.Dto.Orders.CreatingOnlineOrder>(
						new Uri($"exchange:{Library.V5.Dto.Orders.CreatingOnlineOrder.ExchangeAndQueueName}"));
					busConf.AddRequestClient<Library.V6.Dto.Orders.CreatingOnlineOrder>(
						new Uri($"exchange:{Library.V6.Dto.Orders.CreatingOnlineOrder.ExchangeAndQueueName}"));
					busConf.ConfigureRabbitMq();					
				})
				.AddMassTransit<ICustomerNotificationsBus>(busConf =>
				{
					busConf.ConfigureCustomerNotificationsRabbitMq(services, Configuration);
				});

			services
				.AddScoped<IOutboxNotificationPublisher<CustomerNotificationDomainEvent>, OutBoxNotificationPublisher<CustomerNotificationDomainEvent, CustomerNotificationIntegrationEvent>>()
				.AddScoped<IIntegrationEventBuilder<CustomerNotificationDomainEvent, CustomerNotificationIntegrationEvent>, CustomerNotificationsIntegrationEventBuilder>()
				.AddCustomerNotificationsSettingsProvider();

			services
				.Configure<CourierCoordinatesOptions>(
					Configuration.GetSection(nameof(CourierCoordinatesOptions)));

			services.AddAuthentication("Basic")
				.AddScheme<SignatureOptions, CustomAuthenticationHandler>(
				"Basic",
				conf => Configuration.GetSection(SignatureOptions.Path).Bind(conf));

			services.ConfigureHealthCheckService<CustomerOrdersApiHealthCheck, ServiceInfoProvider>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<IUserService>();

			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(options =>
				{
					var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

					foreach(var description in provider.ApiVersionDescriptions)
					{
						options.SwaggerEndpoint(
							$"/swagger/{description.GroupName}/swagger.json",
							description.ApiVersion.ToString());
					}
				});
			}

			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseApiVersioning();
			app.UseVodovozHealthCheck();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}

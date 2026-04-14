using CustomerOrdersApi.HealthCheck;
using CustomerOrdersApi.Library;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using DriverApi.Notifications.Client;
using MassTransit;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Services;
using System;
using Osrm;
using Vodovoz;
using Vodovoz.Core.Application;
using Vodovoz.Core.Application.Logistics;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Presentation.WebApi;
using Vodovoz.Services.Logistics;
using Vodovoz.Trackers;
using VodovozHealthCheck;
using CustomerNotifications.Transport;

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
				.AddCore()
				.AddTrackedUoW()
				.AddOrderTrackerFor1c()
				.AddBusiness(Configuration)
				.AddDriverApiNotificationsSenders()
				.AddCoreApplicationOrderServices()
				.AddInfrastructure()
				.AddConfig(Configuration)
				.AddVersion3()
				.AddVersion4()
				.AddVersioning()
				.AddOsrm()

				.AddScoped<IRouteListService, RouteListService>()
				.AddScoped<IRouteListSpecialConditionsService, RouteListSpecialConditionsService>()
				;

			services.AddStaticScopeForEntity();
			services.AddStaticHistoryTracker();

			services
				.AddMemoryCache()
				.AddMessageTransportSettings()
				.AddMassTransit(busConf =>
				{
					busConf.AddRequestClient<CreatedOnlineOrder>(new Uri($"exchange:{CreatingOnlineOrder.ExchangeAndQueueName}"));
					busConf.ConfigureRabbitMq();
					busConf.ConfigureCustomerNotificationsRabbitMq(services, Configuration);
				})
				.AddHttpClient();

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
			app.UseAuthorization();
			app.UseApiVersioning();
			app.UseVodovozHealthCheck();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}

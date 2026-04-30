using CustomerOrdersApi.HealthCheck;
using System;
using CustomerAppsApi.Services;
using CustomerOrdersApi.Configs;
using CustomerOrdersApi.Library;
using CustomerOrdersApi.Library.Config;
using DriverApi.Notifications.Client;
using MassTransit;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Osrm;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Services;
using Vodovoz;
using Vodovoz.Core.Application.Logistics;
using Vodovoz.Core.Application.Orders.Services;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Trackers;
using VodovozHealthCheck;
using Vodovoz.Presentation.WebApi;
using Vodovoz.Services.Logistics;
using VodovozBusiness.Services.Orders;
using Vodovoz.Core.Application;

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
				.AddVersion5()
				.AddVersioning()
				.AddOsrm()

				.AddScoped<IRouteListService, RouteListService>()
				.AddScoped<IRouteListSpecialConditionsService, RouteListSpecialConditionsService>()
				.AddScoped<IOnlineOrderService, OnlineOrderService>();

			services.AddStaticScopeForEntity();
			services.AddStaticHistoryTracker();
			
			services.AddAuthentication("Basic")
				.AddScheme<BasicAuthenticationOptions, CustomAuthenticationHandler>(
					"Basic",
					conf => Configuration.GetSection(SignatureOptions.Path).Bind(conf));

			services
				.AddMemoryCache()
				.AddMessageTransportSettings()
				.AddMassTransit(busConf =>
				{
					busConf.AddRequestClient<CustomerOrders.Contracts.V4.Orders.CreatedOnlineOrder>(
						new Uri($"exchange:{CustomerOrders.Contracts.V4.Orders.CreatingOnlineOrder.ExchangeAndQueueName}"));
					
					busConf.AddRequestClient<CustomerOrders.Contracts.V5.Orders.CreatedOnlineOrder>(
						new Uri($"exchange:{CustomerOrders.Contracts.V5.Orders.CreatingOnlineOrder.ExchangeAndQueueName}"));
					
					busConf.ConfigureRabbitMq();
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
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseApiVersioning();
			app.UseVodovozHealthCheck();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}

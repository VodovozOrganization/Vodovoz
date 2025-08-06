using System;
using CustomerOrdersApi.Library.V4;
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
using Microsoft.OpenApi.Models;
using QS.Project.Core;
using QS.Services;
using Vodovoz;
using Vodovoz.Application;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Presentation.WebApi;

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
			services.AddSwaggerGen(c =>
				{
					c.SwaggerDoc("v1", new OpenApiInfo
					{
						Title = "CustomerOrdersApi",
						Version = "v1"
					});
				})

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
				.AddBusiness(Configuration)
				.AddDriverApiNotificationsSenders()
				.AddApplicationOrderServices()
				.AddInfrastructure()
				.AddConfig(Configuration)
				.AddDependenciesGroup()
				.AddVersioning();

			services.AddStaticScopeForEntity();

			services
				.AddMemoryCache()
				.AddMessageTransportSettings()
				.AddMassTransit(busConf =>
				{
					busConf.AddRequestClient<CreatedOnlineOrder>(new Uri($"exchange:{OnlineOrderInfoDto.ExchangeName}"));
					busConf.ConfigureRabbitMq();
				})
				.AddHttpClient();
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
			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}

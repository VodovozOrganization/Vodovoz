using CustomerAppsApi.Configs;
using CustomerAppsApi.Library;
using CustomerAppsApi.Middleware;
using CustomerAppsApi.Services;
using MassTransit;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Osrm;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Services;
using RabbitMQ.MailSending;
using Vodovoz;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Presentation.WebApi;
using Vodovoz.Settings;
using VodovozHealthCheck;

namespace CustomerAppsApi
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
			services
				.AddSwaggerGen(c =>
				{
					c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomerAppsApi", Version = "v1" });
				})

				.AddLogging(logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				})

				.AddMemoryCache()

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
				.AddBusiness(Configuration)
				.AddTrackedUoW()
				.AddInfrastructure()
				.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot())
				.ConfigureHealthCheckService<V1.HealthChecks.CustomerAppsApiHealthCheck, V1.HealthChecks.ServiceInfoProvider>()
				.AddHttpClient()
				.AddVersion1()
				.AddVersion2()
				.AddVersioning()
				.AddSwaggerGen(opt =>
					opt.CustomSchemaIds(type => type.FullName))
				.AddOsrm()
				.AddRabbitConfig(Configuration)
				.AddMessageTransportSettings()
				.AddMassTransit(busConf =>
				{
					busConf.ConfigureRabbitMq((rabbitMq, context) => rabbitMq.AddSendAuthorizationCodesByEmailTopology(context));
				})
				.AddControllers()
				;
			
			services.AddAuthentication("Basic")
				.AddScheme<BasicAuthenticationOptions, CustomAuthenticationHandler>(
					"Basic",
					conf => Configuration.GetSection(BasicAuthenticationOptions.Path).Bind(conf));

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);

			services.AddStaticHistoryTracker();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<IUserService>();
			app.ApplicationServices.GetService<ISettingsController>().RefreshSettings();
			
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

			app.UseMiddleware<ResponseLoggingMiddleware>();
			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseAuthorization();
			app.UseApiVersioning();
			app.UseVodovozHealthCheck();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}

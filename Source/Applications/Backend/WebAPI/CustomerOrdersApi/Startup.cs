using CustomerOrdersApi.Library;
using MassTransit;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using QS.Project.Core;
using QS.Services;
using Vodovoz;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;

namespace CustomerOrdersApi
{
	public class Startup
	{
		private const string _nLogSectionName = nameof(NLog);
		
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomerOrdersApi", Version = "v1" }); });

			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection(_nLogSectionName));
				})
				
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(EmployeeWithLoginMap).Assembly,
					typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()
				.AddBusiness()
				.AddConfig(Configuration)
				.AddDependenciesGroup()
				.AddStaticScopeForEntity()
					
				//.AddMessageTransportSettings()
				//.AddMassTransit(busConf => busConf.ConfigureRabbitMq())
				.AddMemoryCache();

				/*configurator.ReceiveEndpoint("online-orders", x =>
				{
					x.ConfigureConsumeTopology = false;

					x.Bind<OnlineOrderInfoDto>(s =>
					{
						s.RoutingKey = "False";
						s.ExchangeType = ExchangeType.Direct;
					});
				});

				configurator.ReceiveEndpoint("online-orders-fault", x =>
				{
					x.ConfigureConsumeTopology = false;

					x.Bind<OnlineOrderInfoDto>(s =>
					{
						s.RoutingKey = "True";
						s.ExchangeType = ExchangeType.Direct;
					});
				});*/
				
			services.AddHttpClient();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<IUserService>();
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomerOrdersApi v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}

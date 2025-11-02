using Edo.Receipt.Callback;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ModulKassa;
using NLog.Web;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;

namespace Edo.Receipt.Api
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
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				});

			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(Vodovoz.Core.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly
				)
				.AddDatabaseConnection()
				.AddNHibernateConventions()
				.AddCoreDataRepositories()
				.AddCore()
				.AddTrackedUoW()
				.AddMessageTransportSettings()
				;

			services.AddReceiptCallback();

			services.Configure<CashboxesSetting>(Configuration);

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Edo.Receipt.Api", Version = "v1" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Edo.Receipt.Api v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}

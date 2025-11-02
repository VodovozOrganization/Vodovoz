using ApiAuthentication;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pacs.Operators.Server;
using QS.BusinessCommon.HMap;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Services;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Pacs;

namespace Pacs.Operator.Service
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
			var transportSettings = new ConfigTransportSettings();
			Configuration.Bind("MessageTransport", transportSettings);

			services
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()

				//Настройки бд должны регистрироваться до настроек MassTransit
				.AddDatabaseSingletonSettings()

				.AddSingleton<IMessageTransportSettings>(transportSettings)
				.AddPacsOperatorServer()

				.AddApiKeyAuthentication()

				.AddMappingAssemblies(
					typeof(QS.Banks.Domain.Account).Assembly,
					typeof(MeasurementUnitsMap).Assembly)
				;

			services.AddStaticHistoryTracker();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<IUserService>();
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}

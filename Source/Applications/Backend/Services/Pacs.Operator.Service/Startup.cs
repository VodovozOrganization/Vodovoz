using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pacs.Operators.Server;
using QS.HistoryLog;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Pacs;

namespace Pacs.Operators.Service
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
				.AddMappingAssemblies(
					typeof(Vodovoz.Core.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(Vodovoz.Settings.Database.SettingMap).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()

				//Настройки бд должны регистрироваться до настроек MassTransit
				.AddDatabaseSingletonSettings()

				.AddSingleton<IMessageTransportSettings>(transportSettings)
				.AddPacsOperatorServer()
				;

			services.AddStaticHistoryTracker();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}

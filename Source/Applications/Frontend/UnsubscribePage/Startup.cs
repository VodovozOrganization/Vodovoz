using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.DB;
using System;
using System.Reflection;
using UnsubscribePage.Controllers;
using UnsubscribePage.HealthChecks;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;
using VodovozHealthCheck;

namespace UnsubscribePage
{
	public class Startup
	{
		private ILogger<Startup> _logger;
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			_logger = new Logger<Startup>(LoggerFactory.Create(logging =>
				logging.AddNLogWeb(NLogBuilder.ConfigureNLog("NLog.config").Configuration)));

			services.AddControllersWithViews();

			services
				.AddCore()
				.AddTrackedUoW()
				.ConfigureHealthCheckService<UnsubscribePageHealthCheck>()
				;

			// Конфигурация Nhibernate
			try
			{
				CreateBaseConfig(services);
			}
			catch(Exception e)
			{
				_logger.LogCritical(e, e.Message);
				throw;
			}
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			builder.RegisterModule<DatabaseSettingsModule>();

			builder.RegisterType<UnsubscribeViewModelFactory>()
				.As<IUnsubscribeViewModelFactory>()
				.SingleInstance();

			builder.RegisterType<EmailRepository>()
				.As<IEmailRepository>()
				.SingleInstance();

			builder.RegisterType<EmailParametersProvider>()
				.As<IEmailParametersProvider>()
				.SingleInstance();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Unsubscribe}/{action=Index}/{id?}");
			});

			app.ConfigureHealthCheckApplicationBuilder();
		}

		private void CreateBaseConfig(IServiceCollection services)
		{
			_logger.LogInformation("Настройка параметров Nhibernate...");

			var conStrBuilder = new MySqlConnectionStringBuilder();

			var domainDBConfig = Configuration.GetSection("DomainDB");

			conStrBuilder.Server = domainDBConfig.GetValue<string>("Server");
			conStrBuilder.Port = domainDBConfig.GetValue<uint>("Port");
			conStrBuilder.Database = domainDBConfig.GetValue<string>("Database");
			conStrBuilder.UserID = domainDBConfig.GetValue<string>("UserID");
			conStrBuilder.Password = domainDBConfig.GetValue<string>("Password");
			conStrBuilder.SslMode = MySqlSslMode.None;

			var connectionString = conStrBuilder.GetConnectionString(true);

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString)
				.AdoNetBatchSize(100)
				.Driver<LoggedMySqlClientDriver>();

			var provider = services.BuildServiceProvider();
			var ormConfig = provider.GetRequiredService<IOrmConfig>();
			ormConfig.ConfigureOrm(
				db_config,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.TypeOfEntityMap)),
					Assembly.GetAssembly(typeof(Vodovoz.Data.NHibernate.AssemblyFinder)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				}
			);

			HistoryMain.Enable(conStrBuilder);
		}
	}
}

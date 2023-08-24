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
using QS.HistoryLog;
using QS.Project.DB;
using System;
using System.Reflection;
using QS.DomainModel.UoW;
using Vodovoz;
using Vodovoz.Settings.Database;
using Vodovoz.Data.NHibernate.NhibernateExtensions;

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

			// Конфигурация Nhibernate
			try
			{
				CreateBaseConfig();
			}
			catch(Exception e)
			{
				_logger.LogCritical(e, e.Message);
				throw;
			}

			services.AddControllersWithViews();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			builder.RegisterModule<DatabaseSettingsModule>();
			
			builder.RegisterType<DefaultUnitOfWorkFactory>()
				.As<IUnitOfWorkFactory>()
				.SingleInstance();
			
			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly)
				.Where(t => t.Name.EndsWith("Provider"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly)
				.Where(t => t.Name.EndsWith("Repository"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly, Assembly.GetExecutingAssembly())
				.Where(t => t.Name.EndsWith("Factory"))
				.AsSelf()
				.AsImplementedInterfaces();
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
		}

		private void CreateBaseConfig()
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

			// Настройка ORM
			OrmConfig.ConfigureOrm(
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

using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using System.Linq;
using System.Reflection;
using TrueMarkApi.Library;
using Vodovoz;
using Vodovoz.Core.DataService;
using Vodovoz.Models.TrueMark;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Edo;
using Vodovoz.Tools;

namespace TrueMarkCodesWorker
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
			NLogBuilder.ConfigureNLog("NLog.config");
			CreateBaseConfig();
		}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterModule<DatabaseSettingsModule>();

			builder.RegisterType<DefaultSessionProvider>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<DefaultUnitOfWorkFactory>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<BaseParametersProvider>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<EdoSettings>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<TrueMarkCodesPool>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<TrueMarkApiClientFactory>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<TrueMarkCodesHandler>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<TrueMarkSelfDeliveriesHandler>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<TrueMarkTransactionalCodesPool>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<TrueMarkWaterCodeParser>().AsSelf().AsImplementedInterfaces();

			builder.RegisterInstance(ErrorReporter.Instance).AsImplementedInterfaces();

			var vodovozBusinessAssembly = typeof(VodovozBusinessAssemblyFinder).Assembly;

			var sdsf = new QS.Osrm.Route();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly)
				.Where(t => t.Name.EndsWith("Provider"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly)
				.Where(t => t.Name.EndsWith("Model"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly)
				.Where(t => t.Name.EndsWith("Repository"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly, Assembly.GetExecutingAssembly())
				.Where(t => t.Name.EndsWith("Factory"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly, Assembly.GetExecutingAssembly())
				.Where(t => t.Name.EndsWith("Controller"))
				.AsSelf()
				.AsImplementedInterfaces();
		}

		private void CreateBaseConfig()
		{
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
				.Driver<LoggedMySqlClientDriver>()
				;

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				db_config,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.Organizations.OrganizationMap)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				}
			);
		}
	}
}

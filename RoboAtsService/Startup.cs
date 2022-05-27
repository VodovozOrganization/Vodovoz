using Autofac;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Services;
using RoboAtsService.Middleware;
using RoboAtsService.Monitoring;
using System.Linq;
using System.Reflection;
using Vodovoz;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Parameters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;

namespace RoboAtsService
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

			services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
            services.AddMvc().AddControllersAsServices();

			NLogBuilder.ConfigureNLog("NLog.config");

			CreateBaseConfig();

			//services.AddControllers();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterType<DefaultSessionProvider>().AsImplementedInterfaces();
			builder.RegisterType<DefaultUnitOfWorkFactory>().AsImplementedInterfaces();
			builder.RegisterType<BaseParametersProvider>().AsImplementedInterfaces();
			builder.RegisterType<RoboatsRepository>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<RoboatsSettings>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<RoboatsCallRegistrator>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<CallTaskWorker>()
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterType<UserService>()
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterType<UserService>()
				.AsSelf()
				.AsImplementedInterfaces();

			/*builder.RegisterType<RoboatsOrderModel>().AsSelf();
			builder.RegisterType<EmployeeRepository>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<PaymentFromBankClientController>().AsSelf().AsImplementedInterfaces();*/


			builder.RegisterInstance(ErrorReporter.Instance).AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
				.Where(t => t.Name.EndsWith("Handler"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly)
				.Where(t => t.Name.EndsWith("Provider"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly)
				.Where(t => t.Name.EndsWith("Model"))
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

			builder.RegisterAssemblyTypes(typeof(VodovozBusinessAssemblyFinder).Assembly)
				.Where(t => t.Name.EndsWith("Controller"))
				.AsSelf()
				.AsImplementedInterfaces();

			/*var instance = CallTaskSingletonFactory.GetInstance();
			builder.RegisterInstance(instance).As<ICallTaskFactory>().SingleInstance();*/
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
			app.UseMiddleware<ApiKeyMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

		private void CreateBaseConfig()
		{
			//_logger.LogInformation("Настройка параметров Nhibernate...");

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
				new System.Reflection.Assembly[]
				{
					System.Reflection.Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					System.Reflection.Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.OrganizationMap)),
					System.Reflection.Assembly.GetAssembly(typeof(Bank)),
					System.Reflection.Assembly.GetAssembly(typeof(HistoryMain)),
					System.Reflection.Assembly.GetAssembly(typeof(TypeOfEntity)),
					System.Reflection.Assembly.GetAssembly(typeof(Attachment))
				}
			);

			var serviceUserId = 0;

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Получение пользователя"))
			{
				serviceUserId = unitOfWork.Session.Query<Vodovoz.Domain.Employees.User>()
					.Where(u => u.Login == domainDBConfig.GetValue<string>("UserID"))
					.Select(u => u.Id)
					.FirstOrDefault();
			}

			QS.Project.Repositories.UserRepository.GetCurrentUserId = () => serviceUserId;

			HistoryMain.Enable();
		}
	}
}

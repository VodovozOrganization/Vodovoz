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
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Project.Services;
using QS.Services;
using RoboatsService.Authentication;
using RoboatsService.Monitoring;
using RoboatsService.OrderValidation;
using Sms.External.SmsRu;
using Sms.Internal.Client.Framework;
using System;
using System.Linq;
using System.Reflection;
using Vodovoz;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Factories;
using Vodovoz.Infrastructure.Database;
using Vodovoz.Models;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;

namespace RoboatsService
{
	public class Startup
    {
		private IDataBaseInfo _dataBaseInfo;
		private ILogger<Startup> _logger;

        public Startup(IConfiguration configuration)
		{
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
			services.AddAuthentication()
				.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, null);
			services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme);
			services.AddMvc().AddControllersAsServices();

			NLogBuilder.ConfigureNLog("NLog.config");

			CreateBaseConfig();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;


			builder.RegisterType<DefaultSessionProvider>().AsImplementedInterfaces();
			builder.RegisterType<DefaultUnitOfWorkFactory>().AsImplementedInterfaces();
			builder.RegisterType<BaseParametersProvider>().AsImplementedInterfaces();
			builder.RegisterType<RoboatsCallFactory>().AsImplementedInterfaces();
			builder.RegisterType<RoboatsRepository>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<RoboatsSettings>().As<IRoboatsSettings>();
			builder.RegisterType<RoboatsCallBatchRegistrator>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<RoboatsCallRegistrator>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<ValidOrdersProvider>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<ApiKeyAuthenticationOptions>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<ApiKeyAuthenticationHandler>().AsSelf().AsImplementedInterfaces();
			builder.RegisterInstance(ServicesConfig.UserService).As<IUserService>();
			
			builder.RegisterType<FastPaymentSender>().AsSelf().AsImplementedInterfaces();

			builder.RegisterInstance(_dataBaseInfo)
				.AsSelf()
				.AsImplementedInterfaces()
				.SingleInstance();
			
			builder.RegisterModule<DatabaseSettingsModule>();
			builder.RegisterModule<SmsExternalSmsRuModule>();
			builder.RegisterModule<SmsInternalClientModule>();
			
			builder.RegisterType<CallTaskWorker>()
				.AsSelf()
				.AsImplementedInterfaces();

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
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
			app.ApplicationServices.GetService<SettingsController>().RefreshSettings();

			if(env.IsDevelopment()) {
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
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				}
			);

			_dataBaseInfo = new DatabaseInfo(conStrBuilder.Database);

			string userLogin = domainDBConfig.GetValue<string>("UserID");
			int serviceUserId = 0;

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Получение пользователя"))
			{
				var serviceUser = unitOfWork.Session.Query<Vodovoz.Domain.Employees.User>()
					.Where(u => u.Login == userLogin)
					.FirstOrDefault();

				serviceUserId = serviceUser.Id;

				ServicesConfig.UserService = new UserService(serviceUser);
			}

			if(serviceUserId == 0)
			{
				throw new ApplicationException($"Невозможно получить пользователя по логину: {userLogin}");
			}

			UserRepository.GetCurrentUserId = () => serviceUserId;

			HistoryMain.Enable(conStrBuilder);
		}
	}
}

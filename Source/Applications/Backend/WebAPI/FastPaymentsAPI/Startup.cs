using System;
using System.Linq;
using System.Reflection;
using FastPaymentsAPI.HealthChecks;
using FastPaymentsAPI.Library.ApiClients;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Managers;
using FastPaymentsAPI.Library.Models;
using FastPaymentsAPI.Library.Notifications;
using FastPaymentsAPI.Library.Services;
using FastPaymentsAPI.Library.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Core.DataService;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.FastPayments;
using VodovozInfrastructure.Cryptography;
using VodovozHealthCheck;

namespace FastPaymentsAPI
{
	public class Startup
	{
		private const string _nLogSectionName = "NLog";

		private ILogger<Startup> _logger;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			var nlogConfig = Configuration.GetSection(_nLogSectionName);
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(nlogConfig);
				});

			services.AddHttpClient()
				.AddControllers()
				.AddXmlSerializerFormatters();

			// Подключение к БД
			services.AddScoped(_ => UnitOfWorkFactory.CreateWithoutRoot("Сервис быстрых платежей"));

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

			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "FastPaymentsAPI", Version = "v1" }); });

			services.AddHttpClient<IOrderService, OrderService>(c =>
			{
				c.BaseAddress = new Uri(Configuration.GetSection("OrderService").GetValue<string>("ApiBase"));
				c.DefaultRequestHeaders.Add("Accept", "application/x-www-form-urlencoded");
			});

			services.AddHttpClient<IDriverAPIService, DriverAPIService>(c =>
			{
				c.BaseAddress = new Uri(Configuration.GetSection("DriverAPIService").GetValue<string>("ApiBase"));
				c.DefaultRequestHeaders.Add("Accept", "application/json");
			});

			services.AddScoped<ISiteSettings, SiteSettings>();
			services.AddScoped<SiteClient>();
			services.AddScoped<MobileAppClient>();
			services.AddScoped<SiteNotifier>();
			services.AddScoped<MobileAppNotifier>();
			services.AddScoped<NotificationModel>();
			

			// Unit Of Work
			services.AddScoped<IUnitOfWorkFactory>((sp) => UnitOfWorkFactory.GetDefaultFactory);

			//backgroundServices
			services.AddHostedService<FastPaymentStatusUpdater>();
			services.AddHostedService<CachePaymentManager>();

			//repositories
			services.AddSingleton<IOrderRepository, OrderRepository>();
			services.AddSingleton<IOrganizationRepository, OrganizationRepository>();
			services.AddSingleton<IFastPaymentRepository, FastPaymentRepository>();
			services.AddSingleton<IStandartNomenclatures, BaseParametersProvider>();
			services.AddSingleton<IRouteListItemRepository, RouteListItemRepository>();
			services.AddSingleton<ISelfDeliveryRepository, SelfDeliveryRepository>();
			services.AddSingleton<ICashRepository, CashRepository>();

			//providers
			services.AddSingleton<IParametersProvider, ParametersProvider>();
			services.AddScoped<ISettingsController, SettingsController>();
			services.AddSingleton<IOrderParametersProvider, OrderParametersProvider>();
			services.AddSingleton<IFastPaymentParametersProvider, FastPaymentParametersProvider>();
			services.AddSingleton<IOrganizationParametersProvider, OrganizationParametersProvider>();
			services.AddScoped<IEmailParametersProvider, EmailParametersProvider>();

			//factories
			services.AddSingleton<IFastPaymentFactory, FastPaymentFactory>();

			//converters
			services.AddSingleton<IOrderSumConverter, OrderSumConverter>();
			services.AddSingleton<IResponseCodeConverter, ResponseCodeConverter>();
			services.AddSingleton<IRequestFromConverter, RequestFromConverter>();

			//models
			services.AddScoped<IFastPaymentOrderModel, FastPaymentOrderModel>();
			services.AddScoped<IFastPaymentModel, FastPaymentModel>();

			//validators
			services.AddScoped<IFastPaymentValidator, FastPaymentValidator>();

			//helpers
			services.AddSingleton<IDTOManager, DTOManager>();
			services.AddScoped<ISignatureManager, SignatureManager>();
			services.AddScoped<IMD5HexHashFromString, MD5HexHashFromString>();
			services.AddSingleton<IFastPaymentManager, FastPaymentManager>();
			services.AddSingleton<IErrorHandler, ErrorHandler>();
			services.AddSingleton(_ => new FastPaymentFileCache("/tmp/VodovozFastPaymentServiceTemp.txt"));
			services.AddScoped<IOrderRequestManager, OrderRequestManager>();

			services.ConfigureHealthCheckService<FastPaymentsHealthCheck>();
		}
		
		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseSwagger();

			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FastPaymentsAPI v1"));
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
			});

			app.ConfigureHealthCheckApplicationBuilder();
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

			var serviceUserId = 0;

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Получение пользователя"))
			{
				var serviceUser = unitOfWork.Session.Query<Vodovoz.Domain.Employees.User>()
					.Where(u => u.Login == domainDBConfig.GetValue<string>("UserID"))
					.FirstOrDefault();

				serviceUserId = serviceUser.Id;

				ServicesConfig.UserService = new UserService(serviceUser);
			}

			QS.Project.Repositories.UserRepository.GetCurrentUserId = () => serviceUserId;
			HistoryMain.Enable(conStrBuilder);
		}
	}
}


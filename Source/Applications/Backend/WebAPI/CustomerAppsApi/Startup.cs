using CustomerAppsApi.Middleware;
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
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Numeric;
using System.Linq;
using System.Reflection;
using CustomerAppsApi.HealthChecks;
using CustomerAppsApi.Library;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;
using VodovozHealthCheck;
using UserRepository = QS.Project.Repositories.UserRepository;

namespace CustomerAppsApi
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
			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomerAppsApi", Version = "v1" }); });
			
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection(_nLogSectionName));
				});

			RegisterDependencies(services);

			services.ConfigureHealthCheckService<CustomerAppsApiHealthCheck>();
			services.AddHttpClient();

			CreateBaseConfig();
		}

		private void RegisterDependencies(IServiceCollection services)
		{
			services.AddStackExchangeRedisCache(redisOptions =>
			{
				var connection = Configuration.GetConnectionString("Redis");
				redisOptions.Configuration = connection;
			});
			
			services.AddScoped<IUnitOfWork>(_ => UnitOfWorkFactory.CreateWithoutRoot("Сервис интеграции"));
			
			services.AddSingleton<IPhoneRepository, PhoneRepository>();
			services.AddSingleton<IEmailRepository, EmailRepository>();
			services.AddSingleton<IWarehouseRepository, WarehouseRepository>();
			services.AddSingleton<IRoboatsRepository, RoboatsRepository>();
			services.AddSingleton<IBottlesRepository, BottlesRepository>();
			services.AddSingleton<INomenclatureRepository, NomenclatureRepository>();
			services.AddSingleton<IOrderRepository, OrderRepository>();
			services.AddSingleton<IStockRepository, StockRepository>();
			services.AddSingleton<IPromotionalSetRepository, PromotionalSetRepository>();
			services.AddSingleton<IExternalCounterpartyRepository, ExternalCounterpartyRepository>();
			services.AddSingleton<IExternalCounterpartyMatchingRepository, ExternalCounterpartyMatchingRepository>();
			
			services.AddSingleton<PhoneFormatter>(_ => new PhoneFormatter(PhoneFormat.DigitsTen));
			services.AddSingleton<ICounterpartySettings, CounterpartySettings>();
			
			services.AddCustomerApiLibrary();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomerAppsApi v1"));
			}

			app.UseMiddleware<ResponseLoggingMiddleware>();
			app.UseHttpsRedirection();
			app.UseRouting();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

			app.ConfigureHealthCheckApplicationBuilder();
		}
		
		private void CreateBaseConfig()
		{
			var conStrBuilder = new MySqlConnectionStringBuilder();

			var domainDbConfig = Configuration.GetSection("DomainDB");

			conStrBuilder.Server = domainDbConfig.GetValue<string>("Server");
			conStrBuilder.Port = domainDbConfig.GetValue<uint>("Port");
			conStrBuilder.Database = domainDbConfig.GetValue<string>("Database");
			conStrBuilder.UserID = domainDbConfig.GetValue<string>("UserID");
			conStrBuilder.Password = domainDbConfig.GetValue<string>("Password");
			conStrBuilder.SslMode = MySqlSslMode.None;

			var connectionString = conStrBuilder.GetConnectionString(true);

			var dbConfig = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString)
				.Driver<LoggedMySqlClientDriver>()
				.AdoNetBatchSize(100);

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				dbConfig,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.Data.NHibernate.AssemblyFinder)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				}
			);

			string userLogin = domainDbConfig.GetValue<string>("UserID");
			int serviceUserId = 0;

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Получение пользователя"))
			{
				var serviceUser = unitOfWork.Session.Query<Vodovoz.Domain.Employees.User>()
					.Where(u => u.Login == userLogin)
					.FirstOrDefault();

				serviceUserId = serviceUser.Id;

				ServicesConfig.UserService = new UserService(serviceUser);
			}

			UserRepository.GetCurrentUserId = () => serviceUserId;
			HistoryMain.Enable(conStrBuilder);
		}
	}
}

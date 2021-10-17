﻿using DriverAPI.Data;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using DriverAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using NLog.Web;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using System;
using System.Linq;
using System.Text;
using QS.Attachments.Domain;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Tools;

namespace DriverAPI
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
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
				});

			_logger = new Logger<Startup>(LoggerFactory.Create(logging => 
				logging.AddNLogWeb(NLogBuilder.ConfigureNLog("NLog.config").Configuration)));

			// Подключение к БД

			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseMySQL(Configuration.GetConnectionString("DefaultConnection")));
			services.AddDatabaseDeveloperPageExceptionFilter();

			// Конфигурация Nhibernate

			try
			{
				CreateBaseConfig();
			}
			catch (Exception e)
			{
				_logger.LogCritical(e, e.Message);
				throw;
			}

			RegisterDependencies(ref services);

			// Аутентификация
			services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			services.Configure<IdentityOptions>(options =>
			{
				// Password settings
				options.Password.RequireDigit =				Configuration.GetValue<bool>("Security:Password:RequireDigit");
				options.Password.RequireLowercase =			Configuration.GetValue<bool>("Security:Password:RequireLowercase");
				options.Password.RequireNonAlphanumeric =	Configuration.GetValue<bool>("Security:Password:RequireNonAlphanumeric");
				options.Password.RequireUppercase =			Configuration.GetValue<bool>("Security:Password:RequireUppercase");
				options.Password.RequiredLength =			Configuration.GetValue<int>("Security:Password:RequiredLength");
				options.Password.RequiredUniqueChars =		Configuration.GetValue<int>("Security:Password:RequiredUniqueChars");

				// Lockout settings.
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
															Configuration.GetValue<int>("Security:Lockout:DefaultLockoutTimeSpan"));
				options.Lockout.MaxFailedAccessAttempts =	Configuration.GetValue<int>("Security:Lockout:MaxFailedAccessAttempts");
				options.Lockout.AllowedForNewUsers =		Configuration.GetValue<bool>("Security:Password:AllowedForNewUsers");

				// User settings.
				options.User.AllowedUserNameCharacters =	Configuration.GetValue<string>("Security:Password:AllowedUserNameCharacters");
				options.User.RequireUniqueEmail =			Configuration.GetValue<bool>("Security:Password:RequireNonAlphanumeric");
			});

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(cfg =>
				{
					cfg.TokenValidationParameters = new TokenValidationParameters()
					{
						ValidateIssuer = false,
						ValidIssuer =						Configuration.GetValue<string>("Security:Token:Issuer"),
						ValidateAudience = false,
						ValidAudience =						Configuration.GetValue<string>("Security:Token:Audience"),
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
															Configuration.GetValue<string>("Security:Token:Key")
						)),
					};
				});

			// Регистрация контроллеров

			services.AddControllersWithViews();
			services.AddControllers();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "DriverAPI", Version = "v1" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseRequestResponseLogging();

			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseMigrationsEndPoint();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DriverAPI v1"));
			}
			else
			{
				app.UseJsonExceptionsHandler();
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
				endpoints.MapRazorPages();
			});
		}

		private void CreateBaseConfig()
		{
			_logger.LogInformation("Настройка параметров Nhibernate...");

			var conStrBuilder = new MySqlConnectionStringBuilder();

			var domainDBConfig =							Configuration.GetSection("DomainDB");

			conStrBuilder.Server =							domainDBConfig.GetValue<string>("Server");
			conStrBuilder.Port =							domainDBConfig.GetValue<uint>("Port");
			conStrBuilder.Database =						domainDBConfig.GetValue<string>("Database");
			conStrBuilder.UserID =							domainDBConfig.GetValue<string>("UserID");
			conStrBuilder.Password =						domainDBConfig.GetValue<string>("Password");
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
				new System.Reflection.Assembly[]
				{
					System.Reflection.Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					System.Reflection.Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.OrganizationMap)),
					System.Reflection.Assembly.GetAssembly(typeof(Bank)),
					System.Reflection.Assembly.GetAssembly(typeof(HistoryMain)),
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

		private void RegisterDependencies(ref IServiceCollection services)
		{
			// Сервисы для контроллеров

			// Unit Of Work
			services.AddScoped<IUnitOfWork>((sp) => UnitOfWorkFactory.CreateWithoutRoot("Мобильное приложение водителей"));

			// ErrorReporter
			services.AddScoped<IErrorReporter>((sp) => SingletonErrorReporter.Instance);

			// Репозитории водовоза
			services.AddScoped<ITrackRepository, TrackRepository>();
			services.AddScoped<IComplaintsRepository, ComplaintsRepository>();
			services.AddScoped<IRouteListRepository, RouteListRepository>();
			services.AddScoped<IStockRepository, StockRepository>();
			services.AddScoped<IRouteListItemRepository, RouteListItemRepository>();
			services.AddScoped<IOrderRepository, OrderRepository>();
			services.AddScoped<IEmployeeRepository, EmployeeRepository>();

			// Провайдеры параметров
			services.AddScoped<IParametersProvider, ParametersProvider>();
			services.AddScoped<IOrderParametersProvider, OrderParametersProvider>();
			services.AddScoped<IDriverApiParametersProvider, DriverApiParametersProvider>();
			services.AddScoped<ITerminalNomenclatureProvider, BaseParametersProvider>();

			// Конвертеры
			foreach(var type in typeof(Library.AssemblyFinder)
									.Assembly
									.GetTypes()
									.Where(type => type.IsClass)
									.Where(type => type.Name.EndsWith("Converter"))
									.ToList())
			{
				services.AddScoped(type);
			}

			// Хелперы
			services.AddScoped<ISmsPaymentServiceAPIHelper, SmsPaymentServiceAPIHelper>();
			services.AddScoped<IFCMAPIHelper, FCMAPIHelper>();
			services.AddScoped<IActionTimeHelper, ActionTimeHelper>();

			// DAL обертки
			services.AddScoped<ITrackPointsModel, TrackPointsModel>();
			services.AddScoped<IDriverMobileAppActionRecordModel, DriverMobileAppActionRecordModel>();
			services.AddScoped<IRouteListModel, RouteListModel>();
			services.AddScoped<IOrderModel, OrderModel>();
			services.AddScoped<IEmployeeModel, EmployeeModel>();
			services.AddScoped<ISmsPaymentModel, SmsPaymentModel>();
			services.AddScoped<IDriverComplaintModel, DriverComplaintModel>();
		}
	}
}

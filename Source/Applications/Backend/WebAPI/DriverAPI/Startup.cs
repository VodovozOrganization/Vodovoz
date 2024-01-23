using DriverAPI.Data;
using DriverAPI.Library;
using DriverAPI.Library.Helpers;
using DriverAPI.Middleware;
using DriverAPI.Options;
using DriverAPI.Services;
using DriverAPI.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Services;
using QS.Services;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using DriverAPI.HealthChecks;
using Vodovoz.Core.DataService;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Models.TrueMark;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using VodovozHealthCheck;
using QS.Project.Domain;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Data.NHibernate;

namespace DriverAPI
{
	internal class Startup
	{
		private const string _nLogSectionName = nameof(NLog);
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
					logging.AddConfiguration(Configuration.GetSection(_nLogSectionName));
				});

			_logger = new Logger<Startup>(LoggerFactory.Create(logging =>
				logging.AddConfiguration(Configuration.GetSection(_nLogSectionName))));

			// Подключение к БД

			var connectionString = Configuration.GetConnectionString("DefaultConnection");

			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

			services.AddDatabaseDeveloperPageExceptionFilter();

			// Конфигурация Nhibernate

			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()
				.AddServiceUser()
				;

			services.AddStaticHistoryTracker();

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
			
			services.AddApiVersioning(config =>
			{
				config.DefaultApiVersion = new ApiVersion(4, 0);
				config.AssumeDefaultVersionWhenUnspecified = true;
				config.ReportApiVersions = true;
				config.ApiVersionReader = new UrlSegmentApiVersionReader();
			});

			services.AddVersionedApiExplorer(config =>
			{
				config.GroupNameFormat = "'v'VVV";
				config.SubstituteApiVersionInUrl = true;
			});

			services.AddSwaggerGen();

			services.ConfigureOptions<ConfigureSwaggerOptions>();
			
			services.AddHttpClient<IFastPaymentsServiceAPIHelper, FastPaymentsesServiceApiHelper>(c =>
			{
				c.BaseAddress = new Uri(Configuration.GetSection("FastPaymentsServiceAPI").GetValue<string>("ApiBase"));
				c.DefaultRequestHeaders.Add("Accept", "application/json");
			});

			services.AddHttpClient<IFCMAPIHelper, FCMAPIHelper>(c =>
			{
				var apiConfiguration = Configuration.GetSection("FCMAPI");

				c.BaseAddress = new Uri(apiConfiguration["ApiBase"]);
				c.DefaultRequestHeaders.Accept.Clear();
				c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + apiConfiguration["AccessToken"]);
			});

			services.ConfigureHealthCheckService<DriverApiHealthCheck>();

			services.AddHttpClient();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseRequestResponseLogging();

			app.UseSwagger();
			app.UseSwaggerUI(options =>
			{
				var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

				foreach(var description in provider.ApiVersionDescriptions)
				{
					options.SwaggerEndpoint(
						 $"/swagger/{description.GroupName}/swagger.json",
						 description.ApiVersion.ToString());
				}
			});

			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseMigrationsEndPoint();
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

			app.ConfigureHealthCheckApplicationBuilder();
		}

		private void RegisterDependencies(ref IServiceCollection services)
		{
			// Сервисы для контроллеров

			// ErrorReporter
			services.AddScoped<IErrorReporter>((sp) => ErrorReporter.Instance);
			services.AddScoped<TrueMarkWaterCodeParser>();
			services.AddScoped<TrueMarkCodesPool, TrueMarkTransactionalCodesPool>();

			// Сервисы
			services.AddSingleton<IWakeUpDriverClientService, WakeUpDriverClientService>();

			// Workers
			services.AddHostedService<WakeUpNotificationSenderService>();

			// Репозитории водовоза
			services.AddScoped<ITrackRepository, TrackRepository>();
			services.AddScoped<IComplaintsRepository, ComplaintsRepository>();
			services.AddScoped<IRouteListRepository, RouteListRepository>();
			services.AddScoped<IStockRepository, StockRepository>();
			services.AddScoped<IRouteListItemRepository, RouteListItemRepository>();
			services.AddScoped<IOrderRepository, OrderRepository>();
			services.AddScoped<IEmployeeRepository, EmployeeRepository>();
			services.AddScoped<IFastPaymentRepository, FastPaymentRepository>();
			services.AddScoped<ICarRepository, CarRepository>();

			// Провайдеры параметров
			services.AddScoped<IParametersProvider, ParametersProvider>();
			services.AddScoped<IOrderParametersProvider, OrderParametersProvider>();
			services.AddScoped<IDriverApiParametersProvider, DriverApiParametersProvider>();
			services.AddScoped<ITerminalNomenclatureProvider, BaseParametersProvider>();
			services.AddScoped<INomenclatureParametersProvider, NomenclatureParametersProvider>();

			services.AddScoped<IPersonProvider, BaseParametersProvider>();
			services.AddScoped<ITerminalNomenclatureProvider, BaseParametersProvider>();

			services.AddDriverApiLibrary();

			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

			services.AddScoped<ICallTaskWorker, CallTaskWorker>();
			services.AddScoped<ICallTaskFactory>(context => CallTaskSingletonFactory.GetInstance());
			services.AddScoped<ICallTaskRepository, CallTaskRepository>();

			services.AddScoped<IUserService>(context => ServicesConfig.UserService);
		}
	}
}

using DriverAPI.Data;
using DriverAPI.HealthChecks;
using DriverAPI.Library;
using DriverAPI.Library.Helpers;
using DriverAPI.Services;
using DriverAPI.Workers;
using Infrastructure.WebApi.Telemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.DB;
using QS.Project.Services.Interactive;
using System;
using System.Text;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Models.TrueMark;
using Vodovoz.Presentation.WebApi;
using Vodovoz.Presentation.WebApi.BuildVersion;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Trackers;
using VodovozHealthCheck;

namespace DriverAPI
{
	/// <summary>
	/// Конфигурация контейнера зависимостей
	/// </summary>
	public static class DependencyInjection
	{
		/// <summary>
		/// Основная конфигурация DriverApi
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public static IServiceCollection AddDriverApi(this IServiceCollection services, IConfiguration configuration)
		{
			// Подключение к БД

			var connectionString = configuration.GetConnectionString("DefaultConnection");

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
					typeof(EmployeeWithLoginMap).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddInfrastructure()
				.AddTrackedUoW()
				.AddOrderTrackerFor1c()
				;


			services.AddScoped<IUnitOfWork>((sp) => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot())
				// Сервисы для контроллеров

				// ErrorReporter
				.AddScoped<IErrorReporter>((sp) => ErrorReporter.Instance)
				.AddScoped<TrueMarkWaterCodeParser>()
				.AddScoped<TrueMarkCodesPool, TrueMarkTransactionalCodesPool>()

				// Телеметрия

				.AddApiOpenTelemetry("driver_api")

				// Сервисы
				.AddSingleton<IWakeUpDriverClientService, WakeUpDriverClientService>()
				//добавляем сервисы, т.к. в методе Order.SendUpdToEmailOnFinishIfNeeded() есть их вызов
				.AddScoped<IInteractiveQuestion, ConsoleInteractiveQuestion>()
				.AddScoped<IInteractiveMessage, ConsoleInteractiveMessage>()
				.AddScoped<IInteractiveService, ConsoleInteractiveService>()

				.AddDriverApiLibrary(configuration)

				.AddScoped<ICallTaskWorker, CallTaskWorker>()
				.AddScoped<ICallTaskFactory>(context => CallTaskSingletonFactory.GetInstance())
				.AddDriverApiHostedServices();

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
			services.AddStaticHistoryTracker();

			// Аутентификация
			services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>();

			services.Configure<IdentityOptions>(options =>
			{
				// Password settings
				options.Password.RequireDigit = configuration.GetValue<bool>("Security:Password:RequireDigit");
				options.Password.RequireLowercase = configuration.GetValue<bool>("Security:Password:RequireLowercase");
				options.Password.RequireNonAlphanumeric = configuration.GetValue<bool>("Security:Password:RequireNonAlphanumeric");
				options.Password.RequireUppercase = configuration.GetValue<bool>("Security:Password:RequireUppercase");
				options.Password.RequiredLength = configuration.GetValue<int>("Security:Password:RequiredLength");
				options.Password.RequiredUniqueChars = configuration.GetValue<int>("Security:Password:RequiredUniqueChars");

				// Lockout settings.
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
															configuration.GetValue<int>("Security:Lockout:DefaultLockoutTimeSpan"));
				options.Lockout.MaxFailedAccessAttempts = configuration.GetValue<int>("Security:Lockout:MaxFailedAccessAttempts");
				options.Lockout.AllowedForNewUsers = configuration.GetValue<bool>("Security:Password:AllowedForNewUsers");

				// User settings.
				options.User.AllowedUserNameCharacters = configuration.GetValue<string>("Security:Password:AllowedUserNameCharacters");
				options.User.RequireUniqueEmail = configuration.GetValue<bool>("Security:Password:RequireNonAlphanumeric");
			});

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(cfg =>
				{
					cfg.TokenValidationParameters = new TokenValidationParameters()
					{
						ValidateIssuer = false,
						ValidIssuer = configuration.GetValue<string>("Security:Token:Issuer"),
						ValidateAudience = false,
						ValidAudience = configuration.GetValue<string>("Security:Token:Audience"),
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
															configuration.GetValue<string>("Security:Token:Key")
						)),
					};
				});

			// Регистрация контроллеров

			services.AddControllersWithViews();

			var commonWebApiPresentationAssembly = typeof(BuildVersionController).Assembly;

			services.AddControllers()
				.AddApplicationPart(commonWebApiPresentationAssembly);

			services.AddVersioning();

			services.AddHttpClient<IFastPaymentsServiceAPIHelper, FastPaymentsesServiceApiHelper>(c =>
			{
				c.BaseAddress = new Uri(configuration.GetSection("FastPaymentsServiceAPI").GetValue<string>("ApiBase"));
				c.DefaultRequestHeaders.Add("Accept", "application/json");
			});

			services.ConfigureHealthCheckService<DriverApiHealthCheck, ServiceInfoProvider>();

			services.AddHttpClient();

			return services;
		}

		/// <summary>
		/// Добавление сервисок работающих в фоновом режиме
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddDriverApiHostedServices(this IServiceCollection services) =>
			services.AddHostedService<WakeUpNotificationSenderService>();
	}
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Project.Services;
using QS.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using TrueMarkApi.HealthChecks;
using TrueMarkApi.Services;
using TrueMarkApi.Services.Authorization;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Edo;
using Vodovoz.Settings.Edo;
using VodovozHealthCheck;


namespace TrueMarkApi
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
			var apiSection = Configuration.GetSection("Api");

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "TrueMarkApi", Version = "v1" });
			});

			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
				});

			_logger = new Logger<Startup>(LoggerFactory.Create(logging =>
				logging.AddNLogWeb(NLogBuilder.ConfigureNLog("NLog.config").Configuration)));

			services.AddControllers();
			services.AddCore();
			services.AddTrackedUoW();

			services.AddHostedService<DocumentService>();
			services.AddSingleton<IAuthorizationService, AuthorizationService>();
			services.AddSingleton<IOrderRepository, OrderRepository>();
			services.AddSingleton<IOrganizationRepository, OrganizationRepository>();
			services.AddSingleton<IEdoSettings, EdoSettings>();
			services.AddSingleton<ISettingsController, SettingsController>();
			services.AddHttpClient();

			// Авторизация
			services.AddAuthorization();
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = false,
						ValidateAudience = false,
						ValidateLifetime = false,
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiSection.GetValue<string>("SecurityKey")))
					};
				});

			services.ConfigureHealthCheckService<TrueMarkHealthCheck>(true);

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

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrueMarkApi v1"));
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			app.ConfigureHealthCheckApplicationBuilder();
		}

		private void CreateBaseConfig(IServiceCollection services)
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
				.Driver<LoggedMySqlClientDriver>();

			var provider = services.BuildServiceProvider();
			var ormConfig = provider.GetRequiredService<IOrmConfig>();
			ormConfig.ConfigureOrm(
				dbConfig,
				new[]
				{
					Assembly.GetAssembly(typeof(SettingMap)),
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.Data.NHibernate.AssemblyFinder)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment))
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

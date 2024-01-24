using System.Linq;
using System.Reflection;
using System.Text;
using EventsApi.Library;
using LogisticsEventsApi.Data;
using LogisticsEventsApi.HealthChecks;
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
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Settings.Database;

namespace LogisticsEventsApi
{
	public class Startup
	{
		private const string _nLogSectionName = nameof(NLog);
		public static readonly string[] AccessedRoles =
			{
				ApplicationUserRole.WarehousePicker.ToString(),
				ApplicationUserRole.WarehouseDriver.ToString()
			};
		
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "LogisticsEventsApi", Version = "v1" }); });
			
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection(_nLogSectionName));
				});

			RegisterDependencies(services);

			//закомментил пока нет зарегистрированных пользователей
			//services.ConfigureHealthCheckService<LogisticsEventsApiHealthCheck>();
			services.AddHttpClient();
			
			var connectionString = CreateBaseConfig();
			
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
			
			// Аутентификация
			services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>();

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(cfg =>
				{
					cfg.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = false,
						ValidIssuer = Configuration.GetValue<string>("SecurityToken:Issuer"),
						ValidateAudience = false,
						ValidAudience =	Configuration.GetValue<string>("SecurityToken:Audience"),
						ValidateIssuerSigningKey = true,
						IssuerSigningKey =
							new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("SecurityToken:Key")
						))
					};
				});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseSwagger();
			app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LogisticsEventsApi v1"));

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
		
		private void RegisterDependencies(IServiceCollection services)
		{
			services.AddScoped((sp) => UnitOfWorkFactory.GetDefaultFactory);
			services.AddScoped((sp) => UnitOfWorkFactory.CreateWithoutRoot("Приложение для сканирования событий(склад)"));
			
			services.AddWarehouseEventsDependencies();
		}
		
		private string CreateBaseConfig()
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
				.ConnectionString(connectionString)
				.Driver<LoggedMySqlClientDriver>()
				.AdoNetBatchSize(100);

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				dbConfig,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(EmployeeWithLoginMap)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				}
			);

			var userLogin = domainDbConfig.GetValue<string>("UserID");
			var serviceUserId = 0;

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Получение пользователя"))
			{
				var serviceUser = unitOfWork.Session
					.Query<UserBase>()
					.FirstOrDefault(u => u.Login == userLogin);

				serviceUserId = serviceUser.Id;

				ServicesConfig.UserService = new UserService(serviceUser);
			}

			UserRepository.GetCurrentUserId = () => serviceUserId;
			HistoryMain.Enable(conStrBuilder);

			return connectionString;
		}
	}
}

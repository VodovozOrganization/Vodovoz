using System.Linq;
using System.Reflection;
using System.Text;
using EventsApi.Library;
using LogisticsEventsApi.Data;
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
using Vodovoz.Settings.Database;

namespace LogisticsEventsApi
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
			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "LogisticsEventsApi", Version = "v1" }); });
			
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection(_nLogSectionName));
				});

			RegisterDependencies(services);

			//services.ConfigureHealthCheckService<CustomerAppsApiHealthCheck>();
			services.AddHttpClient();
			
			var connectionString = CreateBaseConfig();
			
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
			
			// Аутентификация
			services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();
			
			/*services.Configure<IdentityOptions>(options =>
			{
				// Password settings
				options.Password.RequireDigit =	Configuration.GetValue<bool>("Security:Password:RequireDigit");
				options.Password.RequireLowercase =	Configuration.GetValue<bool>("Security:Password:RequireLowercase");
				options.Password.RequireNonAlphanumeric = Configuration.GetValue<bool>("Security:Password:RequireNonAlphanumeric");
				options.Password.RequireUppercase =	Configuration.GetValue<bool>("Security:Password:RequireUppercase");
				options.Password.RequiredLength = Configuration.GetValue<int>("Security:Password:RequiredLength");
				options.Password.RequiredUniqueChars = Configuration.GetValue<int>("Security:Password:RequiredUniqueChars");

				// Lockout settings.
				options.Lockout.DefaultLockoutTimeSpan =
					TimeSpan.FromMinutes(Configuration.GetValue<int>("Security:Lockout:DefaultLockoutTimeSpan"));
				options.Lockout.MaxFailedAccessAttempts = Configuration.GetValue<int>("Security:Lockout:MaxFailedAccessAttempts");
				options.Lockout.AllowedForNewUsers = Configuration.GetValue<bool>("Security:Password:AllowedForNewUsers");

				// User settings.
				options.User.AllowedUserNameCharacters = Configuration.GetValue<string>("Security:Password:AllowedUserNameCharacters");
				options.User.RequireUniqueEmail = Configuration.GetValue<bool>("Security:Password:RequireNonAlphanumeric");
			});*/

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
				//.Dialect<MySQL57SpatialExtendedDialect>()
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

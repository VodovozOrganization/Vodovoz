using DriverAPI.Data;
using DriverAPI.HealthChecks;
using DriverAPI.Library.Helpers;
using DriverAPI.Middleware;
using DriverAPI.Options;
using FirebaseAdmin;
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
using Microsoft.IdentityModel.Tokens;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.DB;
using QS.Services;
using Serilog;
using System;
using System.Text;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Presentation.WebApi.BuildVersion;
using Vodovoz.Presentation.WebApi.ErrorHandling;
using VodovozHealthCheck;

namespace DriverAPI
{
	internal class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
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
					typeof(EmployeeWithLoginMap).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddInfrastructure()
				.AddTrackedUoW()
				;

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
			services.AddStaticHistoryTracker();

			services.AddDriverApi(Configuration);

			// Аутентификация
			services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddRoles<IdentityRole>()
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

			var commonWebApiPresentationAssembly = typeof(BuildVersionController).Assembly;

			services.AddControllers()
				.AddApplicationPart(commonWebApiPresentationAssembly);
			
			services.AddApiVersioning(config =>
			{
				config.DefaultApiVersion = new ApiVersion(5, 0);
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

			services.ConfigureHealthCheckService<DriverApiHealthCheck>();

			services.AddHttpClient();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<FirebaseApp>();
			app.ApplicationServices.GetService<IUserService>();
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
				app.UseMiddleware<ErrorHandlingMiddleware>();
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseSerilogRequestLogging();

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
	}
}

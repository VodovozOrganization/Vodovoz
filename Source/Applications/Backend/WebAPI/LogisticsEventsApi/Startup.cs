using EventsApi.Library;
using FirebaseAdmin;
using LogisticsEventsApi.Data;
using LogisticsEventsApi.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Services;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Presentation.WebApi;
using VodovozHealthCheck;

namespace LogisticsEventsApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "LogisticsEventsApi", Version = "v1"
				});
			});

			services.AddWarehouseEventsDependencies(Configuration);

			//закомментил пока нет зарегистрированных пользователей
			services.ConfigureHealthCheckService<LogisticsEventsApiHealthCheck, ServiceInfoProvider>();
			services.AddHttpClient();

			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(EmployeeWithLoginMap).Assembly,
					typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddInfrastructure()
				.AddTrackedUoW()
				;

			services.AddStaticHistoryTracker();

			services.AddDbContext<ApplicationDbContext>((provider, options) =>
			{
				var connectionStringBuilder = provider.GetRequiredService<MySqlConnectionStringBuilder>();
				options.UseMySql(connectionStringBuilder.ConnectionString, ServerVersion.AutoDetect(connectionStringBuilder.ConnectionString));
			});

			// Аутентификация
			services
				.AddDefaultIdentity<IdentityUser>(options
					=> options.SignIn.RequireConfirmedAccount = true)
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>();

			services.AddSecurity(Configuration)
				.AddOnlyOneSessionRestriction();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetRequiredService<FirebaseApp>();

			app.ApplicationServices.GetService<IUserService>();
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

			app.UseVodovozHealthCheck();
		}
	}
}

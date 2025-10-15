using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using PayPageAPI.Controllers;
using PayPageAPI.HealthChecks;
using PayPageAPI.Models;
using QS.BusinessCommon.HMap;
using QS.DomainModel.UoW;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using VodovozHealthCheck;

namespace PayPageAPI
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
			services
				.AddSwaggerGen(c =>
				{
					c.SwaggerDoc("v1", new OpenApiInfo { Title = "PayPageAPI", Version = "v1" });
				})
				.AddLogging(
					logging =>
					{
						logging.ClearProviders();
						logging.AddNLogWeb();
						logging.AddConfiguration(Configuration.GetSection("NLog"));
					});

			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(EmployeeWithLoginMap).Assembly,
					typeof(MeasurementUnitsMap).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddInfrastructure()
				.AddTrackedUoW()
				;

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);

			// Подключение к БД
			services.AddScoped(provider => provider.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot("Страница быстрых платежей"));
			
			services.AddOptions();
			services.AddMemoryCache();
			
			//load general configuration from appsettings.json
			services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
			//load ip rules from appsettings.json
			services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

			// inject counter and rules stores
			services.AddInMemoryRateLimiting();
			
			services.AddControllersWithViews();

			//factories
			services.AddSingleton<IPayViewModelFactory, PayViewModelFactory>();
			
			//configs and settings
			services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
			
			//models
			services.AddScoped<IAvangardFastPaymentModel, AvangardFastPaymentModel>();

			services.ConfigureHealthCheckService<PayPageHealthCheck>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseIpRateLimiting();
			
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PayPageAPI v1"));
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
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
	}
}

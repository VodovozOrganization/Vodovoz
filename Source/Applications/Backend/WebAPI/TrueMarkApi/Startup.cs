using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;
using QS.HistoryLog;
using QS.Project.Core;
using System.Text;
using TrueMarkApi.HealthChecks;
using TrueMarkApi.Services;
using TrueMarkApi.Services.Authorization;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Data.NHibernate;
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

			services.AddLogging(logging =>
			{
				logging.ClearProviders();
				logging.AddNLogWeb();
				logging.AddConfiguration(Configuration.GetSection("NLog"));
			});


			services.AddMappingAssemblies(
				typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
				typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
				typeof(QS.Banks.Domain.Bank).Assembly,
				typeof(QS.HistoryLog.HistoryMain).Assembly,
				typeof(QS.Project.Domain.TypeOfEntity).Assembly,
				typeof(QS.Attachments.Domain.Attachment).Assembly,
				typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
			);
			services.AddDatabaseConnection();
			services.AddCore();
			services.AddTrackedUoW();
			services.AddServiceUser();
			services.AddStaticHistoryTracker();

			services.AddControllers();
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
	}
}

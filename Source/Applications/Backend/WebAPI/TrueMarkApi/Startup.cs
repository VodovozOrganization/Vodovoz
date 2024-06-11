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
using QS.Services;
using System.Text;
using TrueMarkApi.HealthChecks;
using TrueMarkApi.Options;
using TrueMarkApi.Services.Authorization;
using VodovozHealthCheck;
using System.Net.Http.Headers;

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

			services.AddControllers();
			services.AddHttpClient();

			services.AddSingleton<IAuthorizationService, AuthorizationService>();

			// Авторизация	
			services.AddAuthorization();
			var securityKey = Configuration.GetSection(nameof(TrueMarkApiOptions)).GetValue<string>("InternalSecurityKey");
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = false,
						ValidateAudience = false,
						ValidateLifetime = false,
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey))
					};
				});

			services.ConfigureHealthCheckService<TrueMarkHealthCheck>(true);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<IUserService>();
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

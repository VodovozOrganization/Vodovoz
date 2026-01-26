using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CustomerAppsApi.HealthChecks;
using VodovozHealthCheck;
using TaxcomEdoApi.HealthChecks;

namespace TaxcomEdoApi
{
	public class Startup
	{
		private const string _nLogSectionName = nameof(NLog);
		private Logger<Startup> _logger;

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

			_logger.LogInformation("Логирование Startup начато");

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			services.AddControllers()
				.AddXmlSerializerFormatters()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.Converters.Add(
						new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
				});

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "TaxcomEdoApi", Version = "v1"
				});
			});

			services
				.AddConfig(Configuration)
				.AddDependencyGroup()
				.AddHttpClient()
				.ConfigureHealthCheckService<TaxcomEdoApiHealthCheck, ServiceInfoProvider>(true);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaxcomEdoApi v1"));
			}
			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
			
			app.UseVodovozHealthCheck();
		}
	}
}

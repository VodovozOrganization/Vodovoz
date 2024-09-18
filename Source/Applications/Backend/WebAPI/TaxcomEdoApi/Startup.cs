using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using QS.HistoryLog;
using QS.Project.Core;
using System.Text;
using QS.Services;
using TaxcomEdoApi.HealthChecks;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;
using VodovozHealthCheck;
using Vodovoz.Application;
using Vodovoz.Infrastructure.FileStorage;
using TaxcomEdoApi.Options;

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
				.AddXmlSerializerFormatters();

			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaxcomEdoApi", Version = "v1" }); });

			services.AddMappingAssemblies(
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
				.AddTrackedUoW()
				.AddStaticHistoryTracker()
				.AddStaticScopeForEntity()
				.AddConfig(Configuration)
				.ConfigureOptions<ConfigureS3Options>()
				.AddApplication()
				.AddFileStorage()
				.AddDependencyGroup()
				.ConfigureHealthCheckService<TaxcomEdoApiHealthCheck>(true);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<IUserService>();
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaxcomEdoApi v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

			app.ConfigureHealthCheckApplicationBuilder();
		}
	}
}

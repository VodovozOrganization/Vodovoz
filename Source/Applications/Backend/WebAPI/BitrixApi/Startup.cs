using BitrixApi.Library;
using Infrastructure.WebApi.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QS.HistoryLog;
using QS.Project.Core;
using System.Text;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Core.Domain;
using Vodovoz.Presentation.WebApi;
using Vodovoz.Presentation.WebApi.ErrorHandling;

namespace BitrixApi
{
	public class Startup
	{
		private readonly IConfiguration _configuration;

		static Startup()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		public Startup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddFeatureManagement();

			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(EmployeeWithLoginMap).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()
				.AddBitrixApiServices()
				.AddApiOpenTelemetry("bitrix.api");

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
			services.AddStaticHistoryTracker();

			services
				.AddSecurity(_configuration)
				.AddAuthorizationIfNeeded();

			services.AddControllers()
				.AddSharedControllers();

			services
				.AddVersioning();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseSwagger();

			if(env.IsDevelopment() || _configuration.GetValue<bool>("SwaggerEnabled"))
			{
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
			}

			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseMiddleware<ErrorHandlingMiddleware>();
			}

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}

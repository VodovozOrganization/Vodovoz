using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QS.Services;
using System;
using Vodovoz.Presentation.WebApi.ErrorHandling;
using Vodovoz.RobotMia.Api.HealthCheck;
using VodovozHealthCheck;

namespace Vodovoz.RobotMia.Api
{
	/// <summary>
	/// Настройка Api
	/// </summary>
	public class Startup
	{
		private readonly IConfiguration _configuration;

		/// <summary>
		/// Конструктор
		/// </summary>
		public Startup(IConfiguration configuration)
		{
			_configuration = configuration
				?? throw new ArgumentNullException(nameof(configuration));
		}

		/// <summary>
		/// Подключение сервисов
		/// </summary>
		/// <param name="services"></param>
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRobotMiaApi(_configuration);
			services.ConfigureHealthCheckService<RobotMiaApiHealthCheck, ServiceInfoProvider>();
		}

		/// <summary>
		/// Настройка приложения
		/// </summary>
		/// <param name="app"></param>
		/// <param name="env"></param>
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<IUserService>();
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
			
			app.UseVodovozHealthCheck();
		}
	}
}

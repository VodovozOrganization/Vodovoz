using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using QS.Services;
using Vodovoz.Presentation.WebApi.ErrorHandling;

namespace SecureCodeSenderApi
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
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "SecureCodeSenderApi", Version = "v1" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<IUserService>();
			app.UseSwagger();

			if(env.IsDevelopment() || Configuration.GetValue<bool>("SwaggerEnabled"))
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

			app.UseHttpsRedirection();
			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}

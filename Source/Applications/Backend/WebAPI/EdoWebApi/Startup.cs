using EdoApi.Library.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

namespace EdoWebApi
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
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); //Подключаем больше кодировок

			services.AddControllers().AddXmlSerializerFormatters();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "EdoWebApi", Version = "v1" });
			});

			services.AddHttpClient<IAuthorizationService, AuthorizationService>(c =>
			{
				c.BaseAddress = new Uri(Configuration.GetValue<string>("TaxcomServices:BaseAddress"));
				c.DefaultRequestHeaders.Add("Integrator-Id", Configuration.GetValue<string>("TaxcomServices:IntegratorId"));
			});

			services.AddHttpClient<IContactListService, ContactListService>(c =>
			{
				c.BaseAddress = new Uri(Configuration.GetValue<string>("TaxcomServices:BaseAddress"));
				c.DefaultRequestHeaders.Add("Integrator-Id", Configuration.GetValue<string>("TaxcomServices:IntegratorId"));
			});

			services.AddHttpClient<ITrueApiService, TrueApiService>(c =>
			{
				c.BaseAddress = new Uri(Configuration.GetValue<string>("TrueApiService:BaseAddress"));
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EdoWebApi v1"));
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

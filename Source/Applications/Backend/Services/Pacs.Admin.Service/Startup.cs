﻿using ApiAuthentication;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pacs.Admin.Server;
using QS.BusinessCommon.HMap;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;

namespace Pacs.Admin.Service
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
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()
				.AddMessageTransportSettings()
				.AddPacsAdminServices()
				.AddMappingAssemblies(
					typeof(QS.Banks.Domain.Account).Assembly,
					typeof(MeasurementUnitsMap).Assembly)
				;

			services.AddApiKeyAuthentication();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

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

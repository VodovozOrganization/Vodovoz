using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Services;
using System;
using UnsubscribePage.Controllers;
using UnsubscribePage.HealthChecks;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Database.Common;
using VodovozHealthCheck;

namespace UnsubscribePage
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
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				});

			services.AddControllersWithViews();

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
				.AddInfrastructure()
				.AddTrackedUoW()
				.ConfigureHealthCheckService<UnsubscribePageHealthCheck, ServiceInfoProvider>()
				;

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
			services.AddStaticHistoryTracker();
			
			services.AddHttpClient();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			builder.RegisterType<UnsubscribeViewModelFactory>()
				.As<IUnsubscribeViewModelFactory>()
				.SingleInstance();

			builder.RegisterType<EmailSettings>()
				.As<IEmailSettings>()
				.SingleInstance();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<IUserService>();
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Unsubscribe}/{action=Index}/{id?}");
			});

			app.UseVodovozHealthCheck();
		}
	}
}

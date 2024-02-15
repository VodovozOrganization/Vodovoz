using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.DB;
using System;
using System.Reflection;
using UnsubscribePage.Controllers;
using UnsubscribePage.HealthChecks;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;
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
					typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()
				.ConfigureHealthCheckService<UnsubscribePageHealthCheck>()
				;

			services.AddStaticHistoryTracker();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			builder.RegisterModule<DatabaseSettingsModule>();

			builder.RegisterType<UnsubscribeViewModelFactory>()
				.As<IUnsubscribeViewModelFactory>()
				.SingleInstance();

			builder.RegisterType<EmailRepository>()
				.As<IEmailRepository>()
				.SingleInstance();

			builder.RegisterType<EmailParametersProvider>()
				.As<IEmailParametersProvider>()
				.SingleInstance();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
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

			app.ConfigureHealthCheckApplicationBuilder();
		}
	}
}

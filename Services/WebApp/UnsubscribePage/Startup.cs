using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using NLog.Web;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using System;
using Newtonsoft.Json;
using QS.Attachments.Domain;
using UnsubscribePage.Controllers;
using UnsubscribePage.Models;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Parameters;

namespace UnsubscribePage
{
	public class Startup
	{
		private ILogger<Startup> _logger;
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllersWithViews();


			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
				});

			_logger = new Logger<Startup>(LoggerFactory.Create(logging =>
				logging.AddNLogWeb(NLogBuilder.ConfigureNLog("NLog.config").Configuration)));

			// Подключение к БД
			//services.AddScoped(_ => UnitOfWorkFactory.CreateWithoutRoot("Страница быстрых платежей"));
			//services.AddControllersWithViews()
			//	.AddNewtonsoftJson(options =>
			//	{
			//		options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			//	});


			// Конфигурация Nhibernate
			try
			{
				CreateBaseConfig();
			}
			catch(Exception e)
			{
				_logger.LogCritical(e, e.Message);
				throw;
			}

			services.AddOptions();

			services.AddSingleton<IEmailRepository, EmailRepository>();
			services.AddSingleton<IEmailParametersProvider, EmailParametersProvider>();
			services.AddSingleton<IParametersProvider, ParametersProvider>();
			services.AddSingleton<IUnsubscribeViewModelFactory, UnsubscribeViewModelFactory>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Unsubscribe}/{action=Index}/{id?}");
			});
		}

		private void CreateBaseConfig()
		{
			_logger.LogInformation("Настройка параметров Nhibernate...");

			var conStrBuilder = new MySqlConnectionStringBuilder();

			var domainDBConfig = Configuration.GetSection("DomainDB");

			conStrBuilder.Server = domainDBConfig.GetValue<string>("Server");
			conStrBuilder.Port = domainDBConfig.GetValue<uint>("Port");
			conStrBuilder.Database = domainDBConfig.GetValue<string>("Database");
			conStrBuilder.UserID = domainDBConfig.GetValue<string>("UserID");
			conStrBuilder.Password = domainDBConfig.GetValue<string>("Password");
			conStrBuilder.SslMode = MySqlSslMode.None;

			var connectionString = conStrBuilder.GetConnectionString(true);

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString);

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				db_config,
				new System.Reflection.Assembly[]
				{
					System.Reflection.Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					System.Reflection.Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.TypeOfEntityMap)),
					System.Reflection.Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.OrganizationMap)),
					System.Reflection.Assembly.GetAssembly(typeof(Bank)),
					System.Reflection.Assembly.GetAssembly(typeof(HistoryMain)),
					System.Reflection.Assembly.GetAssembly(typeof(Attachment))
				}
			);

			HistoryMain.Enable();
		}
	}
}

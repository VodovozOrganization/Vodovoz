using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using QS.DomainModel.UoW;
using QS.Project.Core;
using QS.Project.DB;
using Pacs.Admin.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.Mapping.Pacs;
using Vodovoz.Data.NHibernate.NhibernateExtensions;

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
				.AddCoreServerServices()
				.AddSingleton<IUnitOfWorkFactory>(UnitOfWorkFactory.GetDefaultFactory)
				.AddPacsAdminServices()
				;

			CreateBaseConfig();
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

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private static void CreateBaseConfig()
		{
			var conStrBuilder = new MySqlConnectionStringBuilder();


			conStrBuilder.Server = "dev.sql.vod.qsolution.ru";
			conStrBuilder.Port = 3307;
			conStrBuilder.Database = "Vodovoz_dev_test";
			conStrBuilder.UserID = "enzogord";
			conStrBuilder.Password = "dire5Dz8";
			conStrBuilder.SslMode = MySqlSslMode.None;

			var connectionString = conStrBuilder.GetConnectionString(true);

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString)
				.AdoNetBatchSize(100)
				.Driver<LoggedMySqlClientDriver>()
				;

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				db_config,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(SettingsMap))
				}
			);
		}
	}


}

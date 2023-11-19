using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Pacs.Admin.Server;
using QS.DomainModel.UoW;
using QS.Project.Core;
using QS.Project.DB;
using System.Reflection;
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
			var transportSettings = new TransportSettings();
			Configuration.Bind("MessageTransport", transportSettings);

			services
				.AddCoreServerServices()
				.AddSingleton<IUnitOfWorkFactory>(UnitOfWorkFactory.GetDefaultFactory)
				.AddPacsAdminServices(transportSettings)
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

		private void CreateBaseConfig()
		{
			var dbSection = Configuration.GetSection("DomainDB");
			var conStrBuilder = new MySqlConnectionStringBuilder();

			conStrBuilder.Server = dbSection.GetValue<string>("Server");
			conStrBuilder.Port = dbSection.GetValue<uint>("Port");
			conStrBuilder.Database = dbSection.GetValue<string>("Database");
			conStrBuilder.UserID = dbSection.GetValue<string>("UserID");
			conStrBuilder.Password = dbSection.GetValue<string>("Password");
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

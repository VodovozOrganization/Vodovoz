using Autofac;
using FluentNHibernate.Cfg.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using NHibernate.Dialect;
using QS.DomainModel.UoW;
using QS.Project.DB;
using Sms.External.SmsRu;
using Sms.Internal.Service.Middleware;
using SmsRu;
using System.Reflection;
using Vodovoz.Settings.Database;

namespace Sms.Internal.Service
{
	public class Startup
    {
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }


		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
			CreateBaseConfig();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			builder.RegisterType<DefaultSessionProvider>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<DefaultUnitOfWorkFactory>().AsSelf().AsImplementedInterfaces();

			builder.RegisterModule<DatabaseSettingsModule>();

			builder.RegisterInstance(GetSmsRuSettings()).AsSelf().AsImplementedInterfaces();
			builder.RegisterType<SmsRuSendController>().AsSelf().AsImplementedInterfaces();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

			app.UseMiddleware<ApiKeyMiddleware>();

			app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<SmsService>();
            });
		}

		private ISmsRuConfiguration GetSmsRuSettings()
		{
			var smsRuSection = Configuration.GetSection("SmsRu");

			var smsRuConfig = new SmsRuConfiguration(
				smsRuSection["login"],
				smsRuSection["password"],
				smsRuSection["appId"],
				smsRuSection["partnerId"],
				smsRuSection["email"],
				smsRuSection["smsNumberFrom"],
				smsRuSection["smtpLogin"],
				smsRuSection["smtpPassword"],
				smsRuSection["smtpServer"],
				int.Parse(smsRuSection["smtpPort"]),
				bool.Parse(smsRuSection["smtpUseSSL"]),
				bool.Parse(smsRuSection["translit"]),
				bool.Parse(smsRuSection["test"])
				);
			return smsRuConfig;
		}

		private void CreateBaseConfig()
		{
			var conStrBuilder = new MySqlConnectionStringBuilder();

			var domainDBConfig = Configuration.GetSection("DomainDB");

			conStrBuilder.Server = domainDBConfig.GetValue<string>("Server");
			conStrBuilder.Port = domainDBConfig.GetValue<uint>("Port");
			conStrBuilder.Database = domainDBConfig.GetValue<string>("Database");
			conStrBuilder.UserID = domainDBConfig.GetValue<string>("UserID");
			conStrBuilder.Password = domainDBConfig.GetValue<string>("Password");
			conStrBuilder.SslMode = MySqlSslMode.None;

			var connectionString = conStrBuilder.GetConnectionString(true);

			var db_config = MySQLConfiguration.Standard
				.Dialect<MySQL57Dialect>()
				.ConnectionString(connectionString)
				.AdoNetBatchSize(100);

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				db_config,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				}
			);
		}
	}
}

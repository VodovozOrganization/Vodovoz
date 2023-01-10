﻿using Autofac;
using DeliveryRulesService.Cache;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fias.Client;
using Fias.Client.Cache;
using Vodovoz;
using Vodovoz.Core.DataService;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;

namespace DeliveryRulesService
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

			services.AddMvc().AddControllersAsServices();

			services.AddControllers().AddJsonOptions(j =>
			{
				//Необходимо для сериализации свойств как PascalCase
				j.JsonSerializerOptions.PropertyNamingPolicy = null;
			});

			NLogBuilder.ConfigureNLog("NLog.config");

			CreateBaseConfig();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterType<DefaultSessionProvider>().AsImplementedInterfaces();
			builder.RegisterType<DefaultUnitOfWorkFactory>().AsImplementedInterfaces();
			builder.RegisterType<BaseParametersProvider>().AsImplementedInterfaces();
			builder.RegisterType<DistrictCache>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<GeocoderCache>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<FiasApiClientFactory>().AsSelf().AsImplementedInterfaces();
			
			builder.RegisterType<CallTaskWorker>()
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterType<UserService>()
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterInstance(ErrorReporter.Instance).AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
				.Where(t => t.Name.EndsWith("Handler"))
				.AsSelf()
				.AsImplementedInterfaces();

			var vodovozBusinessAssembly = typeof(VodovozBusinessAssemblyFinder).Assembly;

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly)
				.Where(t => t.Name.EndsWith("Provider"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly)
				.Where(t => t.Name.EndsWith("Model"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly)
				.Where(t => t.Name.EndsWith("Repository"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly, Assembly.GetExecutingAssembly())
				.Where(t => t.Name.EndsWith("Factory"))
				.AsSelf()
				.AsImplementedInterfaces();

			builder.RegisterAssemblyTypes(vodovozBusinessAssembly)
				.Where(t => t.Name.EndsWith("Controller"))
				.AsSelf()
				.AsImplementedInterfaces();
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

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
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
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.Organizations.OrganizationMap)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment))
				}
			);
		}
	}
}

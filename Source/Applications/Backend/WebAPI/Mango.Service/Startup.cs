﻿using Mango.Api;
using Mango.CallsPublishing;
using Mango.Client;
using Mango.Core.Handlers;
using Mango.Service.Handlers;
using Mango.Service.HostedServices;
using Mango.Service.Services;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NLog.Web;
using QS.DomainModel.UoW;
using QS.Project.DB;
using System;
using System.Reflection;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Mango;
using Vodovoz.Settings.Mango;

namespace Mango.Service
{
	public class Startup
	{
		private const string _nLogSectionName = "NLog";
		private ILoggerFactory _loggerFactory;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			var nlogConfig = Configuration.GetSection(_nLogSectionName);
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(nlogConfig);
				});

			_loggerFactory = LoggerFactory.Create(logging =>
				logging.AddConfiguration(Configuration.GetSection(_nLogSectionName)));


			var dbSection = Configuration.GetSection("DomainDB");
			if(!dbSection.Exists())
			{
				throw new ArgumentException("Не найдена секция DomainDB в конфигурации");
			}
			var connectionStringBuilder = new MySqlConnectionStringBuilder();
			connectionStringBuilder.Server = dbSection["Server"];
			connectionStringBuilder.Port = uint.Parse(dbSection["Port"]);
			connectionStringBuilder.Database = dbSection["Database"];
			connectionStringBuilder.UserID = dbSection["UserID"];
			connectionStringBuilder.Password = dbSection["Password"];
			connectionStringBuilder.SslMode = MySqlSslMode.None;
			connectionStringBuilder.DefaultCommandTimeout = 5;

			CreateBaseConfig(connectionStringBuilder);

			services.AddSingleton(x =>
				new MySqlConnection(connectionStringBuilder.ConnectionString));

			services.AddSingleton(x => new MangoController(
				_loggerFactory.CreateLogger<MangoController>(), 
				Configuration["Mango:VpbxApiKey"], 
				Configuration["Mango:VpbxApiSalt"])
			);

			services.AddSingleton<IDataBaseInfo>((sp) => new DataBaseLocalInfo(connectionStringBuilder.Database));
			services.AddSingleton<IUnitOfWorkFactory>((sp) => UnitOfWorkFactory.GetDefaultFactory);
			services.AddSingleton<ISettingsController, SettingsController>();
			services.AddSingleton<IMangoUserSettngs, MangoUserSettings>();

			services.AddSingleton<CallsHostedService>();
			services.AddHostedService(provider => provider.GetService<CallsHostedService>());

			services.AddSingleton<PhonebookHostedService>();
			services.AddHostedService(provider => provider.GetService<PhonebookHostedService>());

			services.AddSingleton<NotificationHostedService>();
			services.AddHostedService(provider => provider.GetService<NotificationHostedService>());

			services.AddSingleton<ICallerService, CallerService>();
			services.AddSingleton<ICallEventHandler, MangoHandler>();

			var messageTransportSettings = new MessageTransportSettings(Configuration);
			services.AddSingleton<IMessageTransportSettings>((sp) => messageTransportSettings);

			services.ConfigureMangoServices();
			services.AddCallsPublishing(messageTransportSettings);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

#if DEBUG
			app.UseMiddleware<PerformanceMiddleware>();
#endif

			app.UseRouting();
			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}

		private void CreateBaseConfig(MySqlConnectionStringBuilder conStrBuilder)
		{
			var connectionString = conStrBuilder.ConnectionString;

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString)
				.Driver<LoggedMySqlClientDriver>();

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				db_config,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.TypeOfEntityMap)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				}
			);
		}
	}

	public class DataBaseLocalInfo : IDataBaseInfo
	{
		public DataBaseLocalInfo(string database)
		{
			Name = database;
		}

		public string Name { get; }

		public bool IsDemo => false;

		public Guid? BaseGuid => null;

		public Version Version => null;
	}
}

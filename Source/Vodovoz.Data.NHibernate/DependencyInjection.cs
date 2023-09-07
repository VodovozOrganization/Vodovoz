using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Services;
using QS.Services;
using System;
using System.Linq;
using System.Reflection;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Data.NHibernate.Options;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Data.NHibernate
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddDatabase(this IServiceCollection services, HostBuilderContext hostContext)
		{
			//services.Configure<DatabaseUserSettings>(options => hostContext.Configuration.GetSection(nameof(DatabaseUserSettings)).Bind(options));

			var databaseSettings = new DatabaseUserSettings();
			hostContext.Configuration.GetSection(nameof(DatabaseUserSettings)).Bind(databaseSettings);

			ConfigureUserConnection(databaseSettings);

			// Unit Of Work
			services.AddSingleton<IUnitOfWorkFactory>((sp) => UnitOfWorkFactory.GetDefaultFactory);

			return services;
		}

		private static void ConfigureUserConnection(DatabaseUserSettings settings)
		{
			var mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder()
			{
				Server = settings.ServerName,
				Port = settings.Port,
				Database = settings.DatabaseName,
				UserID = settings.UserName,
				Password = settings.Password,
				SslMode = settings.MySqlSslMode
			};

			var connectionString = mySqlConnectionStringBuilder.GetConnectionString(true);

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString)
				.Driver<LoggedMySqlClientDriver>()
				.AdoNetBatchSize(100);

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				db_config,
				new Assembly[]
				{
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(QS.Project.HibernateMapping.TypeOfEntityMap).Assembly,
					typeof(DependencyInjection).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(Vodovoz.Settings.Database.VodovozSettingsDatabaseAssemblyFinder).Assembly
				}
			);

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Получение пользователя"))
			{
				var serviceUser = unitOfWork.Session.Query<User>()
					.Where(u => u.Login == settings.UserName)
					.FirstOrDefault();

				if(serviceUser is null)
				{
					throw new InvalidOperationException("Service user not found");
				}

				int serviceUserId = serviceUser.Id;

				ServicesConfig.UserService = new UserService(serviceUser);

				QS.Project.Repositories.UserRepository.GetCurrentUserId = () => serviceUserId;

				HistoryMain.Enable(mySqlConnectionStringBuilder);
			}
		}
	}
}

using System;
using System.Reflection;
using QS.Project.DB;
using Vodovoz;
using Vodovoz.Settings.Database;

namespace VodovozBusinessTests.Deletion
{
	public static class ConfigureOneTime
	{
		static bool NhConfigered = false;
		static bool DeletionConfigured = false;

		public static void ConfigureNh()
		{
			throw new NotImplementedException("Необходима интеграция с контейнером зависимостей");
			if(NhConfigered)
				return;

			Console.WriteLine("Инициализация");
			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
								.Dialect<NHibernate.Spatial.Dialect.MySQL57SpatialDialect>()
								.ConnectionString("server=vod.qsolution.ru;port=3306;database=test-test;user id=test_only;password=7qqKWuNugQF2Y2W1;sslmode=None;");

			Console.WriteLine("ORM");
			// Настройка ORM
			/*OrmConfig.ConfigureOrm(db_config, new Assembly[]
			{
				Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
				Assembly.GetAssembly(typeof(Vodovoz.Data.NHibernate.AssemblyFinder)),
				Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.TypeOfEntityMap)),
				Assembly.GetAssembly(typeof(QS.Banks.HMap.BankMap)),
				Assembly.GetAssembly(typeof(QS.HistoryLog.HistoryMain)),
				Assembly.GetAssembly(typeof(QS.Attachments.Domain.Attachment)),
				Assembly.GetAssembly(typeof(AssemblyFinder))
			});*/

			NhConfigered = true;
		}

		public static void ConfigureDeletion()
		{
			if(DeletionConfigured)
				return;

			Console.WriteLine("Delete");
			Configure.ConfigureDeletion();
			DeletionConfigured = true;
		}
	}
}

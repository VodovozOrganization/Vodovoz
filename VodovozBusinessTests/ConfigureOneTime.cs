using System;
using NUnit.Framework;
using QS.Project.DB;
using Vodovoz;

namespace VodovozBusinessTests.Deletion
{
	public static class ConfigureOneTime
	{
		static bool NhConfigered = false;
		static bool DeletionConfigured = false;

		public static void ConfigureNh()
		{
			if(NhConfigered)
				return;

			Console.WriteLine("Инициализация");
			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
								.Dialect<NHibernate.Spatial.Dialect.MySQL57SpatialDialect>()
								.ConnectionString("server=vod.qsolution.ru;port=3306;database=Vodovoz_test;user id=test_only;password=7qqKWuNugQF2Y2W1;sslmode=None;");

			Console.WriteLine("ORM");
			// Настройка ORM
			OrmConfig.ConfigureOrm(db_config, new System.Reflection.Assembly[] {
				System.Reflection.Assembly.GetAssembly (typeof(QS.Project.HibernateMapping.UserBaseMap)),
				System.Reflection.Assembly.GetAssembly (typeof(Vodovoz.HibernateMapping.OrganizationMap)),
				System.Reflection.Assembly.GetAssembly (typeof(QSBanks.QSBanksMain)),
				System.Reflection.Assembly.GetAssembly (typeof(QSContacts.QSContactsMain)),
				System.Reflection.Assembly.GetAssembly (typeof(QS.HistoryLog.HistoryMain)),
			});

			NhConfigered = true;
		}

		public static void ConfogureDeletion()
		{
			if(DeletionConfigured)
				return;

			Console.WriteLine("Delete");
			Configure.ConfigureDeletion();
			DeletionConfigured = true;
		}
	}
}

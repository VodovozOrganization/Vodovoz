using MySql.Data.MySqlClient;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.HibernateMapping;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Vodovoz.Domain.TrueMark;
using Vodovoz.HibernateMapping.Organizations;
using Vodovoz.Models.TrueMark;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Settings.Database;

namespace LoadToPool
{
    internal class Program
    {
        static void Main(string[] args)
        {
			CreateBaseConfig();

			var path = @"C:\Users\Enzo\Desktop\codes.txt";
			var codes = File.ReadAllLines(path);

			for(int i = 0; i < codes.Length; i++)
			{
				var task1 = SaveCode(codes[i++]);
				var task2 = SaveCode(codes[i++]);
				var task3 = SaveCode(codes[i++]);
				var task4 = SaveCode(codes[i++]);
				var task5 = SaveCode(codes[i]);

				Task.WaitAll(new[] { task1, task2, task3, task4, task5 } );
			}
		}

		private static async Task SaveCode(string code)
		{
			await Task.Run(() => {

				TrueMarkWaterCodeParser parser = new TrueMarkWaterCodeParser();
				TrueMarkCodesPool pool = new TrueMarkCodesPool(UnitOfWorkFactory.GetDefaultFactory);

				Console.WriteLine(code);

				var parsed = parser.TryParse(code, out TrueMarkWaterCode parsedCode);
				if(!parsed)
				{
					return;
				}

				var codeEntity = new TrueMarkWaterIdentificationCode();
				codeEntity.IsInvalid = false;
				codeEntity.RawCode = parsedCode.SourceCode;
				codeEntity.GTIN = parsedCode.GTIN;
				codeEntity.SerialNumber = parsedCode.SerialNumber;
				codeEntity.CheckCode = parsedCode.CheckCode;

				using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					try
					{
						uow.Save(codeEntity);
					}
					catch(Exception)
					{
						return;
					}
					uow.Commit();
				}

				pool.PutCode(codeEntity.Id);
			});
		}

		private static void CreateBaseConfig()
		{
			var conStrBuilder = new MySqlConnectionStringBuilder();

			conStrBuilder.Server = "sql.vod.qsolution.ru";
			conStrBuilder.Port = 3306;
			conStrBuilder.Database = "Vodovoz_honeybee";
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
					Assembly.GetAssembly(typeof(DatabaseSettingsModule)),
					Assembly.GetAssembly(typeof(UserBaseMap)),
					Assembly.GetAssembly(typeof(OrganizationMap)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				}
			);
		}
	}
}

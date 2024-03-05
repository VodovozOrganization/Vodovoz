using Autofac;
using QS.Permissions;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI;
using QS.Widgets.GtkUI;
using QSProjectsLib;
using System.Data.Common;
using System.IO;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.JournalColumnsConfigs;
using Vodovoz.Services.Permissions;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewWidgets.Permissions;

namespace Vodovoz
{
	partial class Startup
	{
		internal static IDataBaseInfo DataBaseInfo;

		static void CreateProjectParam()
		{
		}

		static void ConfigureJournalColumnsConfigs()
		{
			JournalsColumnsConfigs.RegisterColumns();
		}

		static void GetPermissionsSettings() {
			string    sql = "SELECT * FROM permissions_settings";
			DbCommand cmd = QSMain.ConnectionDB.CreateCommand();
			cmd.CommandText = sql;
			using (DbDataReader rdr = cmd.ExecuteReader())
			{
				while (rdr.Read())
				{
					PermissionsSettings.PresetPermissions.Add(rdr["name"].ToString(),
						new PresetUserPermissionSource(rdr["name"].ToString(), rdr["display_name"].ToString(),
							string.IsNullOrEmpty(rdr["description"].ToString()) ? "" : rdr["description"].ToString()));
				}
			}
		}

		public static void CreateTempDir()
		{
			var userId = QSMain.User?.Id;

			if(userId == null)
				return;

			var tempVodUserPath = Path.Combine(Path.GetTempPath(), "Vodovoz", userId.ToString());
			DirectoryInfo dirInfo = new DirectoryInfo(tempVodUserPath);

			if(!dirInfo.Exists)
				dirInfo.Create();
		}

		public static void ClearTempDir()
		{
			var userId = QSMain.User?.Id;

			if(userId == null)
				return;

			var tempVodUserPath = Path.Combine(Path.GetTempPath(), "Vodovoz", userId.ToString());
			DirectoryInfo dirInfo = new DirectoryInfo(tempVodUserPath);

			if(dirInfo.Exists)
			{
				foreach(FileInfo file in dirInfo.EnumerateFiles()) {
					file.Delete();
				}
				foreach(DirectoryInfo dir in dirInfo.EnumerateDirectories()) {
					dir.Delete(true);
				}

				dirInfo.Delete();
			}
		}

		public static ILifetimeScope AppDIContainer;
	}
}

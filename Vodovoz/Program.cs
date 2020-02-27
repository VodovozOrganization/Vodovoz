using System;
using Gtk;
using NLog;
using QSProjectsLib;
using Gdk;
using Vodovoz.Parameters;
using Vodovoz.Additions;
using Vodovoz.DriverTerminal;
using QS.Project.Repositories;
using EmailService;
using QS.Project.Dialogs.GtkUI;
using QS.Tools;
using QS.Project.Dialogs.GtkUI.ServiceDlg;
using QS.Utilities.Text;
using QSSupportLib;
using QS.Widgets.GtkUI;
using Vodovoz.Domain.Client;
using QSOrmProject;
using QS.Project.DB;
using SolrSearch;
using System.Reflection;
using Vodovoz.Domain.Orders;
using SolrSearch.Mapping;
using Vodovoz.SolrModel;
using System.Collections.Generic;

namespace Vodovoz
{
	partial class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static MainWindow MainWin;
		public static IProgressBarDisplayable progressBarWin;

		[STAThread]
		public static void Main (string[] args)
		{
			Application.Init ();
			QSMain.SubscribeToUnhadledExceptions();
			QSMain.GuiThread = System.Threading.Thread.CurrentThread;
			MainSupport.SendErrorRequestEmail = false;

			//FIXME Удалить после того как будет удалена зависимость от библиотеки QSProjectLib
			QSMain.ProjectPermission = new System.Collections.Generic.Dictionary<string, UserPermission>();

			CreateProjectParam ();
			ConfigureViewModelWidgetResolver();
			ConfigureJournalColumnsConfigs();

			QSMain.SetupFromArgs(args);

			// Создаем окно входа
			Login LoginDialog = new Login ();
			LoginDialog.Logo = Gdk.Pixbuf.LoadFromResource ("Vodovoz.icons.logo.png");
			LoginDialog.SetDefaultNames ("Vodovoz");
			LoginDialog.DefaultLogin = "user";
			LoginDialog.DefaultServer = "sql.vod.qsolution.ru";
			LoginDialog.UpdateFromGConf ();

			ResponseType LoginResult;
			LoginResult = (ResponseType)LoginDialog.Run ();
			if (LoginResult == ResponseType.DeleteEvent || LoginResult == ResponseType.Cancel)
				return;

			LoginDialog.Destroy ();

			QSProjectsLib.PerformanceHelper.StartMeasurement ("Замер запуска приложения");

			MainSupport.HandleStaleObjectStateException = EntityChangedExceptionHelper.ShowExceptionMessage;

			//Настройка базы
			CreateBaseConfig ();
			QSProjectsLib.PerformanceHelper.AddTimePoint (logger, "Закончена настройка базы");
			VodovozGtkServicesConfig.CreateVodovozDefaultServices();

			SolrCompositionRoot();

			MainSupport.LoadBaseParameters();
			ParametersProvider.Instance.RefreshParameters();

			//Настройка карты
			GMap.NET.MapProviders.GMapProvider.UserAgent = String.Format("{0}/{1} used GMap.Net/{2} ({3})",
				QSSupportLib.MainSupport.ProjectVerion.Product,
				VersionHelper.VersionToShortString(QSSupportLib.MainSupport.ProjectVerion.Version),
				VersionHelper.VersionToShortString(System.Reflection.Assembly.GetAssembly(typeof(GMap.NET.MapProviders.GMapProvider)).GetName().Version),
				Environment.OSVersion.VersionString
			);
			GMap.NET.MapProviders.GMapProvider.Language = GMap.NET.LanguageType.Russian;
			QSProjectsLib.PerformanceHelper.AddTimePoint (logger, "Закончена настройка карты.");

			DatePicker.DefaultWidthRequest = 400;
			DateRangePicker.DefaultWidthRequest = 800;

			QSOsm.OsmWorker.ServiceHost = "osm.vod.qsolution.ru";
			QSOsm.OsmWorker.ServicePort = 7073;
			QSOsm.OsmWorker.MaxReceivedMessageSize = 2000000;
			QSOsm.Osrm.OsrmMain.ServerUrl = "http://osrm.vod.qsolution.ru:5000";
			
			PerformanceHelper.StartPointsGroup ("Главное окно");

			MainSupport.TestVersion(null); //Проверяем версию базы
			QSMain.CheckServer(null); // Проверяем настройки сервера

			PerformanceHelper.AddTimePoint("Закончена загрузка параметров базы и проверка версии.");

			if(QSMain.User.Login == "root") {
				string Message = "Вы зашли в программу под администратором базы данных. У вас есть только возможность создавать других пользователей.";
				MessageDialog md = new MessageDialog(null, DialogFlags.Modal,
									   MessageType.Info,
									   ButtonsType.Ok,
									   Message);
				md.Run();
				md.Destroy();
				UsersDialog usersDlg = new UsersDialog();
				usersDlg.Show();
				usersDlg.Run();
				usersDlg.Destroy();
				return;
			}else if(UserPermissionRepository.CurrentUserPresetPermissions["driver_terminal"]){
				DriverTerminalWindow driverTerminal = new DriverTerminalWindow();
				progressBarWin = driverTerminal;
				driverTerminal.Title = "Печать документов МЛ";
				QSMain.ErrorDlgParrent = driverTerminal;
				driverTerminal.Show();
			}else{
				//Настрока удаления
				Configure.ConfigureDeletion();
				PerformanceHelper.AddTimePoint(logger, "Закончена настройка удаления");

				if(ParametersProvider.Instance.ContainsParameter("email_send_enabled_database") && ParametersProvider.Instance.ContainsParameter("email_service_address")) {
					if(ParametersProvider.Instance.GetParameterValue("email_send_enabled_database") == LoginDialog.BaseName) {
						EmailServiceSetting.Init(ParametersProvider.Instance.GetParameterValue("email_service_address"));
					}
				}

				CreateTempDir();

				//Запускаем программу
				MainWin = new MainWindow();
				progressBarWin = MainWin;
				MainWin.Title += string.Format(" (БД: {0})", LoginDialog.BaseName);
				QSMain.ErrorDlgParrent = MainWin;
				MainWin.Show();
			}

			PerformanceHelper.EndPointsGroup ();

			PerformanceHelper.AddTimePoint (logger, "Закончен старт SAAS. Конец загрузки.");

			QSSaaS.Session.StartSessionRefresh ();

			QSProjectsLib.PerformanceHelper.AddTimePoint (logger, "Закончен старт SAAS. Конец загрузки.");
			QSProjectsLib.PerformanceHelper.Main.PrintAllPoints (logger);

			Application.Run ();
			QSSaaS.Session.StopSessionRefresh ();
			ClearTempDir();
		}

		private static void SolrCompositionRoot()
		{
			string solrCoreAddress = @"http://localhost:8983/solr/Vodovoz_test";
			Utils.Register<Dictionary<string, object>>(solrCoreAddress);
			Utils.Register<CounterpartySolrEntity>(solrCoreAddress);
		}
	}
}

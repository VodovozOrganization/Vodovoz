using System;
using Gtk;
using NLog;
using QSProjectsLib;
using Gdk;
using QSSupportLib;
using Vodovoz.Additions;

namespace Vodovoz
{
	partial class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static MainWindow MainWin;
		public static StatusIcon TrayIcon;

		[STAThread]
		public static void Main (string[] args)
		{
			Application.Init ();
			QSMain.SubscribeToUnhadledExceptions();
			QSMain.GuiThread = System.Threading.Thread.CurrentThread;
			MainSupport.SendErrorRequestEmail = false;

			TrayIcon = new StatusIcon(Pixbuf.LoadFromResource ("Vodovoz.icons.logo.png"));
			TrayIcon.Visible = true;
			TrayIcon.Tooltip = "Веселый Водовоз";

			CreateProjectParam ();
			QSMain.SetupFromArgs(args);
			// Создаем окно входа
			Login LoginDialog = new Login ();
			LoginDialog.Logo = Gdk.Pixbuf.LoadFromResource ("Vodovoz.icons.logo.png");
			LoginDialog.SetDefaultNames ("Vodovoz");
			LoginDialog.DefaultLogin = "user";
			LoginDialog.DefaultServer = "vod-srv.qsolution.ru";
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

			//Настрока удаления
			ConfigureDeletion ();
			QSProjectsLib.PerformanceHelper.AddTimePoint (logger, "Закончена настройка удаления");

			//Настройка карты
			GMap.NET.MapProviders.GMapProvider.UserAgent = String.Format("{0}/{1} used GMap.Net/{2} ({3})",
				QSSupportLib.MainSupport.ProjectVerion.Product,
				StringWorks.VersionToShortString(QSSupportLib.MainSupport.ProjectVerion.Version),
				StringWorks.VersionToShortString(System.Reflection.Assembly.GetAssembly(typeof(GMap.NET.MapProviders.GMapProvider)).GetName().Version),
				Environment.OSVersion.VersionString
			);
			GMap.NET.MapProviders.GMapProvider.Language = GMap.NET.LanguageType.Russian;

			QSProjectsLib.PerformanceHelper.AddTimePoint (logger, "Закончена настройка карты.");

			VodovozService.Chats.ChatMain.ChatServer = "vod-srv.qsolution.ru:9000";

			MainSupport.LoadBaseParameters();
			if(MainSupport.BaseParameters.All.ContainsKey("email_service_address")) {
				EmailService.EmailServiceSetting.EmailServiceURL = MainSupport.BaseParameters.All["email_service_address"];
			}else {
				EmailService.EmailServiceSetting.EmailServiceURL = null;
			}
			
			QSOsm.OsmWorker.ServiceHost = "vod-srv.qsolution.ru";
			QSOsm.OsmWorker.ServicePort = 9000;
			QSOsm.Osrm.OsrmMain.ServerUrl = "http://vod.qsolution.ru:5000";
			
			PerformanceHelper.StartPointsGroup ("Главное окно");

			MainSupport.TestVersion(null); //Проверяем версию базы
			QSMain.CheckServer(null, true); // Проверяем настройки сервера

			PerformanceHelper.AddTimePoint("Закончена загрузка параметров базы и проверка версии.");

			if(QSMain.User.Login == "root") {
				string Message = "Вы зашли в программу под администратором базы данных. У вас есть только возможность создавать других пользователей.";
				MessageDialog md = new MessageDialog(null, DialogFlags.Modal,
									   MessageType.Info,
									   ButtonsType.Ok,
									   Message);
				md.Run();
				md.Destroy();
				Users WinUser = new Users();
				WinUser.Show();
				WinUser.Run();
				WinUser.Destroy();
				return;
			}else if(QSMain.User.Login == "enzogord"){
				MessageDialog md = new MessageDialog(null, DialogFlags.Modal,
									   MessageType.Info,
									   ButtonsType.Ok,
									   "Запускаем окно для печати документов водителями");
				md.Run();
				md.Destroy();
			}else{
				//Запускаем программу
				MainWin = new MainWindow();
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
		}
	}
}

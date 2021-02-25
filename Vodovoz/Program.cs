using System;
using System.Data.Common;
using Gtk;
using NLog;
using QSProjectsLib;
using Vodovoz.Parameters;
using Vodovoz.Additions;
using EmailService;
using QS.Project.Dialogs.GtkUI;
using QS.Utilities.Text;
using QSSupportLib;
using QS.Widgets.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using InstantSmsService;
using QS.Project.Services;
using Vodovoz.Core.DataService;
using QS.ErrorReporting;
using Vodovoz.Infrastructure;
using Vodovoz.Tools;
using QS.Osm;
using QS.Tools;
using SmsPaymentService;
using System.Security.Principal;
using Vodovoz.Domain.Security;
using System.Linq;

namespace Vodovoz
{
	partial class MainClass
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		public static MainWindow MainWin;

		[STAThread]
		public static void Main (string[] args)
		{
			Application.Init ();
			QSMain.GuiThread = System.Threading.Thread.CurrentThread;
			var appInfo = new ApplicationInfo();

			#region Первоначальная настройка обработки ошибок
			SingletonErrorReporter.Initialize(ReportWorker.GetReportService(), appInfo, new LogService(), null, false, null);
			var errorMessageModelFactory = new DefaultErrorMessageModelFactory(SingletonErrorReporter.Instance, null, null);
			var exceptionHandler = new DefaultUnhandledExceptionHandler(errorMessageModelFactory, appInfo);

			exceptionHandler.SubscribeToUnhandledExceptions();
			exceptionHandler.GuiThread = System.Threading.Thread.CurrentThread;
			MainSupport.HandleStaleObjectStateException = EntityChangedExceptionHelper.ShowExceptionMessage;
			#endregion

			//FIXME Удалить после того как будет удалена зависимость от библиотеки QSProjectLib
			QSMain.ProjectPermission = new System.Collections.Generic.Dictionary<string, UserPermission>();

			CreateProjectParam ();
			ConfigureViewModelWidgetResolver();
			ConfigureJournalColumnsConfigs();

			QSMain.SetupFromArgs(args);
			QS.Project.Search.GtkUI.SearchView.QueryDelay = 1500;

			Gtk.Settings.Default.SetLongProperty("gtk-button-images", 1, "");
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

			PerformanceHelper.StartMeasurement ("Замер запуска приложения");
			GetPermissionsSettings();
			//Настройка базы
			CreateBaseConfig ();
			PerformanceHelper.AddTimePoint (logger, "Закончена настройка базы");
			VodovozGtkServicesConfig.CreateVodovozDefaultServices();

			MainSupport.LoadBaseParameters();
			ParametersProvider.Instance.RefreshParameters();

			#region Настройка обработки ошибок c параметрами из базы и сервисами
			var baseParameters = new BaseParametersProvider();
			SingletonErrorReporter.Initialize(
				ReportWorker.GetReportService(),
				appInfo,
				new LogService(), 
				LoginDialog.BaseName, 
				LoginDialog.BaseName == baseParameters.GetDefaultBaseForErrorSend(),
				baseParameters.GetRowCountForErrorLog()
			);

			var errorMessageModelFactory2 = new DefaultErrorMessageModelFactory(SingletonErrorReporter.Instance, ServicesConfig.UserService, UnitOfWorkFactory.GetDefaultFactory);
			exceptionHandler.InteractiveService = ServicesConfig.InteractiveService;
			exceptionHandler.ErrorMessageModelFactory = errorMessageModelFactory2;
			//Настройка обычных обработчиков ошибок.
			exceptionHandler.CustomErrorHandlers.Add(CommonErrorHandlers.MySqlException1055OnlyFullGroupBy);
			exceptionHandler.CustomErrorHandlers.Add(CommonErrorHandlers.MySqlException1366IncorrectStringValue);
			exceptionHandler.CustomErrorHandlers.Add(CommonErrorHandlers.NHibernateFlushAfterException);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.NHibernateStaleObjectStateExceptionException);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.MySqlExceptionConnectionTimeout);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.MySqlExceptionAuth);
			
			#endregion

			//Настройка карты
			GMap.NET.MapProviders.GMapProvider.UserAgent = String.Format("{0}/{1} used GMap.Net/{2} ({3})",
				QSSupportLib.MainSupport.ProjectVerion.Product,
				VersionHelper.VersionToShortString(QSSupportLib.MainSupport.ProjectVerion.Version),
				VersionHelper.VersionToShortString(System.Reflection.Assembly.GetAssembly(typeof(GMap.NET.MapProviders.GMapProvider)).GetName().Version),
				Environment.OSVersion.VersionString
			);
			GMap.NET.MapProviders.GMapProvider.Language = GMap.NET.LanguageType.Russian;
			PerformanceHelper.AddTimePoint (logger, "Закончена настройка карты.");

			DatePicker.CalendarFontSize = 16;
			DateRangePicker.CalendarFontSize = 16;

			OsmWorker.ServiceHost = "osm.vod.qsolution.ru";
			OsmWorker.ServicePort = 7073;

			QS.Osm.Osrm.OsrmMain.ServerUrl = "http://osrm.vod.qsolution.ru:5000";
			
			PerformanceHelper.StartPointsGroup ("Главное окно");

			MainSupport.TestVersion(null); //Проверяем версию базы
			QSMain.CheckServer(null); // Проверяем настройки сервера

			PerformanceHelper.AddTimePoint("Закончена загрузка параметров базы и проверка версии.");

			AutofacClassConfig();
			PerformanceHelper.AddTimePoint("Закончена настройка AutoFac.");
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
			} else {
                if (ChangePassword(LoginDialog.BaseName) && CanLogin())
                {
					StartMainWindow(LoginDialog.BaseName);
				}
				else
					return;
			}

			PerformanceHelper.EndPointsGroup ();

			PerformanceHelper.AddTimePoint (logger, "Закончен старт SAAS. Конец загрузки.");

			QSSaaS.Session.StartSessionRefresh ();

			PerformanceHelper.AddTimePoint (logger, "Закончен старт SAAS. Конец загрузки.");
			PerformanceHelper.Main.PrintAllPoints (logger);

			Application.Run();
			QSSaaS.Session.StopSessionRefresh ();
			ClearTempDir();
		}

		private static bool ChangePassword(string loginDialogName)
		{
			using(var UoW = UnitOfWorkFactory.GetDefaultFactory.CreateForRoot<User>(QSMain.User.Id)) {
				if(!UoW.Root.NeedPasswordChange)
					return true;

				ChangePassword changePasswordWindow = new ChangePassword();
				changePasswordWindow.Title = "Требуется сменить пароль";
				QSMain.ErrorDlgParrent = changePasswordWindow;

				int response = changePasswordWindow.Run();
				if(response == (int)ResponseType.Ok) {
					UoW.Root.NeedPasswordChange = false;
					UoW.Save();
					changePasswordWindow.Destroy();
					return true;
				} else {
					QSSaaS.Session.StopSessionRefresh();
					ClearTempDir();
					return false;
				}
			}
		}

		private static void StartMainWindow(string loginDialogName)
		{
			//Настрока удаления
			Configure.ConfigureDeletion();
			PerformanceHelper.AddTimePoint(logger, "Закончена настройка удаления");
			//Настройка сервисов
			if(ParametersProvider.Instance.ContainsParameter("email_send_enabled_database") && ParametersProvider.Instance.ContainsParameter("email_service_address")) {
				if(ParametersProvider.Instance.GetParameterValue("email_send_enabled_database") == loginDialogName) {
					EmailServiceSetting.Init(ParametersProvider.Instance.GetParameterValue("email_service_address"));
				}
			}
			if(ParametersProvider.Instance.ContainsParameter("instant_sms_enabled_database") && ParametersProvider.Instance.ContainsParameter("sms_service_address")) {
				if(ParametersProvider.Instance.GetParameterValue("instant_sms_enabled_database") == loginDialogName) {
					InstantSmsServiceSetting.Init(ParametersProvider.Instance.GetParameterValue("sms_service_address"));
				}
			}
			
			if(ParametersProvider.Instance.ContainsParameter("sms_payment_send_enabled_database") && ParametersProvider.Instance.ContainsParameter("sms_payment_send_service_address")) {
				if(ParametersProvider.Instance.GetParameterValue("sms_payment_send_enabled_database") == loginDialogName) {
					SmsPaymentServiceSetting.Init(ParametersProvider.Instance.GetParameterValue("sms_payment_send_service_address"));
				}
			}

			CreateTempDir();

			//Запускаем программу
			MainWin = new MainWindow();
			MainWin.Title += string.Format(" (БД: {0})", loginDialogName);
			QSMain.ErrorDlgParrent = MainWin;
			MainWin.Show();
		}

		private static bool CanLogin()
        {
			using (var uow = UnitOfWorkFactory.GetDefaultFactory.CreateWithoutRoot())
			{
				var dBLogin = ServicesConfig.CommonServices.UserService.GetCurrentUser(uow).Login;

				string sid = "";
				// Получение данных пользователя системы
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					var windowsIdentity = WindowsIdentity.GetCurrent();
					sid = windowsIdentity.User?.ToString() ?? "";
				}

				RegisteredRM registeredRMAlias = null;
				var rm = uow.Session.QueryOver<RegisteredRM>(() => registeredRMAlias).Where(x => x.SID == sid && x.IsActive).List().FirstOrDefault();

				return (rm == null) || rm.Users.Any(u => u.Login == dBLogin);
			}
		} 
	}
}
using System;
using System.Globalization;
using Gtk;
using NLog;
using QSProjectsLib;
using Vodovoz.Parameters;
using QS.Project.Dialogs.GtkUI;
using QS.Utilities.Text;
using QS.Widgets.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using QS.Project.Services;
using Vodovoz.Core.DataService;
using QS.ErrorReporting;
using Vodovoz.Infrastructure;
using Vodovoz.Tools;
using QS.Tools;
using SmsPaymentService;
using System.Security.Principal;
using Vodovoz.Domain.Security;
using System.Linq;
using System.Net;
using System.Reflection;
using GMap.NET.MapProviders;
using MySql.Data.MySqlClient;
using QS.BaseParameters;
using QS.ChangePassword.Views;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.Project.DB.Passwords;
using QS.Project.Repositories;
using QS.Project.Versioning;
using QS.Validation;
using QS.ViewModels;
using Vodovoz.Configuration;
using Vodovoz.Services;
using Vodovoz.Tools.Validation;
using VodovozInfrastructure.Configuration;
using VodovozInfrastructure.Passwords;
using Connection = QS.Project.DB.Connection;
using UserRepository = Vodovoz.EntityRepositories.UserRepository;
using System.Net.Http;
using QS.Utilities.Debug;
using System.IO;
using QS.Configuration;
using QS.ErrorReporting.Handlers;

namespace Vodovoz
{
	partial class MainClass
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private static IApplicationInfo applicationInfo;
		private static IErrorReportingSettings _errorReportingSettings;
		private static IPasswordValidator passwordValidator;

		public static MainWindow MainWin;

		[STAThread]
		public static void Main (string[] args)
		{
			CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
			Application.Init ();
			QSMain.GuiThread = System.Threading.Thread.CurrentThread;
			applicationInfo = new ApplicationVersionInfo();

			_errorReportingSettings = new ErrorReportingSettings(false, false, true, 100);

			#region Первоначальная настройка обработки ошибок
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;
			var errorMessageModelFactoryWithoutUserService = new DefaultErrorMessageModelFactory(ErrorReporter.Instance, null, null);
			var exceptionHandler = new DefaultUnhandledExceptionHandler(errorMessageModelFactoryWithoutUserService, applicationInfo);

			exceptionHandler.SubscribeToUnhandledExceptions();
			exceptionHandler.GuiThread = System.Threading.Thread.CurrentThread;
			#endregion

			//FIXME Удалить после того как будет удалена зависимость от библиотеки QSProjectLib
			QSMain.ProjectPermission = new System.Collections.Generic.Dictionary<string, UserPermission>();

			CreateProjectParam ();
			ConfigureViewModelWidgetResolver();
			ConfigureJournalColumnsConfigs();

			QSMain.SetupFromArgs(args);
			QS.Project.Search.GtkUI.SearchView.QueryDelay = 1500;
			QS.Views.Control.EntityEntry.QueryDelay = 250;

			var configurationFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Vodovoz.ini");

			var configuration = new IniFileConfiguration(configurationFile);
			Gtk.Settings.Default.SetLongProperty("gtk-button-images", 1, "");
			// Создаем окно входа
			Login LoginDialog = new Login (configuration);
			LoginDialog.Logo = Gdk.Pixbuf.LoadFromResource ("Vodovoz.icons.logo.png");
			LoginDialog.UpdateFromGConf ();

			ResponseType LoginResult;
			LoginResult = (ResponseType)LoginDialog.Run ();
			if (LoginResult == ResponseType.DeleteEvent || LoginResult == ResponseType.Cancel)
				return;

			DataBaseInfo = new Vodovoz.Infrastructure.Database.DatabaseInfo(LoginDialog.BaseName, false);

			LoginDialog.Destroy ();

			PerformanceHelper.StartMeasurement ("Замер запуска приложения");
			GetPermissionsSettings();
			//Настройка базы
			var applicationConfigurator = new ApplicationConfigurator();
			applicationConfigurator.ConfigureOrm();
			applicationConfigurator.CreateApplicationConfig();

			PerformanceHelper.AddTimePoint (logger, "Закончена настройка базы");
			VodovozGtkServicesConfig.CreateVodovozDefaultServices();
			
			var parametersProvider = new ParametersProvider();
			parametersProvider.RefreshParameters();

			#region Настройка обработки ошибок c параметрами из базы и сервисами
			var baseParameters = new BaseParametersProvider(parametersProvider);

			bool canAutomaticallyErrorSend = LoginDialog.BaseName == baseParameters.GetDefaultBaseForErrorSend();
			ErrorReporter.Instance.DatabaseName = LoginDialog.BaseName;
			ErrorReporter.Instance.AutomaticallySendEnabled = canAutomaticallyErrorSend;
			ErrorReporter.Instance.SendedLogRowCount = baseParameters.GetRowCountForErrorLog();

			var errorMessageModelFactoryWithUserService = new DefaultErrorMessageModelFactory(ErrorReporter.Instance, ServicesConfig.UserService, UnitOfWorkFactory.GetDefaultFactory);
			exceptionHandler.InteractiveService = ServicesConfig.InteractiveService;
			exceptionHandler.ErrorMessageModelFactory = errorMessageModelFactoryWithUserService;
			//Настройка обычных обработчиков ошибок.
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.NHibernateStaleObjectStateExceptionHandler);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.MySqlExceptionConnectionTimeoutHandler);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.MySqlExceptionAuthHandler);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.SocketTimeoutException);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.GeoGroupVersionNotFoundException);

			#endregion

			passwordValidator = new PasswordValidator(
				new PasswordValidationSettingsFactory(UnitOfWorkFactory.GetDefaultFactory).GetPasswordValidationSettings()
			);

			//Настройка карты
			IGMapParametersProviders gMapParametersProviders = new GMapPararmetersProviders(parametersProvider); 
			
			GMapProvider.UserAgent = string.Format("{0}/{1} used GMap.Net/{2} ({3})",
				applicationInfo.ProductName,
				applicationInfo.Version.VersionToShortString(),
				Assembly.GetAssembly(typeof(GMapProvider)).GetName().Version.VersionToShortString(),
				Environment.OSVersion.VersionString
			);
			GMapProvider.Language = GMap.NET.LanguageType.Russian;

			var squidServer = string.Empty;

			try
			{
				using(var httpClient = new HttpClient())
				{
					squidServer = gMapParametersProviders.SquidServer;
					if(httpClient.GetAsync($"{squidServer}/squid-internal-static/icons/SN.png").Result.IsSuccessStatusCode)
					{
						GMapProvider.WebProxy = new WebProxy(gMapParametersProviders.SquidServer);
						logger.Info("Используется прокси сервер карт: {MapsProxyServerUrl}", squidServer);
					}
					else
					{
						logger.Warn("Прокси сервер карт недоступен: {MapsProxyServerUrl}", squidServer);
					}
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex, "Прокси сервер карт недоступен: {MapsProxyServerUrl}", squidServer);
			}
			
			
			PerformanceHelper.AddTimePoint (logger, "Закончена настройка карты.");

			DatePicker.CalendarFontSize = 16;
			DateRangePicker.CalendarFontSize = 16;
			
			PerformanceHelper.StartPointsGroup ("Главное окно");

			var baseVersionChecker = new CheckBaseVersion(applicationInfo, new ParametersService(QS.Project.DB.Connection.ConnectionDB));
			if(baseVersionChecker.Check()) {
				ServicesConfig.CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, baseVersionChecker.TextMessage, "Несовпадение версии");
				return;
			}
			QSMain.CheckServer(null); // Проверяем настройки сервера

			PerformanceHelper.AddTimePoint("Закончена загрузка параметров базы и проверка версии.");

			AutofacClassConfig();
			PerformanceHelper.AddTimePoint("Закончена настройка AutoFac.");

			UnhandledExceptionHandler unhandledExceptionHandler = new UnhandledExceptionHandler();
			//Передаем в настройки GUI поток, чтобы обработчик мог отличать вызовы исключений из других потоков и не падал при этом в момент попытки отобразить диалог пользователю.
			GtkGuiDispatcher.GuiThread = System.Threading.Thread.CurrentThread;
			//Получаем все необходимые зависимости из контейнера.
			unhandledExceptionHandler.UpdateDependencies(AppDIContainer);
			//Подписываемся на необработанные исключения текущего приложения.
			unhandledExceptionHandler.SubscribeToUnhandledExceptions();

			if(QSMain.User.Login == "root")
			{
				string Message = "Вы зашли в программу под администратором базы данных. У вас есть только возможность создавать других пользователей.";
				MessageDialog md = new MessageDialog(null, DialogFlags.Modal,
									   MessageType.Info,
									   ButtonsType.Ok,
									   Message);
				md.Run();
				md.Destroy();
				UsersDialog usersDlg = new UsersDialog(ServicesConfig.InteractiveService);
				usersDlg.Show();
				usersDlg.Run();
				usersDlg.Destroy();
				return;
			} 
			
			if(ChangePassword(applicationConfigurator) && CanLogin())
			{
				StartMainWindow(LoginDialog.BaseName, applicationConfigurator, parametersProvider);
			}
			else
			{
				return;
			}

			PermissionExtensionSingletonStore.AssembliesFilter = new[] { "QS", "Vodovoz" };

			PerformanceHelper.EndPointsGroup ();

			PerformanceHelper.AddTimePoint (logger, "Закончен старт SAAS. Конец загрузки.");

			QSSaaS.Session.StartSessionRefresh ();

			PerformanceHelper.AddTimePoint (logger, "Закончен старт SAAS. Конец загрузки.");
			PerformanceHelper.Main.PrintAllPoints (logger);

			Application.Run();
			QSSaaS.Session.StopSessionRefresh ();
			ClearTempDir();
		}

		/// <summary>
		/// Проверяет, необходима ли смена пароля для текущего пользователя, и, если необходима, открывает диалог смены пароля
		/// </summary>
		/// <returns>
		/// <para><b>True</b> - Если смена пароля не нужна или пароль был успешно изменён</para>
		/// <b>False</b> - Если смена была затребована смена пароля, но пароль не был изменён
		/// </returns>
		/// <exception cref="InvalidOperationException">Если текущий пользователь null</exception>
		private static bool ChangePassword(IApplicationConfigurator applicationConfigurator)
		{
			ResponseType result;
			int currentUserId;
			IChangePasswordModel changePasswordModel;
			
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var userRepository = new UserRepository();
				var currentUser = userRepository.GetCurrentUser(uow);
				if(currentUser is null) {
					throw new InvalidOperationException("CurrentUser is null");
				}
				if(!currentUser.NeedPasswordChange) {
					return true;
				}
				currentUserId = currentUser.Id;
				
				if(!(Connection.ConnectionDB is MySqlConnection mySqlConnection)) {
					throw new InvalidOperationException($"Текущее подключение не является {nameof(MySqlConnection)}");
				}

				var mySqlPasswordRepository = new MySqlPasswordRepository();
				changePasswordModel = new MysqlChangePasswordModelExtended(applicationConfigurator, mySqlConnection, mySqlPasswordRepository);
				var changePasswordViewModel = new ChangePasswordViewModel(
					changePasswordModel,
					passwordValidator,
					null
				);
				var changePasswordView = new ChangePasswordView(changePasswordViewModel) {
					Title = "Требуется сменить пароль"
				};
				changePasswordView.ShowAll();
				result = (ResponseType)changePasswordView.Run();
				changePasswordView.Destroy();
			}
		
			if(result == ResponseType.Ok && changePasswordModel.PasswordWasChanged) {
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var user = uow.GetById<User>(currentUserId);
					user.NeedPasswordChange = false;
					uow.Save(user);
					uow.Commit();
					return true;
				}
			}
			
			QSSaaS.Session.StopSessionRefresh();
			ClearTempDir();
			return false;
		}

		private static void StartMainWindow(
			string loginDialogName,
			IApplicationConfigurator applicationConfigurator,
			IParametersProvider parametersProvider)
		{
			//Настрока удаления
			Configure.ConfigureDeletion();
			PerformanceHelper.AddTimePoint(logger, "Закончена настройка удаления");
			
			if(parametersProvider.ContainsParameter("sms_payment_send_enabled_database") && parametersProvider.ContainsParameter("sms_payment_send_service_address")) {
				if(parametersProvider.GetParameterValue("sms_payment_send_enabled_database") == loginDialogName) {
					SmsPaymentServiceSetting.Init(parametersProvider.GetParameterValue("sms_payment_send_service_address"));
				}
			}
			DriverApiParametersProvider.InitializeNotifications(parametersProvider, loginDialogName);

			CreateTempDir();

			//Запускаем программу
			MainWin = new MainWindow(passwordValidator, applicationConfigurator);
			MainWin.Title += $" (БД: {loginDialogName})";
			QSMain.ErrorDlgParrent = MainWin;
			MainWin.Show();
		}

		private static bool CanLogin()
		{
			using (var uow = UnitOfWorkFactory.GetDefaultFactory.CreateWithoutRoot())
			{
				var dBLogin = ServicesConfig.CommonServices.UserService.GetCurrentUser().Login;

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

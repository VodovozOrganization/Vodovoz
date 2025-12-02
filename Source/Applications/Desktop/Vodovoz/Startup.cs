using Autofac;
using Gamma.GtkWidgets;
using GMap.NET.MapProviders;
using Gtk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NHibernate;
using QS.BaseParameters;
using QS.ChangePassword.Views;
using QS.Configuration;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.ErrorReporting;
using QS.Permissions;
using QS.Project.DB.Passwords;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Repositories;
using QS.Project.Services;
using QS.Project.Versioning;
using QS.Services;
using QS.Utilities.Debug;
using QS.Utilities.Text;
using QS.Validation;
using QS.ViewModels;
using QS.Widgets.GtkUI;
using QSProjectsLib;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using Vodovoz.Application.Pacs;
using Vodovoz.Commons;
using Vodovoz.Configuration;
using Vodovoz.Core.Domain.Users;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Database.Logistics;
using Vodovoz.Tools;
using Vodovoz.Tools.Validation;
using VodovozInfrastructure.Configuration;
using VodovozInfrastructure.Passwords;
using Connection = QS.Project.DB.Connection;

namespace Vodovoz
{
	public partial class Startup
	{
		private readonly ILogger<Startup> _logger;
		private readonly IApplicationInfo _applicationInfo;
		private readonly IConfiguration _configuration;
		private static IErrorReportingSettings _errorReportingSettings;
		private readonly IWikiSettings _wikiSettings;
		private readonly ViewModelWidgetsRegistrar _viewModelWidgetsRegistrar;
		private static IPasswordValidator passwordValidator;

		public static MainWindow MainWin;

		public Startup(
			ILogger<Startup> logger,
			ILifetimeScope lifetimeScope,
			IApplicationInfo applicationInfo,
			IConfiguration configuration,
			IErrorReportingSettings errorReportingSettings,
			IWikiSettings wikiSettings,
			ViewModelWidgetsRegistrar viewModelWidgetsRegistrar)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			AppDIContainer = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_errorReportingSettings = errorReportingSettings ?? throw new ArgumentNullException(nameof(errorReportingSettings));
			_wikiSettings = wikiSettings ?? throw new ArgumentNullException(nameof(wikiSettings));
			_viewModelWidgetsRegistrar = viewModelWidgetsRegistrar ?? throw new ArgumentNullException(nameof(viewModelWidgetsRegistrar));
		}

		public void Start(string[] args)
		{
			CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
			Gtk.Application.Init();
			QSMain.GuiThread = System.Threading.Thread.CurrentThread;
			GtkGuiDispatcher.GuiThread = System.Threading.Thread.CurrentThread;
			yTreeView.TreeModelProvider = new VodovozTreeModelProvider();

			#region Первоначальная настройка обработки ошибок
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;
			var errorMessageModelFactoryWithoutUserService = new DefaultErrorMessageModelFactory(ErrorReporter.Instance, null, null);
			var exceptionHandler = new DefaultUnhandledExceptionHandler(AppDIContainer.Resolve<ILogger<DefaultUnhandledExceptionHandler>>(), errorMessageModelFactoryWithoutUserService, _applicationInfo);

			exceptionHandler.SubscribeToUnhandledExceptions();
			exceptionHandler.GuiThread = System.Threading.Thread.CurrentThread;
			#endregion

			//FIXME Удалить после того как будет удалена зависимость от библиотеки QSProjectLib
			QSMain.ProjectPermission = new System.Collections.Generic.Dictionary<string, UserPermission>();

			_viewModelWidgetsRegistrar.RegisterateWidgets(Assembly.GetExecutingAssembly(), typeof(Presentation.Views.DependencyInjection).Assembly);

			ConfigureJournalColumnsConfigs();

			QSMain.SetupFromArgs(args);
			QS.Project.Search.GtkUI.SearchView.QueryDelay = 1500;
			QS.Views.Control.EntityEntry.QueryDelay = 250;

			var configurationFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Vodovoz.ini");

			var configuration = new IniFileConfiguration(configurationFile);
			Gtk.Settings.Default.SetLongProperty("gtk-button-images", 1, "");
			// Создаем окно входа
			Login LoginDialog = new Login(configuration);
			LoginDialog.Logo = Gdk.Pixbuf.LoadFromResource("Vodovoz.icons.logo.png");
			LoginDialog.UpdateFromGConf();

			ResponseType LoginResult;
			LoginResult = (ResponseType)LoginDialog.Run();
			if(LoginResult == ResponseType.DeleteEvent || LoginResult == ResponseType.Cancel)
				return;

			LoginDialog.Destroy();

			// Запрос поключения к базе, необходимо из-за того,
			// что необходима инициализация статики при настройке подключения базы
			AppDIContainer.Resolve<ISessionFactory>();

			PerformanceHelper.StartMeasurement("Замер запуска приложения");
			GetPermissionsSettings();
			
			//Настройка базы
			var applicationConfigurator = new ApplicationConfigurator();
			applicationConfigurator.CreateApplicationConfig();

			CreateProjectParam();

			PerformanceHelper.AddTimePoint("Закончена настройка базы");

			var permissionService = AppDIContainer.Resolve<IPermissionService>();
			var userService = AppDIContainer.Resolve<IUserService>();

			PermissionsSettings.ConfigureEntityPermissionFinder(new Vodovoz.Domain.Permissions.EntitiesWithPermissionFinder());
			PermissionsSettings.PermissionService = permissionService;
			PermissionsSettings.CurrentPermissionService = new CurrentPermissionServiceAdapter(permissionService, userService);

			#region Настройка обработки ошибок c параметрами из базы и сервисами

			var errorSendSettings = AppDIContainer.Resolve<IErrorSendSettings>();
			bool canAutomaticallyErrorSend = LoginDialog.BaseName == errorSendSettings.DefaultBaseForErrorSend;
			ErrorReporter.Instance.DatabaseName = LoginDialog.BaseName;
			ErrorReporter.Instance.AutomaticallySendEnabled = canAutomaticallyErrorSend;
			ErrorReporter.Instance.SendedLogRowCount = errorSendSettings.RowCountForErrorLog;

			var errorMessageModelFactoryWithUserService = new DefaultErrorMessageModelFactory(ErrorReporter.Instance, ServicesConfig.UserService, ServicesConfig.UnitOfWorkFactory);
			exceptionHandler.InteractiveService = ServicesConfig.InteractiveService;
			exceptionHandler.ErrorMessageModelFactory = errorMessageModelFactoryWithUserService;
			//Настройка обычных обработчиков ошибок.
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.NHibernateStaleObjectStateExceptionHandler);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.MySqlExceptionConnectionTimeoutHandler);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.MySqlExceptionAuthHandler);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.SocketTimeoutException);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.MysqlCommandTimeoutException);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.GeoGroupVersionNotFoundException);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.SystemOutOfMemoryExceptionHandler);
			exceptionHandler.CustomErrorHandlers.Add(ErrorHandlers.DeliveryPointDistrictNotFoundException);

			#endregion

			passwordValidator = new PasswordValidator(
				new PasswordValidationSettingsFactory(ServicesConfig.UnitOfWorkFactory).GetPasswordValidationSettings()
			);

			//Настройка карты
			IGMapSettings gMapSettingss = AppDIContainer.Resolve<IGMapSettings>();

			GMapProvider.UserAgent = string.Format("{0}/{1} used GMap.Net/{2} ({3})",
				_applicationInfo.ProductName,
				_applicationInfo.Version.VersionToShortString(),
				Assembly.GetAssembly(typeof(GMapProvider)).GetName().Version.VersionToShortString(),
				Environment.OSVersion.VersionString
			);
			GMapProvider.Language = GMap.NET.LanguageType.Russian;

			var squidServer = string.Empty;

			try
			{
				using(var httpClient = new HttpClient())
				{
					squidServer = gMapSettingss.SquidServer;
					if(httpClient.GetAsync($"{squidServer}/squid-internal-static/icons/SN.png").Result.IsSuccessStatusCode)
					{
						GMapProvider.WebProxy = new WebProxy(gMapSettingss.SquidServer);
						_logger.LogInformation("Используется прокси сервер карт: {MapsProxyServerUrl}", squidServer);
					}
					else
					{
						_logger.LogWarning("Прокси сервер карт недоступен: {MapsProxyServerUrl}", squidServer);
					}
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Прокси сервер карт недоступен: {MapsProxyServerUrl}", squidServer);
			}

			PerformanceHelper.AddTimePoint("Закончена настройка карты.");

			DatePicker.CalendarFontSize = 16;
			DateRangePicker.CalendarFontSize = 16;

			PerformanceHelper.StartPointsGroup("Главное окно");

			var baseVersionChecker = new CheckBaseVersion(_applicationInfo, new ParametersService(Connection.ConnectionDB));
			if(baseVersionChecker.Check())
			{
				ServicesConfig.CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, baseVersionChecker.TextMessage, "Несовпадение версии");
				return;
			}
			QSMain.CheckServer(null); // Проверяем настройки сервера

			PerformanceHelper.AddTimePoint("Закончена загрузка параметров базы и проверка версии.");

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

			var settingsController = AppDIContainer.Resolve<ISettingsController>();
			var userRepository = AppDIContainer.Resolve<IUserRepository>();
			if(ChangePassword(applicationConfigurator, userRepository) && CanLogin())
			{
				StartMainWindow(LoginDialog.BaseName, settingsController, _wikiSettings);
			}
			else
			{
				return;
			}

			PermissionExtensionSingletonStore.AssembliesFilter = new[] { "QS", "Vodovoz" };

			PerformanceHelper.EndPointsGroup();

			PerformanceHelper.AddTimePoint("Закончен старт SAAS. Конец загрузки.");

			QSSaaS.Session.StartSessionRefresh();

			PerformanceHelper.AddTimePoint("Закончен старт SAAS. Конец загрузки.");
			PerformanceHelper.Main.PrintAllPoints(_logger);

			Gtk.Application.Run();
			QSSaaS.Session.StopSessionRefresh();
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
		private static bool ChangePassword(IApplicationConfigurator applicationConfigurator, IUserRepository userRepository)
		{
			ResponseType result;
			int currentUserId;
			IChangePasswordModel changePasswordModel;

			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
			{
				var currentUser = userRepository.GetCurrentUser(uow);
				if(currentUser is null)
				{
					throw new InvalidOperationException("CurrentUser is null");
				}
				if(!currentUser.NeedPasswordChange)
				{
					return true;
				}
				currentUserId = currentUser.Id;

				if(!(Connection.ConnectionDB is MySqlConnection mySqlConnection))
				{
					throw new InvalidOperationException($"Текущее подключение не является {nameof(MySqlConnection)}");
				}

				var mySqlPasswordRepository = new MySqlPasswordRepository();
				changePasswordModel = new MysqlChangePasswordModelExtended(applicationConfigurator, mySqlConnection, mySqlPasswordRepository);
				var changePasswordViewModel = new ChangePasswordViewModel(
					changePasswordModel,
					passwordValidator,
					null
				);
				var changePasswordView = new ChangePasswordView(changePasswordViewModel)
				{
					Title = "Требуется сменить пароль"
				};
				changePasswordView.ShowAll();
				result = (ResponseType)changePasswordView.Run();
				changePasswordView.Destroy();
			}

			if(result == ResponseType.Ok && changePasswordModel.PasswordWasChanged)
			{
				using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
				{
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
			ISettingsController settingsController,
			IWikiSettings wikiSettings)
		{
			//Настрока удаления
			Configure.ConfigureDeletion();
			PerformanceHelper.AddTimePoint("Закончена настройка удаления");

			DriverApiSettings.InitializeNotifications(settingsController, loginDialogName);

			CreateTempDir();

			//Запускаем программу
			MainWin = new MainWindow(
				AppDIContainer.Resolve<IInteractiveService>(),
				AppDIContainer.Resolve<IApplicationInfo>(),
				wikiSettings);
			
			MainWin.Configure();
			MainWin.InitializeManagers();
			MainWin.Title += $" (БД: {loginDialogName})";
			QSMain.ErrorDlgParrent = MainWin;
			MainWin.Show();
		}

		private static bool CanLogin()
		{
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
			{
				var dBLogin = ServicesConfig.CommonServices.UserService.GetCurrentUser().Login;

				string sid = "";
				// Получение данных пользователя системы
				if(Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					var windowsIdentity = WindowsIdentity.GetCurrent();
					sid = windowsIdentity.User?.ToString() ?? "";
				}

				RegisteredRM registeredRMAlias = null;
				var rm = uow.Session.QueryOver<RegisteredRM>(() => registeredRMAlias).Where(x => x.SID == sid && x.IsActive).List().FirstOrDefault();

				return (rm == null) || rm.Users.Any(u => u.Login == dBLogin);
			}
		}

		private void DeletedEvent(object o, DeleteEventArgs args)
		{
			Gtk.Application.Quit();
		}
	}
}

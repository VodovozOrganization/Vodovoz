using System;
using System.Data.Common;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using NHibernate;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.DB.EntityMappingConfig;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using Vodovoz.Domain.Employees;
using VodovozInfrastructure.ConnectionStringPipeListener;
using WhereIsTheBottle.Infrastructure;
using WhereIsTheBottle.Models;
using WhereIsTheBottle.ViewModels;
using WhereIsTheBottle.Views;

namespace WhereIsTheBottle.Startup
{
	public class Main
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly ILifetimeScope _startupScope;
		private readonly string[] _commandLineArgs;

		public Main(ILifetimeScope startupScope)
		{
			_startupScope = startupScope ?? throw new ArgumentNullException(nameof(startupScope));
			_commandLineArgs = Environment.GetCommandLineArgs();
		}

		public void StartApplication()
		{
			_logger.Debug("Запуск приложения \"Где бутыль?\"...");
			OpenLoginDialog();
		}

		private void OpenLoginDialog()
		{
			ILifetimeScope rootScope;
			BottleAnalyticsView bottleAnalyticsView;

			try
			{
				_logger.Debug("Открытие окна логина...");

				var loginModel = _startupScope.Resolve<ILoginModel>();
				var loginViewModel = _startupScope.Resolve<LoginViewModel>();
				var loginView = _startupScope.Resolve<LoginView>();

				loginModel.SetOverwriteConnectionFromArgs(_commandLineArgs);
				loginView.WindowStartupLocation = WindowStartupLocation.CenterScreen;

				if(IsAutoLoginStart(_commandLineArgs))
				{
					TryToAutoLoginOnBackground(_commandLineArgs, loginModel, loginViewModel, loginView);
				}
				Application.Current.MainWindow = loginView;
				loginView.ShowDialog();

				if(!loginModel.SuccessfullyLoggedIn)
				{
					return;
				}
				_logger.Debug("Подключено");

				rootScope = _startupScope.BeginLifetimeScope(App.RegisterStartupComponents);

				ConfigureUserService(
					rootScope.Resolve<IUnitOfWorkFactory>(),
					rootScope.Resolve<IEntityMappingConfigProvider>(),
					loginModel.ActiveConnection.Login
				);
				bottleAnalyticsView = rootScope.Resolve<BottleAnalyticsView>();
				var bottleAnalyticsViewModel = rootScope.Resolve<BottleAnalyticsViewModel>();

				_ = bottleAnalyticsViewModel.InititalizeAsync();
			}
			catch(Exception ex)
			{
				Dispatcher.CurrentDispatcher.Invoke(() => throw new TerminatingException(ex));
				return;
			}

			Application.Current.MainWindow = bottleAnalyticsView;
			bottleAnalyticsView.ShowDialog();
			rootScope.Dispose();
		}

		private void TryToAutoLoginOnBackground(string[] commandLineArgs, ILoginModel loginModel, LoginViewModel loginViewModel,
			LoginView loginView)
		{
			var connectionStringListener = new PipeStreamConnectionStringListener();

			connectionStringListener.Failed += (sender, eventArgs) =>
			{
				if(!loginModel.SuccessfullyLoggedIn && !loginModel.ConnectionInProgress)
				{
					loginViewModel.ErrorMessage = "Авто-вход:\n" + eventArgs.Message;
					loginViewModel.TaskInProgress = false;
					loginViewModel.StatusMessage = "";
				}
			};
			connectionStringListener.GotConnectionString += (sender, eventArgs) =>
			{
				if(!loginModel.SuccessfullyLoggedIn && !loginModel.ConnectionInProgress)
				{
					loginViewModel.TaskInProgress = true;

					try
					{
						var connectionBuilder = new DbConnectionStringBuilder { ConnectionString = eventArgs.ConnectionString };
						var connection = loginModel.GetOrCreateConnection(
							connectionBuilder["server"].ToString(),
							connectionBuilder["database"].ToString(),
							connectionBuilder["user id"].ToString());
						loginModel.Connect(connection, connectionBuilder["password"].ToString().ToSecureString());
						_logger.Debug("Автоматическое подключение успешно");
						loginModel.SuccessfullyLoggedIn = true;
						loginView.Dispatcher.Invoke(loginView.Close);
					}
					catch(Exception ex)
					{
						_logger.Error(ex);
						loginViewModel.ErrorMessage = "Ошибка при попытке авто-входа:\n" + ex.Message;
						loginViewModel.TaskInProgress = false;
						loginViewModel.StatusMessage = "";
					}
				}
			};
			loginViewModel.StatusMessage = "Автоматическое подключение...";
			connectionStringListener.SetupPipeStream(commandLineArgs, 1000);
		}

		private bool IsAutoLoginStart(string[] args)
		{
			return args.Any(x => x is "-autologin" or "-auto");
		}

		private void ConfigureUserService(IUnitOfWorkFactory unitOfWorkFactory, IEntityMappingConfigProvider configProvider, string login)
		{
			using var uow = unitOfWorkFactory.CreateWithoutRoot();

			var userConfig = configProvider.GetEntityMappingConfig<User>();

			var query =
				$"SELECT {userConfig.PropertyNames[nameof(User.Id)]} as id FROM {userConfig.TableName} WHERE {userConfig.PropertyNames[nameof(User.Login)]} = '{login}'";

			var currentUserId = uow.Session.CreateSQLQuery(query).AddScalar("id", NHibernateUtil.Int32).List<int>().First();
			ServicesConfig.UserService = new UserService(currentUserId);
		}
	}
}

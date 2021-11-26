using System;
using System.Collections.ObjectModel;
using System.Security;
using System.Threading.Tasks;
using NLog;
using QS.MachineConfig;
using QS.Utilities.Text;
using QS.ViewModels;
using WhereIsTheBottle.Commands;
using WhereIsTheBottle.Database;
using WhereIsTheBottle.Models;

namespace WhereIsTheBottle.ViewModels
{
	public sealed class LoginViewModel : ViewModelBase
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private ObservableCollection<Connection> _connections;
		private string _errorMessage;
		public Action OpenEditConnectionsWindow;
		private SecureString _password;
		private Connection _selectedConnection;
		private bool _showSpinner;
		private string _statusMessage;
		private bool _taskInProgress;
		private RelayCommand _confirmLogInCommand;
		private RelayCommand _openEditConnectionsWindowCommand;

		public LoginViewModel(ILoginModel loginModel)
		{
			LoginModel = loginModel ?? throw new ArgumentNullException(nameof(loginModel));

			TaskInProgress = false;
			RefreshConnections();

			loginModel.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == nameof(loginModel.ConnectionInProgress))
				{
					OnPropertyChanged(nameof(TaskInProgress));
				}
			};
		}

		public Action Close;

		public ILoginModel LoginModel { get; }
		public bool ShowSpinner
		{
			get => _showSpinner || TaskInProgress;
			set => SetField(ref _showSpinner, value);
		}
		public ObservableCollection<Connection> Connections
		{
			get => _connections;
			set => SetField(ref _connections, value);
		}
		public Connection SelectedConnection
		{
			get => _selectedConnection;
			set => SetField(ref _selectedConnection, value);
		}
		public SecureString Password
		{
			get => _password;
			set => SetField(ref _password, value);
		}
		public string ErrorMessage
		{
			get => _errorMessage;
			set => SetField(ref _errorMessage, value);
		}
		public string StatusMessage
		{
			get => _statusMessage;
			set => SetField(ref _statusMessage, value);
		}

		public string ApplicationInfoStr
		{
			get
			{
				var appInfo = LoginModel.ApplicationInfo;
				return appInfo == null ? "" : $"{appInfo.ProductTitle} v.{appInfo.Version.VersionToShortString()}";
			}
		}

		public string BuildTimeStr
		{
			get
			{
				var appInfo = LoginModel.ApplicationInfo;
				return appInfo == null ? "" : $"{appInfo.BuildDate:dd.MM.yyyy HH:mm}";
			}
		}
		public bool TaskInProgress
		{
			get => _taskInProgress || LoginModel.ConnectionInProgress;
			set
			{
				if(SetField(ref _taskInProgress, value))
				{
					OnPropertyChanged(nameof(ShowSpinner));
				}
			}
		}

		public void RefreshConnections()
		{
			LoginModel.ReloadConnections();
			Connections = new ObservableCollection<Connection>(LoginModel.Connections);
			SelectedConnection = LoginModel.GetDefaultConnection();
		}

		#region Commands

		public RelayCommand OpenEditConnectionsWindowCommand => _openEditConnectionsWindowCommand ??= new RelayCommand(
			() => OpenEditConnectionsWindow?.Invoke()
		);

		public RelayCommand ConfirmLogInCommand => _confirmLogInCommand ??= new RelayCommand(
			async () =>
			{
				try
				{
					TaskInProgress = true;
					ErrorMessage = "";
					StatusMessage = "Соединение...";
					var task = Task.Run(() => LoginModel.Connect(SelectedConnection, Password));
					await task;
					SelectedConnection.IsDefault = true;
					LoginModel.SaveConnection(SelectedConnection);
					Close?.Invoke();
				}
				catch(Exception ex)
				{
					StatusMessage = "Ошибка";
					ErrorMessage = ex.Message;
					_logger.Error(ex);
					throw;
				}
				finally
				{
					TaskInProgress = false;
				}
			},
			() => !TaskInProgress
		);

		#endregion
	}
}

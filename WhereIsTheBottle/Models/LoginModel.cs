using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using MySql.Data.MySqlClient;
using QS.DomainModel.Entity;
using QS.ErrorReporting;
using QS.MachineConfig;
using QS.Project.Versioning;
using WhereIsTheBottle.Database;
using WhereIsTheBottle.Infrastructure;

namespace WhereIsTheBottle.Models
{
	public class LoginModel : PropertyChangedBase, ILoginModel
	{
		private readonly IBaseConnector _baseConnector;
		private readonly IConnectionManager _connectionManager;
		private bool _connectionInProgress;
		private IList<Connection> _connections;

		public LoginModel(IConnectionManager connectionManager, IBaseConnector baseConnector, IApplicationInfo applicationInfo)
		{
			_connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
			ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			_baseConnector = baseConnector ?? throw new ArgumentNullException(nameof(baseConnector));
			SuccessfullyLoggedIn = false;
		}

		public IApplicationInfo ApplicationInfo { get; }
		public string OverwriteConnectionName { get; set; }
		public bool SuccessfullyLoggedIn { get; set; }

		public Connection ActiveConnection { get; private set; }

		public bool ConnectionInProgress
		{
			get => _connectionInProgress;
			set => SetField(ref _connectionInProgress, value);
		}

		public IList<Connection> Connections
		{
			get
			{
				if(_connections == null)
				{
					InitializeConnections();
				}
				return _connections;
			}
		}

		public void Connect(Connection connection, SecureString password)
		{
			try
			{
				ConnectionInProgress = true;
				ActiveConnection = null;
				_baseConnector.Connect(connection.Server, connection.BaseName, connection.Login, password);
				ActiveConnection = connection;
				SuccessfullyLoggedIn = true;
			}
			catch(Exception ex)
			{
				string message = "Ошибка соединения с базой данных";
				var mySqlExceptions = ex.FindAllExceptionsWithTypeInInner<MySqlException>();
				if(mySqlExceptions.Any(x => x.Number is 1045 or 0))
				{
					message = "Доступ запрещен. Проверьте логин и пароль.";
				}
				else if(mySqlExceptions.Any(x => x.Number is 1042))
				{
					message = "Не удалось подключиться к серверу БД.";
				}
				throw new Exception(message, ex);
			}
			finally
			{
				ConnectionInProgress = false;
			}
		}

		public void ReloadConnections()
		{
			_connections = null;
			InitializeConnections();
		}

		public Connection GetDefaultConnection()
		{
			return Connections.FirstOrDefault(x => x.ConnectionName == OverwriteConnectionName)
				?? Connections.FirstOrDefault(x => x.IsDefault)
				?? Connections.FirstOrDefault();
		}

		public void SaveConnection(Connection connection)
		{
			_connectionManager.SaveConnection(connection);
		}

		public void SetOverwriteConnectionFromArgs(string[] args)
		{
			for(var i = 0; i < args.Length - 1; i++)
			{
				if(args[i] == "-d" || args[i].ToLower() == "--default" || args[i].ToLower() == "-default")
				{
					OverwriteConnectionName = args[i + 1];
				}
			}
		}

		public Connection GetOrCreateConnection(string server, string baseName, string user)
		{
			if(String.IsNullOrWhiteSpace(server))
			{
				throw new ArgumentException("Передано пустое значение строки или null", nameof(server));
			}
			if(String.IsNullOrWhiteSpace(baseName))
			{
				throw new ArgumentException("Передано пустое значение строки или null", nameof(baseName));
			}
			if(String.IsNullOrWhiteSpace(user))
			{
				throw new ArgumentException("Передано пустое значение строки или null", nameof(user));
			}

			var existing = Connections.FirstOrDefault(x => x.Server == server && x.BaseName == baseName && x.Login == user);
			if(existing == null)
			{
				string connName = null;
				var dbNameSuffix = baseName.Split('_').LastOrDefault();
				if(!String.IsNullOrWhiteSpace(dbNameSuffix))
				{
					connName = Char.ToUpper(dbNameSuffix.First()) + dbNameSuffix.Substring(1).ToLower();
				}

				existing = _connectionManager.CreateConnection(
					connName,
					server,
					baseName,
					user
				);
				_connections.Add(existing);
				_connectionManager.SaveConnection(existing);
			}
			return existing;
		}

		private void InitializeConnections()
		{
			_connections = _connectionManager.LoadConnections();
			if(_connections.Any())
			{
				return;
			}
			var newConnection = _connectionManager.CreateConnection(isDefault: true);
			_connectionManager.SaveConnection(newConnection);
			_connections.Add(newConnection);
		}
	}
}

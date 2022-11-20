using MySql.Data.MySqlClient;
using NLog;
using QS.Project.Versioning;
using QSProjectsLib;
using System;
using Vodovoz.Services;
using VodovozInfrastructure.Connections;

namespace Vodovoz.Infrastructure
{
	public class ConnectionStringProvider : IConnectionStringProvider
	{
		ILogger _logger = LogManager.GetCurrentClassLogger();
		private readonly IGlobalSettings _globalSettings;
		private readonly IDataBaseInfo _dataBaseInfo;
		private MySqlConnectionStringBuilder _mySqlConnectionStringBuilder;

		public ConnectionStringProvider(IGlobalSettings globalSettings, IDataBaseInfo dataBaseInfo)
		{
			_globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
			_dataBaseInfo = dataBaseInfo ?? throw new ArgumentNullException(nameof(dataBaseInfo));
		}

		public string MasterConnectionString => GetConnectionString();

		public string SlaveConnectionString
		{
			get
			{
				var slaveEnabled = _globalSettings.SlaveConnectionEnabled
					&& _dataBaseInfo.Name == _globalSettings.SlaveConnectionEnabledForThisDatabase;

				var slaveHost = _globalSettings.SlaveConnectionHost;
				var slavePort = _globalSettings.SlaveConnectionPort;
				var hasSlaveHost = !string.IsNullOrWhiteSpace(slaveHost) && slavePort > 0;
				if(!hasSlaveHost && slaveEnabled)
				{
					_logger.Warn($"Для Slave подключения будет использовано Master подключение, так как Slave " +
						$"подключение настроено не правильно или не настроено: {slaveHost}:{slavePort} (Host:port)");
				}

				if(!slaveEnabled || !hasSlaveHost)
				{
					return MasterConnectionString;
				}

				SlaveConnectionStringBuilder.Server = slaveHost;
				SlaveConnectionStringBuilder.Port = (uint)slavePort;

				var connected = TestConnection(SlaveConnectionStringBuilder.ConnectionString);
				if(connected)
				{
					return SlaveConnectionStringBuilder.ConnectionString;
				}
				else
				{
					return MasterConnectionString;
				}
			}
		}

		private MySqlConnectionStringBuilder SlaveConnectionStringBuilder
		{
			get
			{
				if(_mySqlConnectionStringBuilder == null)
				{
					var connectionString = GetConnectionString();
					_mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
				}
				return _mySqlConnectionStringBuilder;
			}
		}

		private string GetConnectionString()
		{
			if(QSMain.ConnectionStringBuilder == null)
			{
				throw new InvalidOperationException("Подключение еще не было настроено, получение строки подключения невозможно.");
			}
			return QSMain.ConnectionStringBuilder.ConnectionString;
		}

		private bool TestConnection(string connectionString)
		{
			using(MySqlConnection connection = new MySqlConnection(connectionString))
			{
				try
				{
					connection.Open();
					return true;
				}
				catch(Exception ex)
				{
					_logger.Error(ex, "Возникло исключение при проверки соединения к slave серверу.");
					return false;
				}
			}
		}
	}
}

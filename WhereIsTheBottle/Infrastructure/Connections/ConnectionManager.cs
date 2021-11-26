using System;
using System.Collections.Generic;
using System.Linq;
using QS.MachineConfig;
using QS.MachineConfig.Configuration;

namespace WhereIsTheBottle.Infrastructure.Connections
{
	public class ConnectionManager : IConnectionManager
	{
		private readonly IConfigurationManager _configurationManager;

		private string _defaultConnectionName;
		private string _defaultDatabaseName;
		private string _defaultLogin;
		private string _defaultServer;

		public ConnectionManager(IConfigurationManager configurationManager, IDefaultConnectionSettings connectionSettings = null)
		{
			_configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
			_defaultConnectionName = connectionSettings?.ConnectionName;
			_defaultDatabaseName = connectionSettings?.DatabaseName;
			_defaultLogin = connectionSettings?.Login;
			_defaultServer = connectionSettings?.Server;
		}

		public IList<Connection> LoadConnections()
		{
			return _configurationManager.GetConfiguration().Connections;
		}

		public void SetDefaultConnectionNames(string baseName, string connectionName, string server, string userLogin)
		{
			_defaultDatabaseName = baseName;
			_defaultConnectionName = connectionName;
			_defaultServer = server;
			_defaultLogin = userLogin;
		}

		public Connection CreateConnection(
			string overrideConnectionName = null,
			string overrideServer = null,
			string overrideBaseName = null,
			string overrideUserLogin = null,
			bool isDefault = false,
			int overrideId = 0)
		{
			return new Connection
			{
				ConnectionName = overrideConnectionName ?? _defaultConnectionName,
				Server = overrideServer ?? _defaultServer,
				BaseName = overrideBaseName ?? _defaultDatabaseName,
				Login = overrideUserLogin ?? _defaultLogin,
				IsDefault = isDefault,
				Id = overrideId
			};
		}

		public void SaveConnection(Connection connection)
		{
			var config = _configurationManager.GetConfiguration();

			if(connection.IsDefault && config.Connections.FirstOrDefault(x => x.IsDefault) is { } currentDefaultConnection)
			{
				currentDefaultConnection.IsDefault = false;
			}

			if(connection.Id == 0)
			{
				connection.Id = GetNewId(config.Connections);
			}

			if(config.Connections.FirstOrDefault(x => x.Id == connection.Id) is { } oldConnection)
			{
				config.Connections.Remove(oldConnection);
			}
			config.Connections.Add(connection);
			_configurationManager.SaveConfigration(config);
		}

		public void SetConnections(IList<Connection> connections)
		{
			bool defaultFound = false;
			foreach(var connection in connections)
			{
				if(connection.Id == 0)
				{
					connection.Id = GetNewId(connections);
				}
				if(defaultFound)
				{
					connection.IsDefault = false;
				}
				else
				{
					defaultFound = connection.IsDefault;
				}
			}
			var config = _configurationManager.GetConfiguration();
			config.Connections = connections;
			_configurationManager.SaveConfigration(config);
		}

		private static int GetNewId(IList<Connection> allConnections)
		{
			if(allConnections == null || !allConnections.Any())
			{
				return 1;
			}
			return allConnections.Max(x => x.Id) + 1;
		}
	}
}

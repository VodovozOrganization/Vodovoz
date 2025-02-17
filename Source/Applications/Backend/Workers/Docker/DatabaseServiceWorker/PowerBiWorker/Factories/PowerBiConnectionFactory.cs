using System.Data;
using MySqlConnector;
using QS.Project.DB;

namespace DatabaseServiceWorker.PowerBiWorker.Factories
{
	internal class PowerBiConnectionFactory : IPowerBiConnectionFactory
	{
		public MySqlConnection CreateConnection(IDatabaseConnectionSettings dateBaseSettings)
		{
			var connectionBuilder = new MySqlConnectionStringBuilder
			{
				Port = dateBaseSettings.Port,
				Server = dateBaseSettings.ServerName,
				UserID = dateBaseSettings.UserName,
				Password = dateBaseSettings.Password,
				Database = dateBaseSettings.DatabaseName
			};

			var connection = new MySqlConnection(connectionBuilder.ToString());

			if(connection.State != ConnectionState.Open)
			{
				connection.Open();
			}

			return connection;
		}
	}
}

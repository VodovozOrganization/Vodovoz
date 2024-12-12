using MySqlConnector;
using QS.Project.DB;

namespace DatabaseServiceWorker.PowerWorker.Helpers
{
	internal class PowerBiConnectionFactory : IPowerBiConnectionFactory
	{
		public MySqlConnection CreateConnection(IDatabaseConnectionSettings dateBasesettings)
		{
			var connectionBuilder = new MySqlConnectionStringBuilder();
			connectionBuilder.Port = dateBasesettings.Port;
			connectionBuilder.Server = dateBasesettings.ServerName;
			connectionBuilder.UserID = dateBasesettings.UserName;
			connectionBuilder.Password = dateBasesettings.Password;
			connectionBuilder.Database = dateBasesettings.DatabaseName;

			var connection = new MySqlConnection(connectionBuilder.ToString());

			if(connection.State != System.Data.ConnectionState.Open)
			{
				connection.Open();
			}

			return connection;
		}
	}
}

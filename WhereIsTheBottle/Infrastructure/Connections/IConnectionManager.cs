using System.Collections.Generic;
using QS.MachineConfig;

namespace WhereIsTheBottle.Infrastructure
{
	public interface IConnectionManager
	{
		IList<Connection> LoadConnections();

		void SetDefaultConnectionNames(string baseName, string connectionName, string server, string userLogin);

		Connection CreateConnection(
			string overrideConnectionName = null,
			string overrideServer = null,
			string overrideBaseName = null,
			string overrideUserLogin = null,
			bool isDefault = false,
			int overrideId = 0);

		void SaveConnection(Connection connection);

		void SetConnections(IList<Connection> connections);
	}
}

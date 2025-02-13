using MySqlConnector;
using QS.Project.DB;

namespace DatabaseServiceWorker.PowerBiWorker.Factories
{
	public interface IPowerBiConnectionFactory
	{
		MySqlConnection CreateConnection(IDatabaseConnectionSettings dateBaseSettings);
	}
}

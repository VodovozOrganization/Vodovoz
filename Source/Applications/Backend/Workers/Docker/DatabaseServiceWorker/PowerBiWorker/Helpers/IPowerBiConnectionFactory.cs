using MySqlConnector;
using QS.Project.DB;

namespace DatabaseServiceWorker.PowerWorker.Helpers
{
	interface IPowerBiConnectionFactory
	{
		MySqlConnection CreateConnection(IDatabaseConnectionSettings dateBasesettings);
	}
}

using Dapper;
using MailganerEventsDistributorApi.DTO;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;
using System.Linq;

namespace MailganerEventsDistributorApi.DataAccess
{
	public class InstanceData : IInstanceData
	{
		private readonly IConfiguration _configuration;

		public InstanceData(IConfiguration configuration)
		{
			_configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
		}

		public InstanceDto GetInstanceByDatabaseId(int Id)
		{
			string connectionString = _configuration.GetConnectionString("Default");

			using IDbConnection connection = new MySqlConnection(connectionString);
			var output = connection.Query<InstanceDto>(
				"SELECT\n" +
				"	i.id AS Id,\n" +
				"	i.database_id AS DatabaseId,\n" +
				"	i.message_broker_host AS MessageBrockerHost,\n" +
				"	i.message_broker_virtual_host AS MessageBrockerVirtualHost,\n" +
				"	i.port AS Port\n" +
				"FROM instances i\n" +
				$"WHERE i.database_id = { Id };"
				).ToList().LastOrDefault();

			return output;
		}
	}
}

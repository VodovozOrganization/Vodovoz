using System.Security;

namespace WhereIsTheBottle.Database
{
	public interface IBaseConnector
	{
		public void Connect(string server, string databaseName, string user, SecureString password);

		public void Connect(string connectionString);

		public bool TryConnect(string server, string databaseName, string user, SecureString password);

		public bool TryConnect(string connectionString);
	}
}
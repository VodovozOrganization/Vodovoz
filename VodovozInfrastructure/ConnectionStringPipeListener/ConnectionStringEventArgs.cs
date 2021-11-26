using System;

namespace VodovozInfrastructure.ConnectionStringPipeListener
{
	public class ConnectionStringEventArgs : EventArgs
	{
		public ConnectionStringEventArgs(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public string ConnectionString { get; }
	}
}
using MySqlConnector;

namespace Vodovoz.Data.NHibernate.Options
{
	public class DatabaseUserSettings
	{
		/// <summary>
		/// Database server url
		/// </summary>
		public string ServerName { get; set; }

		/// <summary>
		/// Server port
		/// </summary>
		public uint Port { get; set; }

		/// <summary>
		/// Database name
		/// </summary>
		public string DatabaseName { get; set; }

		/// <summary>
		/// Database username
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// Database user password
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// SSL mode
		/// </summary>
		public MySqlSslMode MySqlSslMode { get; set; }
	}
}

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Версия MariaDB для тестов. Совпадает с версией на проде, меняется вместе с prod-server.cnf.
	/// </summary>
	public static class MariaDbVersion
	{
		public const string Version = "11.2.5";
		public const string ArchiveName = "mariadb-11.2.5-winx64";
		public const string ArchiveUrl = "https://archive.mariadb.org/mariadb-11.2.5/winx64-packages/mariadb-11.2.5-winx64.zip";
		public const string ArchiveHost = "archive.mariadb.org";
		public const string ArchiveSha256 = "72f84a6f9983e4fe2fdeb048fdd0d372f96c9d178d4b7460d96ec0d70e96cfe7";
	}
}

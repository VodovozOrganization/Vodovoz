namespace WhereIsTheBottle.Infrastructure.Connections
{
	internal class DefaultConnectionSettings : IDefaultConnectionSettings
	{
		public string ConnectionName { get; set; } = "По умолчанию";
		public string Login { get; set; }
		public string DatabaseName { get; set; } = "Vodovoz_";
		public string Server { get; set; } = "sql.vod.qsolution.ru";
	}
}

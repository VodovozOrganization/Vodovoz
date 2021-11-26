namespace WhereIsTheBottle.Infrastructure.Connections
{
	public interface IDefaultConnectionSettings
	{
		string ConnectionName { get; set; }
		string DatabaseName { get; set; }
		string Login { get; set; }
		string Server { get; set; }
	}
}

namespace VodovozInfrastructure.Connections
{
	public interface IConnectionStringProvider
	{
		string MasterConnectionString { get; }
		string SlaveConnectionString { get; }
	}
}

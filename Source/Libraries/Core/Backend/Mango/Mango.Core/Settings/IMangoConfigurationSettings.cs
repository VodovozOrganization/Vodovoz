namespace Mango.Core.Settings
{
	public interface IMangoConfigurationSettings
	{
		string VpbxApiKey { get; }
		string VpbxApiSalt { get; }
		GrpcConnectionSettings GrpcConnectionSettings { get; }
	}
}

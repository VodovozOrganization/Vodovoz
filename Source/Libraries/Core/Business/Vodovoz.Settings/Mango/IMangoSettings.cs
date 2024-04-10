namespace Vodovoz.Settings.Mango
{
	public interface IMangoSettings
	{
		string ServiceHost { get; }
		uint ServicePort { get; }
		string VpbxApiKey { get; }
		string VpbxApiSalt { get; }
		bool MangoEnabled { get; }
		bool TestMode { get; }

		int GrpcKeepAliveTimeMs { get; }
		int GrpcKeepAliveTimeoutMs { get; }
		bool GrpcKeepAlivePermitWithoutCalls { get; }
		int GrpcMaxPingWithoutData { get; }
	}
}

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

		/// <summary>
		/// Номер линии Манго, предназначенный для связи водителя с клиентом
		/// </summary>
		string DriversCallsLineNumber { get; }

		/// <summary>
		/// URL для совершения звонков через вебхук Манго
		/// </summary>
		string WebhookCallsUrl { get; }
	}
}

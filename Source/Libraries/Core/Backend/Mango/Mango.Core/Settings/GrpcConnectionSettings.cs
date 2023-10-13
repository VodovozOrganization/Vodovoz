namespace Mango.Core.Settings
{
	public class GrpcConnectionSettings
	{
		public int Port { get; set; }
		public int KeepAliveTimeMs { get; set; }
		public int KeepAliveTimeoutMs { get; set; }
		public bool KeepAlivePermitWithoutCalls { get; set; }
		public int MaxPingWithoutData { get; set; }
		public int MinTimeBetweenPingsMs { get; set; }
		public int MaxPingStrikes { get; set; }
	}
}

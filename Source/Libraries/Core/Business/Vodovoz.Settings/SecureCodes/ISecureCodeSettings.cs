namespace Vodovoz.Settings.SecureCodes
{
	public interface ISecureCodeSettings
	{
		int TimeForNextCodeSeconds { get; }
		int CodeLifetimeSeconds { get; }
	}
}

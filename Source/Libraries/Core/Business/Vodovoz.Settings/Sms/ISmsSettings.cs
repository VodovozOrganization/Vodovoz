namespace Vodovoz.Settings.Sms
{
	public interface ISmsSettings
    {
		string InternalSmsServiceUrl { get; }
		string InternalSmsServiceApiKey { get; }
		bool SmsSendingAllowed { get; }
	}
}

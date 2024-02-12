namespace Sms.Internal.Client.Framework
{
	public interface ISmsClientChannelFactory
	{
		SmsClientChannel OpenChannel(string url = null);
	}
}
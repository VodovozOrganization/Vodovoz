namespace Pacs.Server
{
	public interface IBreakAvailabilityNotifier
	{
		void NotifyBreakAvailability(bool breakAvailable);
	}
}

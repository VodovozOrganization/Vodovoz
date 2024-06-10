using Pacs.Core.Messages.Events;

namespace Pacs.Server.Breaks
{
	public interface IBreakAvailabilityNotifier
	{
		void NotifyGlobalBreakAvailability(GlobalBreakAvailabilityEvent breakAvailability);
		void NotifyOperatorsOnBreak(OperatorsOnBreakEvent operatorsOnBreak);
	}
}

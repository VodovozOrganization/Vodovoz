using Pacs.Core.Messages.Events;
using System;

namespace Pacs.Server.Breaks
{
	public interface IOperatorBreakAvailabilityService
	{
		event EventHandler<OperatorBreakAvailability> BreakAvailabilityChanged;

		OperatorBreakAvailability GetBreakAvailability(int operatorId);
		void WarmUpCacheForOperatorIds(params int[] operatorIds);
	}
}

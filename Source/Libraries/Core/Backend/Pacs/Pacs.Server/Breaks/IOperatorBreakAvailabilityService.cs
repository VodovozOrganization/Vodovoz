using Pacs.Core.Messages.Events;
using System;

namespace Pacs.Server.Breaks
{
	public interface IOperatorBreakAvailabilityService
	{
		OperatorBreakAvailability GetBreakAvailability(int operatorId);
	}
}

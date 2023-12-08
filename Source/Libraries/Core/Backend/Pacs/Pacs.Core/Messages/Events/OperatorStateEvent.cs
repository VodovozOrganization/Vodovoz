using System;
using System.Collections.Generic;
using System.Text;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Events
{
	public class OperatorStateEvent
	{
		public OperatorState State { get; set; }
		public OperatorBreakAvailability BreakAvailability { get; set; }
	}
}

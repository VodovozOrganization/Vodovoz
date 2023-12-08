using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Events
{
	public class OperatorsOnBreakEvent
	{
		public IEnumerable<OperatorState> OnBreak { get; set; } = Enumerable.Empty<OperatorState>();
	}
}

using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Events
{
	/// <summary>
	/// Событие ухода на перерыв оператора
	/// </summary>
	public class OperatorsOnBreakEvent : EventBase
	{
		/// <summary>
		/// Состояния оператора
		/// </summary>
		public IEnumerable<OperatorState> OnBreak { get; set; } = Enumerable.Empty<OperatorState>();
	}
}

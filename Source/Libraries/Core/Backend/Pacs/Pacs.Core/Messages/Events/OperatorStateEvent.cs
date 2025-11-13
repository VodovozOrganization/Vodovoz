using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Events
{
	/// <summary>
	/// Событие смены состояния оператора
	/// </summary>
	public class OperatorStateEvent : EventBase
	{
		/// <summary>
		/// Состояние оператора
		/// </summary>
		public OperatorState State { get; set; }

		/// <summary>
		/// Доступность перерывов
		/// </summary>
		public OperatorBreakAvailability BreakAvailability { get; set; }
	}
}

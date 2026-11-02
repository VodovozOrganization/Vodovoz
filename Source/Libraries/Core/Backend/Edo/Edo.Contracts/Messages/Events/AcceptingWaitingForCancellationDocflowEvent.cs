using TransactionalOutbox.Contracts;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие для подписания входящего документооборота
	/// </summary>
	public class AcceptingWaitingForCancellationDocflowEvent : IIdempotentOutboxMessage
	{
		public const string Event = "accepting-waiting-for-cancellation-docflow-event";

		/// <summary>
		/// Код кабинета в ЭДО
		/// </summary>
		public string EdoAccount { get; set; }

		/// <summary>
		/// Id документооборота
		/// </summary>
		public string DocFlowId { get; set; }

		public int GetAggregateId()
		{
			return DocFlowId.GetHashCode();
		}

		public string GetDeduplicationKey()
		{
			return $"Event={Event}" +
			$"AggregateId={GetAggregateId()}" +
			$"EdoAccount={EdoAccount}" +
			$":{nameof(AcceptingWaitingForCancellationDocflowEvent)}";
		}
	}
}

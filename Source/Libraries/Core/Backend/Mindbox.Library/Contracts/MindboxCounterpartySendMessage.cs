using System;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Сообщение очереди отправки контрагента в Mindbox.
	/// Публикуется в outbox шины online_orders, обрабатывается воркером отправки.
	/// </summary>
	public class MindboxCounterpartySendMessage
	{
		/// <summary>
		/// Идентификатор контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Ключ идемпотентности операции. При повторах должен оставаться тем же,
		/// чтобы Mindbox не создал дубликат.
		/// </summary>
		public Guid TransactionId { get; set; }
	}
}

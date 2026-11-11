using System;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Сообщение очереди синхронизации онлайн-заказа с Mindbox
	/// </summary>
	public sealed class MindboxOnlineOrderSyncMessage
	{
		public int OnlineOrderId { get; set; }
		public Guid TransactionId { get; set; }
	}
}

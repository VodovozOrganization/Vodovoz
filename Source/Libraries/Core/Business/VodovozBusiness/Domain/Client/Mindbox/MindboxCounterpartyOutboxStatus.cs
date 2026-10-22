namespace Vodovoz.Domain.Client.Mindbox
{
	/// <summary>
	/// Статус сообщения очереди отправки контрагента в Mindbox
	/// </summary>
	public enum MindboxCounterpartyOutboxStatus
	{
		/// <summary>
		/// Ожидает отправки
		/// </summary>
		Pending = 0,

		/// <summary>
		/// Отправлено
		/// </summary>
		Sent = 1,

		/// <summary>
		/// Исчерпаны попытки отправки
		/// </summary>
		Failed = 2
	}
}

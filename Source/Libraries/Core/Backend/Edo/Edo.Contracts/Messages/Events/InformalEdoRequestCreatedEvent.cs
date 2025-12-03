
namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие создания заявки на создание неформального документа заказа
	/// </summary>
	public class InformalEdoRequestCreatedEvent
	{
		/// <summary>
		/// Идентификатор заявки на создание неформального документа заказа
		/// </summary>
		public int InformalRequestId { get; set; }
	}
}

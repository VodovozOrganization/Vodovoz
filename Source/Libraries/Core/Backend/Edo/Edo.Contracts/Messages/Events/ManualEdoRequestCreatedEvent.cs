
namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие создания ручной заявки документа заказа
	/// </summary>
	public class ManualEdoRequestCreatedEvent
	{
		/// <summary>
		/// Идентификатор ручной заявки документа заказа
		/// </summary>
		public int ManualRequestId { get; set; }
	}
}

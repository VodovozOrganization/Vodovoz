namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие отправки документа
	/// </summary>
	public class OrderDocumentSentEvent
	{
		/// <summary>
		/// Идентификатор документа
		/// </summary>
		public int Id { get; set; }

		public static OrderDocumentSentEvent Create(int id) =>
			new OrderDocumentSentEvent
			{
				Id = id
			};
	}
}

using Mailganer.Api.Client.Dto;

namespace EmailSchedulerWorker.Consumers
{
	/// <summary>
	/// Событие для обработки писем клиентов
	/// </summary>
	public record ProcessClientEmailEvent
	{
		/// <summary>
		/// Письмо клиенту
		/// </summary>
		public EmailMessage EmailMessage { get; init; } = new EmailMessage();

		/// <summary>
		/// Идентификатор клиента
		/// </summary>
		public int ClientId { get; init; }

		/// <summary>
		/// Идентификаторы заказов клиента
		/// </summary>
		public List<int> OrderIds { get; init; } = new();
	}
}

using Vodovoz.Domain.Client;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.EntityRepositories
{
	/// <summary>
	/// Информация о подписке на массовую рассылку
	/// </summary>
	public class CounterpartyBulkSubscribeNode
	{
		/// <summary>
		/// Id Клиента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Типы события рассылки
		/// </summary>
		public BulkEmailEventType? BulkEmailEventType { get; set; }

		/// <summary>
		/// Типы письма
		/// </summary>
		public CounterpartyEmailType? CounterpartyEmailType { get; set; }
	}
}

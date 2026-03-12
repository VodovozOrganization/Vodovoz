using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Строка с данными о просроченной задаче по документообороту
	/// </summary>
	public class TimedOutOrderDocumentTaskNode
	{
		/// <summary>
		/// Id контрагента
		/// </summary>
		public int ClientId { get; set; }

		/// <summary>
		/// ИНН контрагента
		/// </summary>
		public string ClientInn { get; set; }

		/// <summary>
		/// Статус регистрации клиента в ЧЗ
		/// </summary>
		public RegistrationInChestnyZnakStatus RegistrationInChestnyZnakStatus { get; set; }

		/// <summary>
		/// Заказ
		/// </summary>
		public OrderEntity Order { get; set; }

		/// <summary>
		/// Задача на документооборот
		/// </summary>
		public DocumentEdoTask Task { get; set; }
	}
}

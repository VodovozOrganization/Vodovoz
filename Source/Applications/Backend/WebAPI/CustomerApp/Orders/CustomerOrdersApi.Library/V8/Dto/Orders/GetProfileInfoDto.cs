using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V8.Dto.Orders
{
	/// <summary>
	/// Запрос профиля клиента
	/// </summary>
	public sealed class GetProfileInfoDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }

		/// <summary>
		/// Id клиента в ERP
		/// </summary>
		public int ErpCounterpartyId { get; set; }

		/// <summary>
		/// Id пользователя в МП
		/// </summary>
		public System.Guid ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Контрольная сумма запроса
		/// </summary>
		public string Signature { get; set; }
	}
}

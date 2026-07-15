using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Строка информации о клиенте и организации, по которым есть непринятый УПД в ЭДО
	/// </summary>
	public class TimedOutDocFlowRow
	{
		/// <summary>
		/// ЭДО аккаунт организации, от которой был отправлен документ
		/// </summary>
		public string OurEdoAccount { get; set; }

		/// <summary>
		/// Номер УПД
		/// </summary>
		public string UpdNum { get; set; }

		/// <summary>
		/// Клиент
		/// </summary>
		public CounterpartyEntity Client { get; set; }

		/// <summary>
		/// Организация
		/// </summary>
		public OrganizationEntity Organization { get; set; }

		/// <summary>
		/// Заказ
		/// </summary>
		public OrderEntity Order { get; set; }

		/// <summary>
		/// Документооборот в Такском
		/// </summary>
		public TaxcomDocflow TaxcomDocflow { get; set; }
	}
}

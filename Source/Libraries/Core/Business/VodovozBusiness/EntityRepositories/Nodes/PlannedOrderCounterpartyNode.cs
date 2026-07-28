using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные контрагента для уведомления по плановым заказам
	/// </summary>
	public class PlannedOrderCounterpartyNode
	{
		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Полное наименование контрагента
		/// </summary>
		public string FullName { get; set; }

		/// <summary>
		/// ИНН контрагента
		/// </summary>
		public string Inn { get; set; }

		/// <summary>
		/// Форма контрагента (физическое/юридическое лицо)
		/// </summary>
		public PersonType PersonType { get; set; }

		/// <summary>
		/// Отсрочка по оплате для контрагента в днях
		/// </summary>
		public int DelayDaysForBuyers { get; set; }
	}
}

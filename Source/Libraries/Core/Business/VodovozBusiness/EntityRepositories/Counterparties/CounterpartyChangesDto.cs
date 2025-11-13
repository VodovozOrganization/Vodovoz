using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	/// <summary>
	/// Информация об изменениях контрагента
	/// </summary>
	public class CounterpartyChangesDto
	{
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; set; }
		
		/// <summary>
		/// КПП
		/// </summary>
		public string Kpp { get; set; }
		
		/// <summary>
		/// Тип контрагента
		/// </summary>
		public CounterpartyType CounterpartyType { get; set; }
		
		/// <summary>
		/// Сеть?
		/// </summary>
		public bool IsChainStore { get; set; }
		
		/// <summary>
		/// Тип задолженности
		/// </summary>
		public DebtType? CloseDeliveryDebtType { get; set; }
		
		/// <summary>
		/// Откуда клиент
		/// </summary>
		public ClientCameFrom CameFrom { get; set; }
		
		/// <summary>
		/// Отсрочка дней покупателям
		/// </summary>
		public int DelayDaysForBuyers { get; set; }
		
		/// <summary>
		/// Статус контрагента в налоговой
		/// </summary>
		public RevenueStatus? RevenueStatus { get; set; }
	}
}

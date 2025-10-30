namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные по платежам контрагента
	/// </summary>
	public class CounterpartyPaymentsDataNode
	{
		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }
		/// <summary>
		/// Наименование контрагента
		/// </summary>
		public string CounterpartyName { get; set; }
		/// <summary>
		/// ИНН контрагента
		/// </summary>
		public string CounterpartyInn { get; set; }
		/// <summary>
		/// Сумма поступлений
		/// </summary>
		public decimal IncomeSum { get; set; }
		/// <summary>
		/// Сумма распределенных оплат
		/// </summary>
		public decimal PaymentItemsSum { get; set; }
		/// <summary>
		/// Нераспределенный баланс
		/// </summary>
		public decimal UnallocatedBalance => IncomeSum - PaymentItemsSum;
		/// <summary>
		/// Возвращенный баланс
		/// </summary>
		public decimal WriteOffSum { get; set; }
		/// <summary>
		/// Отсрочка по оплате для контрагента в днях
		/// </summary>
		public int DelayDaysForCounterparty { get; set; }
		/// <summary>
		/// Контрагент в статусе ликвидации
		/// </summary>
		public bool IsLiquidating { get; set; }
	}
}

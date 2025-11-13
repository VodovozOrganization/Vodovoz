namespace TaxcomEdo.Contracts.Counterparties
{
	/// <summary>
	/// Информация о точке доставки для ЭДО(электронного документооборота)
	/// </summary>
	public class DeliveryPointInfoForEdo
	{
		/// <summary>
		/// Id ТД
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Id клиента
		/// </summary>
		public int CounterpartyId { get; set; }
		/// <summary>
		/// Сокращенный адрес
		/// </summary>
		public string ShortAddress { get; set; }
		/// <summary>
		/// КПП
		/// </summary>
		public string KPP { get; set; }
	}
}

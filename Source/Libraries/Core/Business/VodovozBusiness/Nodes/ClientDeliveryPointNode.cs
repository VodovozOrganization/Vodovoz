namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Информация о клиентской ТД
	/// </summary>
	public class ClientDeliveryPointNode
	{
		/// <summary>
		/// Идентификатор ТД
		/// </summary>
		public int DeliveryPointId { get; set; }
		/// <summary>
		/// Адрес
		/// </summary>
		public string Address { get; set; }
		/// <summary>
		/// Есть фикса
		/// </summary>
		public bool HasFixedPrices { get; set; }
		/// <summary>
		/// Есть шаблоны автозаказов
		/// </summary>
		public bool HasTemplates { get; set; }
		/// <summary>
		/// Действующая ТД
		/// </summary>
		public bool IsActive { get; set; }
	}
}

namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Данные по ДЗЧ
	/// </summary>
	public sealed class FastDeliveryDto
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }

		public static FastDeliveryDto Create(int id, string name, decimal price)
		{
			return new FastDeliveryDto
			{
				ErpId = id,
				Name = name,
				Price = price
			};
		}
	}
}

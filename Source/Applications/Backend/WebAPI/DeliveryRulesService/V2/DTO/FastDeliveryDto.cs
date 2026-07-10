namespace DeliveryRulesService.V2.DTO
{
	public sealed class FastDeliveryDto
	{
		public int ErpId { get; set; }
		public string Name { get; set; }
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

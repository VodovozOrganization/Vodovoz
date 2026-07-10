namespace DeliveryRulesService.V2.DTO
{
	public sealed class PaidDeliveryDto
	{
		public int ErpId { get; set; }
		public string Name { get; set; }

		public static PaidDeliveryDto Create(int erpId, string name) =>
			new PaidDeliveryDto
			{
				ErpId = erpId,
				Name = name
			};
	}
}

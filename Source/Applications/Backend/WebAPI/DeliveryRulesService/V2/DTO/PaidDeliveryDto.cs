namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Данные по платной доставке
	/// </summary>
	public sealed class PaidDeliveryDto
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; set; }

		public static PaidDeliveryDto Create(int erpId, string name) =>
			new PaidDeliveryDto
			{
				ErpId = erpId,
				Name = name
			};
	}
}

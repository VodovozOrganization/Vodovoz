namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Графики доставки
	/// </summary>
	public class ScheduleRestrictionDto
	{
		/// <summary>
		/// Идентификатор в ДВ
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Название
		/// </summary>
		public string IntervalName { get; set; }

		public static ScheduleRestrictionDto Create(int erpId, string intervalName) =>
			new ScheduleRestrictionDto
			{
				ErpId = erpId,
				IntervalName = intervalName
			};
	}
}

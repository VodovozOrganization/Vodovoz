namespace CustomerAppsApi.Library.Dto
{
	/// <summary>
	/// Информация о созданной точке доставки
	/// </summary>
	public class CreatedDeliveryPointDto : DeliveryPointInfoDto
	{
		/// <summary>
		/// Id точки доставки в Erp
		/// </summary>
		public int DeliveryPointErpId { get; set; }
		/// <summary>
		/// Id клиента в Erp, которому принадлежит эта ТД
		/// </summary>
		public int CounterpartyErpId { get; set; }
	}
}

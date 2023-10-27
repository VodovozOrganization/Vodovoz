namespace CustomerAppsApi.Library.Dto
{
	public class DeliveryPointDto : DeliveryPointInfoDto
	{
		public int DeliveryPointErpId { get; set; }
		public int CounterpartyErpId { get; set; }
	}
}

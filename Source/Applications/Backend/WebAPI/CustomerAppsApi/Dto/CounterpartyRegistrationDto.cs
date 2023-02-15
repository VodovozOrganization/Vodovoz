namespace CustomerAppsApi.Dto
{
	public class CounterpartyRegistrationDto
	{
		public int? ErpCounterpartyId { get; set; }
		public string ErrorDescription { get; set; }
		public CounterpartyRegistrationStatus CounterpartyRegistrationStatus { get; set; }
	}
}

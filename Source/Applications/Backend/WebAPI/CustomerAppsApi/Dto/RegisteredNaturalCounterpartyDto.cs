namespace CustomerAppsApi.Controllers
{
	public class RegisteredNaturalCounterpartyDto
	{
		public int ExternalCounterpartyId { get; set; }
		public int ErpCounterpartyId { get; set; }
		public string FirstName { get; set; }
		public string Surname { get; set; }
		public string Patronymic { get; set; }
		public string Email { get; set; }
	}
}

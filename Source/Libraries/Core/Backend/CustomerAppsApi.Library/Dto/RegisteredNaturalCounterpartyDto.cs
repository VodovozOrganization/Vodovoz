using System;

namespace CustomerAppsApi.Library.Dto
{
	public class RegisteredNaturalCounterpartyDto
	{
		public Guid ExternalCounterpartyId { get; set; }
		public int ErpCounterpartyId { get; set; }
		public string FirstName { get; set; }
		public string Surname { get; set; }
		public string Patronymic { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
	}
}

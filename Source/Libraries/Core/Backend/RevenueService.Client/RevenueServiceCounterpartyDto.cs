namespace RevenueService.Client
{
	public class RevenueServiceCounterpartyDto
	{
		public string Inn { get; set; }
		public string Kpp { get; set; }
		public string ShortName { get; set; }
		public string FullName { get; set; }
		public string Address { get; set; }
		public string PersonSurname { get; set; }
		public string PersonName { get; set; }
		public string PersonPatronymic { get; set; }
		public string[] Phones { get; set; }
		public string[] Emails { get; set; }
		public string BranchType { get; set; }
		public string TypeOfOwnerShip { get; set; }
		public string LegalPersonFullName { get; set; }
	}
}

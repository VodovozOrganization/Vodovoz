using RevenueService.Client.Enums;

namespace RevenueService.Client.Dto
{
	public class CounterpartyDto
	{
		public string Inn { get; set; }
		public string Kpp { get; set; }
		public string ShortName { get; set; }
		public string FullName { get; set; }

		public string Name => string.IsNullOrEmpty(ShortName) ? FullName : ShortName;
		public string Address { get; set; }
		public string PersonSurname { get; set; }
		public string PersonName { get; set; }
		public string PersonPatronymic { get; set; }

		public string PersonFullName
		{
			get
			{
				if(PersonSurname == null && PersonName == null && PersonPatronymic == null)
				{
					return null;
				}

				return string.Join(" ", PersonSurname, PersonName, PersonPatronymic);
			}
		}
		public string[] Phones { get; set; }
		public string[] Emails { get; set; }
		public BranchType BranchType { get; set; }
		public TypeOfOwnership TypeOfOwnerShip { get; set; }
		public string ManagerFullName { get; set; }
		public string State { get; set; }

		public bool IsActive => State == "ACTIVE";
	}
}

using System.Collections.Generic;

namespace Vodovoz.Services
{
	public class CounterpartyRevenueServiceInfo
	{
		public string INN { get; set; }
		public string KPP { get; set; }
		public string Name { get; set; }
		public string FullName { get; set; }
		public string LegalAddress { get; set; }
		public string TypeOfOwnership { get; set; }
		public string SignatoryFIO { get; set; }
		public string Surname { get; set; }
		public string FirstName { get; set; }
		public string Patronymic { get; set; }
		public IEnumerable<string> Emails { get; set; }
		public IEnumerable<string> Phones { get; set; }
	}
}

using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Documents
{
	public class Participant : IParticipant
	{
		public string Inn { get; set; }
		public string Kpp { get; set; }
		public string Name { get; set; }
		public string Identifier { get; set; }
	}
}

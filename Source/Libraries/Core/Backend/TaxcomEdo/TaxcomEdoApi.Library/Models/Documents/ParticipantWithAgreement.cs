using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Documents
{
	public class ParticipantWithAgreement : Participant, IParticipantWithAgreement
	{
		public IAgreement Agreement { get; set; } = new Agreement();
	}
}

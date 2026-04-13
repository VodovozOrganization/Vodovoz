namespace TaxcomEdoApi.Library.Models.Interfaces
{
	public interface IParticipantWithAgreement : IParticipant
	{
		IAgreement Agreement { get; set; }
	}
}

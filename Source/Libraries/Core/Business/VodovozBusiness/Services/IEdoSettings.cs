namespace Vodovoz.Services
{
	public interface IEdoSettings
	{
		string TaxcomIntegratorId { get; }
		string TaxcomBaseAddressUri { get; }
		string TaxcomLogin { get; }
		string TaxcomPassword { get; }
		string TaxcomCheckContragentUri { get;}
		string TaxcomSendContactsUri { get; }
		string TaxcomGetContactListUpdatesUri { get; }
		string TrueApiBaseAddressUri { get; }
		string TrueApiParticipantsUri { get; }
		int EdoCheckPeriodDays { get; }
	}
}

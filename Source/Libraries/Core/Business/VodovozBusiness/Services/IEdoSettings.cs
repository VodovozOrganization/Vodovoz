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
		string TrueMarkApiBaseUrl { get; }
		string TrueMarkApiParticipantRegistrationForWaterUri { get; }
		string TrueMarkApiToken { get; }
		int EdoCheckPeriodDays { get; }
		int TaxcomManualInvitationFileId { get; }
		int TrueMarkCodesHandleInterval { get; }

	}
}

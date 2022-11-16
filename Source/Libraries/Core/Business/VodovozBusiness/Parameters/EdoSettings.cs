using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class EdoSettings : IEdoSettings
	{
		private readonly IParametersProvider _parametersProvider;

		public EdoSettings(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider;
		}

		public string TaxcomIntegratorId => _parametersProvider.GetStringValue("TaxcomIntegratorId");
		public string TaxcomBaseAddressUri => _parametersProvider.GetStringValue("TaxcomBaseAddressUri");
		public string TaxcomLogin => _parametersProvider.GetStringValue("TaxcomLogin");
		public string TaxcomPassword => _parametersProvider.GetStringValue("TaxcomPassword");
		public string TaxcomCheckContragentUri => _parametersProvider.GetStringValue("TaxcomCheckContragentUri");
		public string TaxcomSendContactsUri => _parametersProvider.GetStringValue("TaxcomSendContactsUri");
		public string TaxcomGetContactListUpdatesUri => _parametersProvider.GetStringValue("TaxcomGetContactListUpdatesUri");
		public string TrueApiBaseAddressUri => _parametersProvider.GetStringValue("TrueApiBaseAddressUri");
		public string TrueApiParticipantsUri => _parametersProvider.GetStringValue("TrueApiParticipantsUri");
		public int TaxcomCheckConsentDays => _parametersProvider.GetIntValue("TaxcomCheckConsentDays");
		public int EdoCheckPeriodDays => _parametersProvider.GetIntValue("EdoCheckPeriodDays");
	}
}

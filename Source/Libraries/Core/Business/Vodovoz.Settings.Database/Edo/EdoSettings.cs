using System;
using System.Linq;
using Vodovoz.Settings.Edo;

namespace Vodovoz.Settings.Database.Edo
{
	public class EdoSettings : IEdoSettings
	{
		private readonly ISettingsController _settingsController;

		public EdoSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new System.ArgumentNullException(nameof(settingsController));
		}

		public string TaxcomIntegratorId => _settingsController.GetStringValue("TaxcomIntegratorId");
		public string TaxcomBaseAddressUri => _settingsController.GetStringValue("TaxcomBaseAddressUri");
		public string TaxcomLogin => _settingsController.GetStringValue("TaxcomLogin");
		public string TaxcomPassword => _settingsController.GetStringValue("TaxcomPassword");
		public string TaxcomCheckContragentUri => _settingsController.GetStringValue("TaxcomCheckContragentUri");
		public string TaxcomSendContactsUri => _settingsController.GetStringValue("TaxcomSendContactsUri");
		public string TaxcomGetContactListUpdatesUri => _settingsController.GetStringValue("TaxcomGetContactListUpdatesUri");
		public string TrueMarkApiBaseUrl => _settingsController.GetStringValue("TrueMarkApiBaseUrl");
		public string TrueMarkApiParticipantRegistrationForWaterUri => _settingsController.GetStringValue("TrueMarkApiParticipantRegistrationForWaterUri");
		public string TrueMarkApiToken => _settingsController.GetStringValue("TrueMarkApiToken"); 
		public int EdoCheckPeriodDays => _settingsController.GetIntValue("EdoCheckPeriodDays");
		public int TaxcomManualInvitationFileId => _settingsController.GetIntValue("TaxcomManualInvitationFileId");
		public int TrueMarkCodesHandleInterval => _settingsController.GetIntValue("TrueMarkCodesHandleInterval");
		public bool NewEdoProcessing => _settingsController.GetBoolValue("Edo.NewEdoProcessing");
		public string TrueMarkApiParticipantsUri => _settingsController.GetStringValue("TrueMarkApiParticipantsUri");
		public int CodePoolCheckCodesDepth => _settingsController.GetIntValue("TrueMarkCodePoolCheckCodesDepth");
		public int CodePoolCheckIntervalMinutes => _settingsController.GetIntValue("TrueMarkCodePoolCheckIntervalMinutes");
		public int CodePoolPromoteWithExtraSeconds => _settingsController.GetIntValue("TrueMarkCodePoolPromoteWithExtraSeconds");
		public int[] OrganizationsHavingAccountsInTrueMark => _settingsController
			.GetStringValue("TrueMark.OrganizationsHavingAccountsInTrueMark")
			.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
			.Select(x => int.Parse(x.Trim(' ')))
			.ToArray();

		public bool CodePoolLoaderToNewPool => _settingsController.GetBoolValue("CodePoolLoaderToNewPool");
	}
}

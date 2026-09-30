using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Settings.Edo;

namespace Vodovoz.Settings.Database.Edo
{
	public class EdoSettings : IEdoSettings
	{
		private readonly ISettingsController _settingsController;
		private IReadOnlyDictionary<int, string> _taxcomOrganizationBaseAddresses;

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
		public int CodePoolTakeValidCodeAttempts => _settingsController.GetIntValue("TrueMarkCodePoolTakeValidCodeAttempts");
		public int[] OrganizationsHavingAccountsInTrueMark => _settingsController
			.GetStringValue("TrueMark.OrganizationsHavingAccountsInTrueMark")
			.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
			.Select(x => int.Parse(x.Trim(' ')))
			.ToArray();

		public bool CodePoolLoaderToNewPool => _settingsController.GetBoolValue("CodePoolLoaderToNewPool");

		public int WithdrawalDocflowTimeoutDays =>
			_settingsController.GetIntValue("edo.withdrawal.docflow_timeout");

		public int ExpiredCodesCleanerIntervalMinutes => _settingsController.GetIntValue(nameof(ExpiredCodesCleanerIntervalMinutes));

		public int UsedCodesCleanerIntervalHours => _settingsController.GetIntValue(nameof(UsedCodesCleanerIntervalHours));

		public string TaxcomGetDocflowStatusEndpoint => _settingsController.GetStringValue(nameof(TaxcomGetDocflowStatusEndpoint));

		public IReadOnlyDictionary<int, string> TaxcomOrganizationBaseAddresses
		{
			get
			{
				var dict = new Dictionary<int, string>();

				var addressesString = _settingsController.GetStringValue(nameof(TaxcomOrganizationBaseAddresses));

				if(!string.IsNullOrWhiteSpace(addressesString))
				{
					var cleaned = addressesString.Replace(" ", "");
					var blocks = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

					foreach(var block in blocks)
					{
						var inner = block.Trim('{', '}');
						var parts = inner.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

						if(parts.Count() == 2 && int.TryParse(parts[0].Trim(), out int organizationId))
						{
							dict[organizationId] = parts[1].Trim();
						}
					}
				}

				return dict;
			}
		}
	}
}

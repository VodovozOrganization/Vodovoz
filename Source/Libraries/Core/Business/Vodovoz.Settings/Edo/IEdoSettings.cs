using System;

namespace Vodovoz.Settings.Edo
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
		string TrueMarkApiParticipantsUri { get; }
		string TrueMarkApiToken { get; }
		int EdoCheckPeriodDays { get; }
		int TaxcomManualInvitationFileId { get; }
		int TrueMarkCodesHandleInterval { get; }
		bool NewEdoProcessing { get; }
		int CodePoolCheckCodesDepth { get; }
		int CodePoolCheckIntervalMinutes { get; }
		int CodePoolPromoteWithExtraSeconds { get; }
		int[] OrganizationsHavingAccountsInTrueMark { get; }
		bool CodePoolLoaderToNewPool { get; }

		/// <summary>
		/// Количество дней не принятия документооборота клиентом, подключенным к ЧЗ, 
		/// после которого будет создана заявка на вывод кодов из оборота
		/// </summary>
		int ConnectedTrueMarkClientsWithdrawalDocflowTimeoutDays { get; }

		/// <summary>
		/// Количество дней не принятия документооборота клиентом, не подключенным к ЧЗ, 
		/// после которого будет создана заявка на вывод кодов из оборота
		/// </summary>
		int NotConnectedTrueMarkClientsWithdrawalDocflowTimeoutDays { get; }
	}
}

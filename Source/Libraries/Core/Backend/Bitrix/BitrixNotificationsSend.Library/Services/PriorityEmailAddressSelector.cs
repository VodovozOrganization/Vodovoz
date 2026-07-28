using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Contacts;
using VodovozBusiness.EntityRepositories.Nodes;

namespace BitrixNotificationsSend.Library.Services
{
	/// <summary>
	/// Выбор приоритетного адреса электронной почты контрагента
	/// </summary>
	internal static class PriorityEmailAddressSelector
	{
		/// <summary>
		/// Выбор приоритетного адреса электронной почты из адресов контрагента.
		/// Приоритет назначений: для счетов, рабочий, личный, для чеков, любой первый
		/// </summary>
		/// <param name="counterpartyEmails">Адреса электронной почты контрагента с назначениями</param>
		/// <returns>Приоритетный адрес, null - если адресов нет</returns>
		public static string SelectPriorityEmailAddress(IList<CounterpartyEmailWithPurposeNode> counterpartyEmails)
		{
			if(counterpartyEmails == null || !counterpartyEmails.Any())
			{
				return null;
			}

			var email = counterpartyEmails.FirstOrDefault(e => e.EmailPurpose == EmailPurpose.ForBills)
				?? counterpartyEmails.FirstOrDefault(e => e.EmailPurpose == EmailPurpose.Work)
				?? counterpartyEmails.FirstOrDefault(e => e.EmailPurpose == EmailPurpose.Personal)
				?? counterpartyEmails.FirstOrDefault(e => e.EmailPurpose == EmailPurpose.ForReceipts)
				?? counterpartyEmails.FirstOrDefault();

			return email?.Address;
		}
	}
}

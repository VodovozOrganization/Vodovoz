using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Client
{
	/// <summary>
	/// ЭДО аккаунт контрагента
	/// </summary>
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "ЭДО аккаунты контрагента",
			Nominative = "ЭДО аккаунт контрагента",
			Accusative = "ЭДО аккаунта контрагента",
			Genitive = "ЭДО аккаунта контрагента"
		)
	]
	[HistoryTrace]
	public class CounterpartyEdoAccount : CounterpartyEdoAccountEntity
	{
		private Counterparty _counterparty;
		
		/// <summary>
		/// Клиент
		/// </summary>
		[Display(Name = "Клиент")]
		public new virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		public static CounterpartyEdoAccount Create(
			Counterparty counterparty,
			EdoOperator edoOperator,
			string personalAccountIdInEdo,
			int organizationId,
			bool isDefault,
			ConsentForEdoStatus consentForEdoStatus = ConsentForEdoStatus.Unknown
			)
		{
			return new CounterpartyEdoAccount
			{
				Counterparty = counterparty,
				EdoOperator = edoOperator,
				PersonalAccountIdInEdo = personalAccountIdInEdo,
				OrganizationId = organizationId,
				IsDefault = isDefault,
				ConsentForEdoStatus = consentForEdoStatus
			};
		}
	}
}

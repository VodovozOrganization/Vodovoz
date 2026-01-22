using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Clients.Accounts
{
	/// <summary>
	/// Аккаунт юр лица в ИПЗ
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Аккаунты юр лиц в ИПЗ",
		Nominative = "Аккаунт юр лица в ИПЗ",
		Prepositional = "Аккаунте юр лица в ИПЗ",
		PrepositionalPlural = "Аккаунтах юр лиц в ИПЗ"
	)]
	[HistoryTrace]
	public class ExternalLegalCounterpartyAccount : PropertyChangedBase, IDomainObject
	{
		private int _legalCounterpartyId;
		private int _legalCounterpartyEmailId;
		private string _accountPasswordHash;
		private string _accountPasswordSalt;

		protected ExternalLegalCounterpartyAccount() { }
		
		private ExternalLegalCounterpartyAccount(
			int legalCounterpartyId,
			int legalCounterpartyEmailId,
			(string Salt, string PasswordHash) passwordData)
		{
			LegalCounterpartyId = legalCounterpartyId;
			LegalCounterpartyEmailId = legalCounterpartyEmailId;
			
			UpdatePasswordData(passwordData);
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }
		
		/// <summary>
		/// Статус проверки в ФНС
		/// </summary>
		[IgnoreHistoryTrace]
		public virtual TaxServiceCheckState? TaxServiceCheckState { get; set; }

		/// <summary>
		/// Идентификатор юридического лица, от которого сможет заказывать в ИПЗ физик
		/// </summary>
		[Display(Name = "Идентификатор юридического лица из аккаунта ИПЗ")]
		public virtual int LegalCounterpartyId
		{
			get => _legalCounterpartyId;
			set => SetField(ref _legalCounterpartyId, value);
		}

		/// <summary>
		/// Идентификатор почты юр лица, через которую настроена связь с физиком
		/// </summary>
		[Display(Name = "Идентификатор почты юр лица, через которую настроена связь с физиком")]
		public virtual int LegalCounterpartyEmailId
		{
			get => _legalCounterpartyEmailId;
			set => SetField(ref _legalCounterpartyEmailId, value);
		}
		
		/// <summary>
		/// Хэш пароля аккаунта
		/// </summary>
		[Display(Name = "Хэш пароля аккаунта")]
		[IgnoreHistoryTrace]
		public virtual string AccountPasswordHash
		{
			get => _accountPasswordHash;
			set => SetField(ref _accountPasswordHash, value);
		}
		
		/// <summary>
		/// Соль пароля аккаунта
		/// </summary>
		[Display(Name = "Соль пароля аккаунта")]
		[IgnoreHistoryTrace]
		public virtual string AccountPasswordSalt
		{
			get => _accountPasswordSalt;
			set => SetField(ref _accountPasswordSalt, value);
		}

		public static ExternalLegalCounterpartyAccount Create(
			int legalCounterpartyId,
			int legalCounterpartyEmailId,
			(string Salt, string PasswordHash) passwordData) =>
			new ExternalLegalCounterpartyAccount(
				legalCounterpartyId,
				legalCounterpartyEmailId,
				passwordData);

		public virtual void UpdatePasswordData((string Salt, string PasswordHash) passwordData)
		{
			AccountPasswordHash = passwordData.PasswordHash;
			AccountPasswordSalt = passwordData.Salt;
		}
	}
}

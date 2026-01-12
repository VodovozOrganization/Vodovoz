using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Clients.Accounts
{
	/// <summary>
	/// Информация о связи юридического лица и пользователя ИПЗ, через почту первого,
	/// для возможности заказа под указанным юриком в ИПЗ
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
		private Source _source;
		private int _legalCounterpartyId;
		private int _legalCounterpartyEmailId;
		private Guid _externalUserId;
		private string _accountPasswordHash;
		private string _accountPasswordSalt;
		private ExternalLegalCounterpartyAccountActivation _accountActivation;

		protected ExternalLegalCounterpartyAccount() { }
		
		private ExternalLegalCounterpartyAccount(
			Source source,
			int legalCounterpartyId,
			int legalCounterpartyEmailId,
			Guid externalUserId,
			(string Salt, string PasswordHash) passwordData)
		{
			Source = source;
			LegalCounterpartyId = legalCounterpartyId;
			LegalCounterpartyEmailId = legalCounterpartyEmailId;
			ExternalUserId = externalUserId;
			AccountPasswordSalt = passwordData.Salt;
			AccountPasswordHash = passwordData.PasswordHash;
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

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
		/// ИПЗ
		/// </summary>
		[Display(Name = "Источник")]
		public virtual Source Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}
		
		/// <summary>
		/// Идентификатор внешнего пользователя
		/// </summary>
		[Display(Name = "Идентификатор внешнего пользователя")]
		public virtual Guid ExternalUserId
		{
			get => _externalUserId;
			set => SetField(ref _externalUserId, value);
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

		/// <summary>
		/// Состояние активации
		/// </summary>
		[Display(Name = "Состояние активации")]
		public virtual ExternalLegalCounterpartyAccountActivation AccountActivation
		{
			get => _accountActivation;
			set => SetField(ref _accountActivation, value);
		}

		public static ExternalLegalCounterpartyAccount Create(
			Source source,
			int legalCounterpartyId,
			int legalCounterpartyEmailId,
			Guid externalCounterpartyId,
			(string Salt, string PasswordHash) passwordData) =>
			new ExternalLegalCounterpartyAccount(
				source,
				legalCounterpartyId,
				legalCounterpartyEmailId,
				externalCounterpartyId,
				passwordData);
	}
}

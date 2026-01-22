using System;
using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Данные по запросу юр лиц по ИНН
	/// </summary>
	public class LegalCustomersByInnResponse
	{
		private LegalCustomersByInnResponse(
			int id,
			string name,
			string firstName,
			string surname,
			string patronymic,
			string inn,
			string kpp,
			string jurAddress,
			string shortTypeOfOwnership,
			bool isArchived,
			int? emailId,
			int? activeEmailId
			)
		{
			ErpCounterpartyId = id;
			JurAddress = jurAddress;
			Inn = inn;
			Kpp = kpp;
			FullName = name;
			ShortTypeOfOwnership = shortTypeOfOwnership;
			FirstName = firstName;
			Surname = surname;
			Patronymic = patronymic;
			IsArchived = isArchived;

			UpdateEmailState(emailId, activeEmailId);
		}

		private LegalCustomersByInnResponse(NextStepGetLegalCounterpartiesByInnRequest nextStep)
		{
			NextStep = nextStep;
		}
		
		/// <summary>
		/// Идентификатор клиента
		/// </summary>
		public int ErpCounterpartyId { get; }
		/// <summary>
		/// Полное наименование
		/// </summary>
		public string FullName { get; }
		/// <summary>
		/// Имя(ИП)
		/// </summary>
		public string FirstName { get; }
		/// <summary>
		/// Фамилия(ИП)
		/// </summary>
		public string Surname { get; }
		/// <summary>
		/// Отчество(ИП)
		/// </summary>
		public string Patronymic { get; }
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; }
		/// <summary>
		/// КПП
		/// </summary>
		public string Kpp { get; }
		/// <summary>
		/// Юридический адрес
		/// </summary>
		public string JurAddress { get; }
		/// <summary>
		/// Сокращенная форма собственности
		/// </summary>
		public string ShortTypeOfOwnership { get; }
		/// <summary>
		/// Состояние почты
		/// </summary>
		[JsonIgnore]
		public CounterpartyEmailState EmailState { get; set; }
		/// <summary>
		/// Спец предупреждение
		/// </summary>
		public Warning Warning { get; set; }
		/// <summary>
		/// Следующий шаг после запроса юр лиц по ИНН
		/// </summary>
		public NextStepGetLegalCounterpartiesByInnRequest NextStep { get; set; }
		/// <summary>
		/// Архивирован или нет
		/// </summary>
		[JsonIgnore]
		private bool IsArchived { get; }
		
		public void UpdateNextStep()
		{
			if(IsArchived)
			{
				NextStep = NextStepGetLegalCounterpartiesByInnRequest.CounterpartyArchived;
				Warning = Warning.CreateCounterpartyArchived();
				return;
			}
			
			switch(EmailState)
			{
				case CounterpartyEmailState.EmailNotExistsAndNotExistsActiveEmails:
					NextStep = NextStepGetLegalCounterpartiesByInnRequest.ConfirmAccess;
					break;
				case CounterpartyEmailState.EmailExistsAndNotExistsActiveEmails:
					NextStep = NextStepGetLegalCounterpartiesByInnRequest.CreateConnection;
					break;
				case CounterpartyEmailState.HasAnotherActiveEmail:
					NextStep = NextStepGetLegalCounterpartiesByInnRequest.UserHasAnotherActiveEmail;
					Warning = Warning.CreateAnotherAccountExists();
					break;
				case CounterpartyEmailState.HasEmailAndActive:
					NextStep = NextStepGetLegalCounterpartiesByInnRequest.Authenticate;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(EmailState), "Неизвестное состояние электронной почты и аккаунта!");
			}
		}
		
		private void UpdateEmailState(int? emailId, int? activeEmailId)
		{
			if(!emailId.HasValue && !activeEmailId.HasValue)
			{
				EmailState = CounterpartyEmailState.EmailNotExistsAndNotExistsActiveEmails;
			}
			else if((!emailId.HasValue && activeEmailId.HasValue)
				|| emailId.HasValue && activeEmailId.HasValue && activeEmailId != emailId)
			{
				EmailState = CounterpartyEmailState.HasAnotherActiveEmail;
			}
			else if(emailId.HasValue && !activeEmailId.HasValue)
			{
				EmailState = CounterpartyEmailState.EmailExistsAndNotExistsActiveEmails;
			}
			else if(emailId.HasValue && activeEmailId.HasValue && activeEmailId == emailId)
			{
				EmailState = CounterpartyEmailState.HasEmailAndActive;
			}
		}

		public static LegalCustomersByInnResponse Create(
			int id,
			string name,
			string firstName,
			string surname,
			string patronymic,
			string inn,
			string kpp,
			string jurAddress,
			string shortTypeOfOwnership,
			bool isArchive,
			int? emailId,
			int? activeEmailId) =>
			new LegalCustomersByInnResponse(
				id, name, firstName, surname, patronymic, inn, kpp, jurAddress, shortTypeOfOwnership, isArchive, emailId, activeEmailId);
		
		public static LegalCustomersByInnResponse CreateEmpty() =>
			new LegalCustomersByInnResponse(NextStepGetLegalCounterpartiesByInnRequest.CounterpartiesNotExists);
	}
}

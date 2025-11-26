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
			string fullName,
			string firstName,
			string surname,
			string patronymic,
			string inn,
			string kpp,
			string jurAddress,
			string shortTypeOfOwnership,
			int? emailId,
			int? activeEmailId
			)
		{
			ErpCounterpartyId = id;
			Name = name;
			JurAddress = jurAddress;
			Inn = inn;
			Kpp = kpp;
			FullName = fullName;
			ShortTypeOfOwnership = shortTypeOfOwnership;
			FirstName = firstName;
			Surname = surname;
			Patronymic = patronymic;

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
		/// Наименование
		/// </summary>
		public string Name { get; }
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
		
		public void UpdateNextStep()
		{
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
					//TODO 5417: дописать после обновления Костей
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
			else if(!emailId.HasValue && activeEmailId.HasValue
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
			string fullName,
			string firstName,
			string surname,
			string patronymic,
			string inn,
			string kpp,
			string jurAddress,
			string shortTypeOfOwnership,
			int? emailId,
			int? activeEmailId) =>
			new LegalCustomersByInnResponse(
				id, name, fullName, firstName, surname, patronymic, inn, kpp, jurAddress, shortTypeOfOwnership, emailId, activeEmailId);
		
		public static LegalCustomersByInnResponse CreateEmpty() =>
			new LegalCustomersByInnResponse(NextStepGetLegalCounterpartiesByInnRequest.CounterpartiesNotExists);
	}

	public enum CounterpartyEmailState
	{
		/// <summary>
		/// У клиента нет указанной почты и нет активной учетной записи
		/// </summary>
		EmailNotExistsAndNotExistsActiveEmails,
		/// <summary>
		/// У клиента есть указанная почта и нет активной учетной записи
		/// </summary>
		EmailExistsAndNotExistsActiveEmails,
		/// <summary>
		/// У клиента уже есть другая активная учетная запись
		/// </summary>
		HasAnotherActiveEmail,
		/// <summary>
		/// У клиента уже есть почта и она активна
		/// </summary>
		HasEmailAndActive
	}

	/// <summary>
	/// Класс для размещения спец виджета предупреждения
	/// </summary>
	public class Warning
	{
		/// <summary>
		/// Заголовок
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// Описание
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// Название кнопки
		/// </summary>
		public string Button { get; set; }

		public static Warning CreateAnotherAccountExists() => new Warning
		{
			Title = "У этой компании уже есть учетная запись",
			Description = "Зайдите в профиль компании через другую почту или обратитесь в поддержку",
			Button = "support"
		};
	}
}

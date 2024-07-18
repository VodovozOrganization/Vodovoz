using System;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Зарегистрированный пользователь (физ лицо)
	/// </summary>
	public class RegisteredNaturalCounterpartyDto
	{
		protected RegisteredNaturalCounterpartyDto(
			Counterparty counterparty,
			Guid externalCounterpartyId,
			string emailAddress,
			string digitsNumber)
		{
			ErpCounterpartyId = counterparty.Id;
			Email = emailAddress;
			ExternalCounterpartyId = externalCounterpartyId;
			FirstName = counterparty.FirstName;
			Surname = counterparty.Surname;
			Patronymic = counterparty.Patronymic;
			PhoneNumber = $"+7{digitsNumber}";
		}
		
		/// <summary>
		/// Внешний номер пользователя
		/// </summary>
		public Guid ExternalCounterpartyId { get; }
		/// <summary>
		/// Id клиента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; }
		/// <summary>
		/// Имя
		/// </summary>
		public string FirstName { get; }
		/// <summary>
		/// Фамилия
		/// </summary>
		public string Surname { get; }
		/// <summary>
		/// Отчество
		/// </summary>
		public string Patronymic { get; }
		/// <summary>
		/// Электронка
		/// </summary>
		public string Email { get; }
		/// <summary>
		/// Телефон в формате +7XXXXXXXXXX, где X - целые числа, обозначающие код и номер телефона
		/// </summary>
		public string PhoneNumber { get; }

		public static RegisteredNaturalCounterpartyDto Create(
			Counterparty counterparty, Guid externalCounterpartyId, string emailAddress, string digitsNumber)
		{
			return new RegisteredNaturalCounterpartyDto(counterparty, externalCounterpartyId, emailAddress, digitsNumber);
		}
	}
}

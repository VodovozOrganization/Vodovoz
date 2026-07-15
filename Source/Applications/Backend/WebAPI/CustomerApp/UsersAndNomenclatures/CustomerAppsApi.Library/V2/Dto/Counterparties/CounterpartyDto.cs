using System;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Данные по клиенту
	/// </summary>
	public class CounterpartyDto
	{
		/// <summary>
		/// Наименование
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Полное наименование
		/// </summary>
		public string FullName { get; set; }
		/// <summary>
		/// Идентификатор пользователя из ИПЗ
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Идентификатор клиента из ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Имя
		/// </summary>
		public string FirstName { get; set; }
		/// <summary>
		/// Фамилия
		/// </summary>
		public string Surname { get; set; }
		/// <summary>
		/// Отчество
		/// </summary>
		public string Patronymic { get; set; }
		/// <summary>
		/// Номер телефона
		/// </summary>
		public string PhoneNumber { get; set; }
		/// <summary>
		/// Электронная почта
		/// </summary>
		public string Email { get; set; }
		/// <summary>
		/// Тип клиента <see cref="PersonType"/>
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public PersonType PersonType { get; set; }
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; set; }
		/// <summary>
		/// КПП
		/// </summary>
		public string Kpp { get; set; }
		/// <summary>
		/// Форма собственности
		/// </summary>
		public string TypeOfOwnership { get; set; }
		/// <summary>
		/// Налогобложение
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TaxType? TaxType { get; set; }
		/// <summary>
		/// Откуда клиент
		/// </summary>
		public int CameFromId { get; set; }
		/// <summary>
		/// Юридический адрес
		/// </summary>
		public string JurAddress { get; set; }
	}
}

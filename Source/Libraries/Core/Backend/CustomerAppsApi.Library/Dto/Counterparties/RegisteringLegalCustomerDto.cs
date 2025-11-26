using System;
using System.Text.Json.Serialization;
using Vodovoz.Core.Data.Interfaces.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация для регистрации юр лица в ERP
	/// </summary>
	public class RegisteringLegalCustomerDto : ILegalCounterpartyInfo
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Id пользователя
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Наименование
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Полное наименование клиента
		/// </summary>
		[JsonIgnore]
		public string FullName { get; set; }
		/// <summary>
		/// Электронная почта
		/// </summary>
		public string Email { get; set; }
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; set; }
		/// <summary>
		/// КПП
		/// </summary>
		public string Kpp { get; set; }
		/// <summary>
		/// Юр адрес
		/// </summary>
		public string JurAddress { get; set; }
		/// <summary>
		/// Полное наименование организационно-правовой формы
		/// </summary>
		public string FullTypeOfOwnership { get; set; }
		/// <summary>
		/// Сокращенное наименование ОПФ
		/// </summary>
		public string ShortTypeOfOwnership { get; set; }
		/// <summary>
		/// Код ОПФ
		/// </summary>
		public string CodeTypeOfOwnership { get; set; }
		/// <summary>
		/// Налогооблажение
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TaxType? TaxType { get; set; }
	}
}

using System;
using CustomerAppsApi.Library.Dto.Edo;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Phones
{
	/// <summary>
	/// Данные запроса на добавление телефона клиенту
	/// </summary>
	public class AddingPhoneNumberDto : IFindExternalLegalCounterpartyAccountDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Идентификатор пользователя
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Идентификатор клиента в Erp
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Номер телефона в формате 7XXXXXXXXXX
		/// </summary>
		public string PhoneNumber { get; set; }
	}
}

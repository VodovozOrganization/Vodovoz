using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Edo
{
	/// <summary>
	/// Данные для добавления ЭДО аккаунта
	/// </summary>
	public class AddingEdoAccount : IFindExternalLegalCounterpartyAccountDto
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
		/// ЭДО аккаунт
		/// </summary>
		public string EdoAccount { get; set; }
	}
}

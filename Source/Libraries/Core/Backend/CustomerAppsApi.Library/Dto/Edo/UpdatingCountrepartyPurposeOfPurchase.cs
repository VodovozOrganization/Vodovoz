using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Edo
{
	/// <summary>
	/// Данные для обновления цели покупки воды клиента
	/// </summary>
	public class UpdatingCounterpartyPurposeOfPurchase : IFindExternalLegalCounterpartyAccountDto
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
		public int CounterpartyErpId { get; set; }
		/// <summary>
		/// Цель покупки воды
		/// </summary>
		public WaterPurposeOfPurchase WaterPurposeOfPurchase { get; set; }
	}
}

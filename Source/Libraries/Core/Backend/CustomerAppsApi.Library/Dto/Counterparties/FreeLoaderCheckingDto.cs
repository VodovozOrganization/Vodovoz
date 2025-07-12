using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация для проверки халявщиков
	/// </summary>
	public class FreeLoaderCheckingDto
	{
		/// <summary>
		/// Источник запроса <see cref="Vodovoz.Core.Domain.Clients.Source"/>
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Id клиента в Erp
		/// </summary>
		public int? ErpCounterpartyId { get; set; }
		/// <summary>
		/// Id точки доставки в Erp
		/// </summary>
		public int? ErpDeliveryPointId { get; set; }
		/// <summary>
		/// Номер телефона в формате +7XXXXXXXXXX
		/// </summary>
		public string Phone { get; set; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
	}
}

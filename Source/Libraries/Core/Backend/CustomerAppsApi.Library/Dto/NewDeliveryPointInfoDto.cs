using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto
{
	/// <summary>
	/// Данные для создания новой точки доставки из ИПЗ
	/// </summary>
	public class NewDeliveryPointInfoDto : DeliveryPointInfoDto
	{
		/// <summary>
		/// Код клиента в Erp
		/// </summary>
		[Display(Name = "Клиент")]
		public int CounterpartyErpId { get; set; }
		/// <summary>
		/// Источник запроса <see cref="Vodovoz.Core.Domain.Clients.Source"/>
		/// </summary>
		public Source Source { get; set; }
	}
}

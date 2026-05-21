using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V5.Dto.Orders.OrderTemplates
{
	/// <summary>
	/// Данные для получения информации о шаблоне автозаказа
	/// </summary>
	public class GetOrderTemplateInfoDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		public int OrderTemplateId { get; set; }
	}
}

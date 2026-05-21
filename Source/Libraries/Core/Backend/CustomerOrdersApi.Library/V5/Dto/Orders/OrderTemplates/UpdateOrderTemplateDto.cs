using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V5.Dto.Orders.OrderTemplates
{
	/// <summary>
	/// Данные запроса на обновление шаблона автозаказа
	/// </summary>
	public class UpdateOrderTemplateDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Идентификатор шаблона
		/// </summary>
		public int OrderTemplateId { get; set; }
		/// <summary>
		/// Действующий шаблон
		/// </summary>
		public bool IsActive { get; set; }
		/// <summary>
		/// Архивный
		/// </summary>
		public bool IsArchive { get; set; }
	}
}

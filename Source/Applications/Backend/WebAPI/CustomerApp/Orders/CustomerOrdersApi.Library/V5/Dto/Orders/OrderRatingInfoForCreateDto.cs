using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	/// <summary>
	/// Данные для создания оценки заказа
	/// </summary>
	public class OrderRatingInfoForCreateDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Контрольная сумма оценки, для проверки валидности отправителя
		/// </summary>
		public string Signature { get; set; }
		/// <summary>
		/// Номер онлайн заказа в ERP
		/// </summary>
		public int? OnlineOrderId { get; set; }
		/// <summary>
		/// Номер заказа в ERP
		/// </summary>
		public int? OrderId { get; set; }
		/// <summary>
		/// Оценка
		/// </summary>
		public int Rating { get; set; }
		/// <summary>
		/// Комментарий к оценке
		/// </summary>
		public string Comment { get; set; }
		/// <summary>
		/// Список Id причин оценки
		/// </summary>
		public int[] OrderRatingReasonsIds { get; set; }
	}
}

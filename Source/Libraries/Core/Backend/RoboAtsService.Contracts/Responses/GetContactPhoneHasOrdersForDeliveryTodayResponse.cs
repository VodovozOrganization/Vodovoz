using System.Collections.Generic;

namespace RoboAtsService.Contracts.Responses
{
	/// <summary>
	/// Ответ есть ли заказы с контактным телефоном
	/// </summary>
	public class GetContactPhoneHasOrdersForDeliveryTodayResponse
	{
		/// <summary>
		/// Статус наличия заказов с контактным номером из запроса
		/// </summary>
		public bool Status { get; set; }

		/// <summary>
		/// Идентификаторы точек доставки с заказами на сегодня с указанным номером для связи
		/// </summary>
		public IEnumerable<int> DeliveryPointIds { get; set; }
	}
}

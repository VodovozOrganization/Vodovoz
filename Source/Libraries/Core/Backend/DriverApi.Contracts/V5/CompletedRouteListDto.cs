using System.Collections.Generic;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Завершенный маршрутный лист
	/// </summary>
	public class CompletedRouteListDto
	{
		/// <summary>
		/// Номер маршрутного листа
		/// </summary>
		public int RouteListId { get; set; }

		/// <summary>
		/// Статус маршрутного листа
		/// </summary>
		public RouteListDtoStatus RouteListStatus { get; set; }

		/// <summary>
		/// Наличные деньги
		/// </summary>
		public decimal CashMoney { get; set; }
		public decimal TerminalCardMoney { get; set; }
		public decimal TerminalQRMoney { get; set; }
		public int TerminalOrdersCount { get; set; }

		/// <summary>
		/// Полных бутылей к возврату
		/// </summary>
		public int FullBottlesToReturn { get; set; }

		/// <summary>
		/// Пустых бутылей к возврату
		/// </summary>
		public int EmptyBottlesToReturn { get; set; }

		/// <summary>
		/// Оборудование на возврат
		/// </summary>
		public IEnumerable<OrdersReturnItemDto> OrdersReturnItems { get; set; }
	}
}

using System.Collections.Generic;

namespace DriverApi.Contracts.V6
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

		/// <summary>
		/// Деньги по карте через терминал
		/// </summary>
		public decimal TerminalCardMoney { get; set; }

		/// <summary>
		/// Деньги через QR-код терминала
		/// </summary>
		public decimal TerminalQRMoney { get; set; }

		/// <summary>
		/// Количество заказов через терминал
		/// </summary>
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

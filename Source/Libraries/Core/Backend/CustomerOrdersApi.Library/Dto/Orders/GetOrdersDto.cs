using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// Информация для получения списка заказов
	/// </summary>
	public class GetOrdersDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }

		/// <summary>
		/// Контрольная сумма запроса
		/// </summary>
		public string Signature { get; set; }

		/// <summary>
		/// Id клиента из ДВ
		/// </summary>
		public int CounterpartyErpId { get; set; }

		/// <summary>
		/// Номер страницы
		/// </summary>
		public int Page { get; set; }

		/// <summary>
		/// Количество заказов для отображения на странице
		/// </summary>
		public int OrdersCountOnPage { get; set; }

		/// <summary>
		/// Статус заказа(фильтр)
		/// </summary>
		public ExternalOrderStatus? OrderStatus { get; set; }

		/// <summary>
		/// Начальная дата доставки(фильтр)
		/// </summary>
		public DateTime? StartDateTime { get; set; }

		/// <summary>
		/// Окончательная дата доставки(фильтр)
		/// </summary>
		public DateTime? EndDateTime { get; set; }

		/// <summary>
		/// Массив Id точек доставки(фильтр)
		/// </summary>
		public int[] DeliveryPointsIds { get; set; }
	}

	/// <summary>
	/// DTO для результата проверки возможности отмены заказа
	/// </summary>
	public class CancellationCheckResultDto
	{
		/// <summary>
		/// Возможна ли отмена заказа
		/// </summary>
		public bool CanCancel { get; set; }

		/// <summary>
		/// Требуется ли контакт с менеджером (для установленного маршрута)
		/// </summary>
		public bool RequireManagerContact { get; set; }

		/// <summary>
		/// Причина, если отмена невозможна
		/// </summary>
		public string ReasonMessage { get; set; }
	}

	/// <summary>
	/// DTO для результата действия с заказом
	/// </summary>
	public class OrderActionResultDto
	{
		/// <summary>
		/// ID заказа из ИПЗ
		/// </summary>
		public Guid ExternalOrderId { get; set; }

		/// <summary>
		/// Успешно ли выполнено действие
		/// </summary>
		public bool IsSuccess { get; set; }

		/// <summary>
		/// Сообщение результата
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Требуется ли связь с менеджером (для отмены при установленном маршруте)
		/// </summary>
		public bool RequireManagerContact { get; set; }

		/// <summary>
		/// Ссылка на чат с менеджером
		/// </summary>
		public string ManagerChatUrl { get; set; }
	}
}

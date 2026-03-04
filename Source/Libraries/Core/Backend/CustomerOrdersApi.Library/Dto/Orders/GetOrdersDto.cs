using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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
	/// DTO для запроса на перенос заказа
	/// </summary>
	public class TransferOrderDto
	{
		/// <summary>
		/// Источник заказа (id ИПЗ)
		/// </summary>
		[Required]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public Source Source { get; set; }

		/// <summary>
		/// ID заказа из ИПЗ
		/// </summary>
		[Required]
		public Guid ExternalOrderId { get; set; }

		/// <summary>
		/// Новая дата доставки
		/// </summary>
		[Required]
		public DateTime DeliveryDate { get; set; }

		/// <summary>
		/// ID нового интервала доставки
		/// </summary>
		[Required]
		public int DeliveryScheduleId { get; set; }

		/// <summary>
		/// Контрольная сумма (подпись)
		/// </summary>
		[Required]
		public string Signature { get; set; }
	}

	/// <summary>
	/// DTO для запроса на отмену заказа
	/// </summary>
	public class CancelOrderDto
	{
		/// <summary>
		/// Источник заказа (id ИПЗ)
		/// </summary>
		[Required]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public Source Source { get; set; }

		/// <summary>
		/// ID заказа из ИПЗ
		/// </summary>
		[Required]
		public Guid ExternalOrderId { get; set; }

		/// <summary>
		/// Тип оплаты онлайн заказа
		/// </summary>
		[Required]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public OnlineOrderPaymentType OnlineOrderPaymentType { get; set; }

		/// <summary>
		/// Статус оплаты онлайн заказа
		/// </summary>
		[Required]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public OnlineOrderPaymentStatus OnlineOrderPaymentStatus { get; set; }

		/// <summary>
		/// ID транзакции из CloudPayments (если применимо)
		/// </summary>
		public int? TransactionId { get; set; }

		/// <summary>
		/// Сумма заказа
		/// </summary>
		[Required]
		public decimal OrderSum { get; set; }

		/// <summary>
		/// Контрольная сумма (подпись)
		/// </summary>
		[Required]
		public string Signature { get; set; }
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

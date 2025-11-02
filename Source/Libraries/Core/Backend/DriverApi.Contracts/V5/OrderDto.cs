using System;
using System.Collections.Generic;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Заказ
	/// </summary>
	public class OrderDto
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Статус оплаты по смс
		/// </summary>
		public SmsPaymentDtoStatus? SmsPaymentStatus { get; set; }

		/// <summary>
		/// Статус оплаты по QR-коду
		/// </summary>
		public QrPaymentDtoStatus? QRPaymentStatus { get; set; }

		/// <summary>
		/// Время доставки
		/// </summary>
		public string DeliveryTime { get; set; }

		/// <summary>
		/// Количество полных бутылей
		/// </summary>
		public int FullBottleCount { get; set; }

		/// <summary>
		/// Количество пустых бутылей
		/// </summary>
		public int EmptyBottlesToReturn { get; set; }

		/// <summary>
		/// Возвращаемое количество бутылей по акции "бутыль"
		/// </summary>
		public int BottlesByStockActualCount { get; set; }

		/// <summary>
		/// Имя контрагента
		/// </summary>
		public string Counterparty { get; set; }

		/// <summary>
		/// Телефонные номера
		/// </summary>
		public IEnumerable<PhoneDto> PhoneNumbers { get; set; }

		/// <summary>
		/// Тип оплаты
		/// </summary>
		public PaymentDtoType PaymentType { get; set; }

		/// <summary>
		/// Адрес
		/// </summary>
		public AddressDto Address { get; set; }

		/// <summary>
		/// Комментарий к заказу
		/// </summary>
		public string OrderComment { get; set; }

		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum { get; set; }

		/// <summary>
		/// Доставка за час
		/// </summary>
		public bool IsFastDelivery { get; set; }

		/// <summary>
		/// Бесконтактная доставка
		/// </summary>
		public bool ContactlessDelivery { get; set; }

		/// <summary>
		/// Врмемя добавления в маршрутный лист
		/// </summary>
		public string AddedToRouteListTime { get; set; }

		/// <summary>
		/// Товары на продажу
		/// </summary>
		public IEnumerable<OrderSaleItemDto> OrderSaleItems { get; set; }

		/// <summary>
		/// Оборудование для доставки
		/// </summary>
		public IEnumerable<OrderDeliveryItemDto> OrderDeliveryItems { get; set; }

		/// <summary>
		/// Оборудование для забора
		/// </summary>
		public IEnumerable<OrderReceptionItemDto> OrderReceptionItems { get; set; }

		/// <summary>
		/// Дополнительная информация по заказу
		/// </summary>
		public OrderAdditionalInfoDto OrderAdditionalInfo { get; set; }

		/// <summary>
		/// Сдача
		/// </summary>
		public int Trifle { get; set; }

		/// <summary>
		/// Подписание документов
		/// </summary>
		public SignatureDtoType? SignatureType { get; set; }

		/// <summary>
		/// Отзвон за
		/// </summary>
		public int? CallBeforeArrivalMinutes { get; set; }

		/// <summary>
		/// Ожидание до
		/// </summary>
		public TimeSpan? WaitUntilTime { get; set; }
	}
}

using System;
using System.Collections.Generic;
using SmsPaymentDtoStatus = DriverAPI.Library.DTOs.SmsPaymentDtoStatus;
using QRPaymentDTOStatus = DriverAPI.Library.DTOs.QRPaymentDTOStatus;
using PhoneDto = DriverAPI.Library.DTOs.PhoneDto;
using AddressDto = DriverAPI.Library.DTOs.AddressDto;
using OrderSaleItemDto = DriverAPI.Library.DTOs.OrderSaleItemDto;
using OrderDeliveryItemDto = DriverAPI.Library.DTOs.OrderDeliveryItemDto;
using OrderReceptionItemDto = DriverAPI.Library.DTOs.OrderReceptionItemDto;
using SignatureDtoType = DriverAPI.Library.DTOs.SignatureDtoType;

namespace DriverAPI.Library.Deprecated2.DTOs
{
	/// <summary>
	/// Заказ
	/// </summary>
	[Obsolete("Будет удален с прекращением поддержки API v2")]
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
		public QRPaymentDTOStatus? QRPaymentStatus { get; set; }

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
	}
}

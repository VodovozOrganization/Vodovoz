using System;

namespace Vodovoz.ViewModels.Dialogs.Mango.Talks
{
	/// <summary>
	/// Строка списка заказов клиента для перевода звонка на водителя
	/// </summary>
	public class DriverForwardingOrderNode
	{
		private const string _deliveryDateFormat = "dd.MM.yyyy";
		private const string _emptyValueText = "—";

		/// <summary>
		/// Номер заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime? DeliveryDate { get; set; }

		/// <summary>
		/// Адрес доставки
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// Статус заказа
		/// </summary>
		public string OrderStatusTitle { get; set; }

		/// <summary>
		/// Фамилия и инициалы водителя, доставляющего заказ
		/// </summary>
		public string DriverName { get; set; }

		/// <summary>
		/// Активный добавочный номер Манго водителя
		/// </summary>
		public int? DriverExtensionNumber { get; set; }

		/// <summary>
		/// Дата доставки для отображения
		/// </summary>
		public string DeliveryDateText => DeliveryDate?.ToString(_deliveryDateFormat) ?? _emptyValueText;

		/// <summary>
		/// Добавочный номер водителя для отображения
		/// </summary>
		public string DriverExtensionNumberText => DriverExtensionNumber?.ToString() ?? _emptyValueText;

		/// <summary>
		/// Можно ли перевести звонок на водителя по этому заказу
		/// </summary>
		public bool CanForwardCall => DriverExtensionNumber.HasValue;

		/// <summary>
		/// Причина, по которой перевод звонка на водителя недоступен.
		/// Пустая строка, если перевод доступен
		/// </summary>
		public string ForwardingUnavailableReason
		{
			get
			{
				if(string.IsNullOrWhiteSpace(DriverName))
				{
					return "Заказ не в маршрутном листе";
				}

				if(!DriverExtensionNumber.HasValue)
				{
					return "У водителя нет активного добавочного номера Манго";
				}

				return string.Empty;
			}
		}
	}
}

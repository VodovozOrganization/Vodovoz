using Gamma.Utilities;
using QS.Utilities.Text;
using System;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Заказ контрагента в пути с данными водителя, доставляющего этот заказ.
	/// Используется для перевода звонка на добавочный номер водителя
	/// </summary>
	public class DriverForwardingOrderNode
	{
		private const string _deliveryDateFormat = "dd.MM.yyyy";
		private const string _emptyValueText = "—";

		/// <summary>
		/// Id заказа
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
		public OrderStatus OrderStatus { get; set; }

		/// <summary>
		/// Id водителя, доставляющего заказ. <see langword="null"/>, если заказа нет в маршрутном листе
		/// </summary>
		public int? DriverId { get; set; }

		/// <summary>
		/// Фамилия водителя
		/// </summary>
		public string DriverLastName { get; set; }

		/// <summary>
		/// Имя водителя
		/// </summary>
		public string DriverFirstName { get; set; }

		/// <summary>
		/// Отчество водителя
		/// </summary>
		public string DriverPatronymic { get; set; }

		/// <summary>
		/// Активный добавочный номер Манго водителя.
		/// <see langword="null"/>, если добавочного номера нет
		/// </summary>
		public int? DriverExtensionNumber { get; set; }

		/// <summary>
		/// Название статуса заказа
		/// </summary>
		public string OrderStatusTitle => OrderStatus.GetEnumTitle();

		/// <summary>
		/// Дата доставки для отображения
		/// </summary>
		public string DeliveryDateText => DeliveryDate?.ToString(_deliveryDateFormat) ?? _emptyValueText;

		/// <summary>
		/// Фамилия и инициалы водителя
		/// </summary>
		public string DriverName =>
			DriverId.HasValue
				? PersonHelper.PersonNameWithInitials(DriverLastName, DriverFirstName, DriverPatronymic)
				: string.Empty;

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
				if(!DriverId.HasValue)
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

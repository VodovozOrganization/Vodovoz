using System;
using System.Runtime.Serialization;
using Gamma.Utilities;
using Vodovoz.Domain.Logistic;

namespace Android
{
	[DataContract]
	public class ShortOrderDTO
	{
		[DataMember]
		public int Id;

		//Расписание доставки
		[DataMember]
		public string DeliverySchedule;

		//Статус заказа
		[DataMember]
		public string OrderStatus;

		//Контрагент
		[DataMember]
		public string Counterparty;

		//Адрес
		[DataMember]
		public string Address;

		public ShortOrderDTO (RouteListItem item)
		{
			Id = item.Order.Id;
			DeliverySchedule = item.Order?.DeliverySchedule?.DeliveryTime ?? String.Empty;
			OrderStatus = item.Status.GetEnumTitle();
			Counterparty = item.Order?.Client?.FullName ?? String.Empty;
			Address = item.Order?.DeliveryPoint?.ShortAddress ?? String.Empty;
		}
	}
}


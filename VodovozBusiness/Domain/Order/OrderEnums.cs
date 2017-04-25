using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{

	public enum OrderStatus
	{
		[Display (Name = "Новый")]
		NewOrder,
		[Display (Name = "Отменён")]
		Canceled,
		[Display (Name = "Ожидание оплаты")]
		WaitForPayment,
		[Display (Name = "Принят")]
		Accepted,
		[Display (Name = "В маршрутном листе")]
		InTravelList,
		[Display (Name = "На погрузке")]
		OnLoading,
		[Display (Name = "В пути")]
		OnTheWay,
		[Display (Name = "Доставка отменена")]
		DeliveryCanceled,
		[Display (Name = "Доставлен")]
		Shipped,
		[Display (Name = "Выгрузка на складе")]
		UnloadingOnStock,
		[Display (Name = "Недовоз")]
		NotDelivered,
		[Display (Name = "Закрыт")]
		Closed,
	}

	public class OrderStatusStringType : NHibernate.Type.EnumStringType
	{
		public OrderStatusStringType () : base (typeof(OrderStatus))
		{
		}
	}

	public enum OrderSignatureType
	{
		[Display (Name = "По печати")]
		BySeal,
		[Display (Name = "По доверенности")]
		ByProxy,
		[Display (Name = "Доверенность на адресе")]
		ProxyOnDeliveryPoint
	}

	public class OrderSignatureTypeStringType : NHibernate.Type.EnumStringType
	{
		public OrderSignatureTypeStringType () : base (typeof(OrderSignatureType))
		{
		}
	}

}

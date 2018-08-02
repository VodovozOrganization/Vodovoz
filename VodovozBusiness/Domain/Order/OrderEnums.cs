using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{

	public enum OrderStatus
	{
		[Display(Name = "Отменён")]
		Canceled,

		// При смене имени статуса NewOrder не забыть сменить имя также и 
		// в тригере на таблице заказов в моделе БД
		[Display (Name = "Новый")]
		NewOrder,

		// При смене имени статуса WaitForPayment не забыть сменить имя также и 
		// в тригере на таблице заказов в моделе БД
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

	public enum ReasonType
	{
		[Display(Name = "Причина неизвестна")]
		Unknown,
		[Display(Name = "Новый адрес")]
		NewAddress,
		[Display(Name = "Увеличение заказа")]
		OrderIncrease,
		[Display(Name = "Первый заказ")]
		FirstOrder
	}

	public class ReasonTypeStringType : NHibernate.Type.EnumStringType
	{
		public ReasonTypeStringType() : base(typeof(ReasonType))
		{
		}
	}

	public enum DriverCallType
	{
		[Display(Name = "Водитель не звонил")]
		NoCall,
		[Display(Name = "Водитель отзвонился с адреса")]
		CallFromAddress,
		[Display(Name = "Водитель отзвонился не с адреса")]
		CallFromAnywhere
	}

	public class DriverCallTypeStringType : NHibernate.Type.EnumStringType
	{
		public DriverCallTypeStringType() : base(typeof(DriverCallType))
		{
		}
	}

	public enum DiscountUnits
	{
		[Display(Name = "₽")]
		money,
		[Display(Name = "%")]
		percent
	}
}

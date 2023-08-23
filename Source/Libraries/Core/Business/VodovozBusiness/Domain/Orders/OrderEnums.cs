using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	[Appellative(
		Nominative = "Статус заказа",
		NominativePlural = "Статусы заказа")]
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

	public enum OrderPaymentStatus
	{
		[Display(Name = "Нет")]
		None,
		[Display(Name = "Оплачен")]
		Paid,
		[Display(Name = "Частично оплачен")]
		PartiallyPaid,
		[Display(Name = "Не оплачен")]
		UnPaid
	}

	public class OrderPaymentStatusStringType : NHibernate.Type.EnumStringType
	{
		public OrderPaymentStatusStringType() : base(typeof(OrderPaymentStatus))
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
		ProxyOnDeliveryPoint,
        [Display (Name = "Подпись/расшифровка")]
        SignatureTranscript
    }

	public class OrderSignatureTypeStringType : NHibernate.Type.EnumStringType
	{
		public OrderSignatureTypeStringType () : base (typeof(OrderSignatureType))
		{
		}
	}

	public enum OrderAddressType
	{
		[Display(Name = "Обычная доставка")]
		Delivery,
		[Display(Name = "Сервисное обслуживание")]
		Service,
		[Display(Name = "Сетевой магазин")]
		ChainStore,
		[Display(Name = "Складская логистика")]
		StorageLogistics
	}
	
	public class OrderAddressTypeStringType : NHibernate.Type.EnumStringType
	{
		public OrderAddressTypeStringType () : base (typeof(OrderAddressType))
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
		CallFromAnywhere,
		[Display(Name = "Комментарий загружен из приложения")]
		CommentFromMobileApp,
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

	public enum OrderSource
	{
		[Display(Name = "Диспетчер")]
		VodovozApp,
		[Display(Name = "Интернет магазин")]
		OnlineStore,
		[Display(Name = "Мобильное приложение")]
		MobileApp
	}

	public class OrderSourceStringType : NHibernate.Type.EnumStringType
	{
		public OrderSourceStringType() : base(typeof(OrderSource))
		{
		}
	}

	/// <summary>
	/// Используется для заполнения комбобоксов
	/// </summary>
	public enum OrderDateType
	{
		[Display(Name = "Дата создания")]
		CreationDate,
		[Display(Name = "Дата доставки")]
		DeliveryDate
	}
}

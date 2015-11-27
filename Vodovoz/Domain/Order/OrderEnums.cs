using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{

	public enum OrderStatus
	{
		[Display (Name = "Новый")]
		[ItemTitleAttribute ("Новый")] 
		NewOrder,
		[Display (Name = "Принят")]
		[ItemTitleAttribute ("Принят")] 
		Accepted,
		[Display (Name = "В маршрутном листе")]
		[ItemTitleAttribute ("В маршрутном листе")]
		InTravelList,
		[Display (Name = "На погрузке")]
		[ItemTitleAttribute ("На погрузке")]
		OnLoading,
		[Display (Name = "В пути")]
		[ItemTitleAttribute ("В пути")]
		OnTheWay,
		[Display (Name = "Доставлен")]
		[ItemTitleAttribute ("Доставлен")]
		Shipped,
		[Display (Name = "Выгрузка на складе")]
		[ItemTitleAttribute ("Выгрузка на складе")]
		UnloadingOnStock,
		[Display (Name = "Отчет не закрыт")]
		[ItemTitleAttribute ("Отчет не закрыт")]
		ReportNotClosed,
		[Display (Name = "Закрыт")]
		[ItemTitleAttribute ("Закрыт")]
		Closed,
		[Display (Name = "Отменен")]
		[ItemTitleAttribute ("Отменен")]
		Canceled,
		[Display (Name = "Недовоз")]
		[ItemTitleAttribute ("Недовоз")]
		NotDelivered
	}

	public class OrderStatusStringType : NHibernate.Type.EnumStringType
	{
		public OrderStatusStringType () : base (typeof(OrderStatus))
		{
		}
	}

	public enum OrderSignatureType
	{
		//FIXME Удалить ItemTitleAttribute после полного перехода на GammaBinding
		[Display (Name = "По печати")]
		[ItemTitleAttribute ("По печати")]
		BySeal,
		[Display (Name = "По доверенности")]
		[ItemTitleAttribute ("По доверенности")]
		ByProxy,
		[Display (Name = "Доверенность на адресе")]
		[ItemTitleAttribute ("Доверенность на адресе")]
		ProxyOnDeliveryPoint
	}

	public class OrderSignatureTypeStringType : NHibernate.Type.EnumStringType
	{
		public OrderSignatureTypeStringType () : base (typeof(OrderSignatureType))
		{
		}
	}

}

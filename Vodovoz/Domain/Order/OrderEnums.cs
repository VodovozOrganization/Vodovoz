using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{

	public enum OrderStatus
	{
		[Display (Name = "Новый")]
		NewOrder,
		[Display (Name = "Принят")]
		Accepted,
		[Display (Name = "В маршрутном листе")]
		InTravelList,
		[Display (Name = "Готов к отгрузке")]
		ReadyForShipment,
		[Display (Name = "В пути")]
		OnTheWay,
		[Display (Name = "Доставлен")]
		Shipped,
		[Display (Name = "Выгрузка на складе")]
		UnloadingOnStock,
		[Display (Name = "Отчет не закрыт")]
		ReportNotClosed,
		[Display (Name = "Закрыт")]
		Closed,
		[Display (Name = "Отменен")]
		Canceled,
		[Display (Name = "Недовоз")]
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

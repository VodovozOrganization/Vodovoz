using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Logistic
{

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "адреса маршрутного листа",
		Nominative = "адрес маршрутного листа")]
	public class RouteListItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Orders.Order order;

		[Display (Name = "Заказ")]
		public virtual Orders.Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}

		RouteList routeList;

		[Display (Name = "Маршрутный лист")]
		public virtual RouteList RouteList {
			get { return routeList; }
			set { 
				SetField (ref routeList, value, () => RouteList); 
			}
		}

		public RouteListItem ()
		{
		}
			
		//Конструктор создания новой строки
		public RouteListItem (RouteList routeList, Order order)
		{
			this.routeList = routeList;
			if(order.OrderStatus == OrderStatus.Accepted)
			{
				this.order = order;
				order.OrderStatus = OrderStatus.InTravelList;
			}
		}

		public void RemovedFromRoute()
		{
			Order.OrderStatus = OrderStatus.Accepted;
		}
	}
}


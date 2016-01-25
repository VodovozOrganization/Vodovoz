using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QSOrmProject;
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

		int indexInRoute;

		public int IndexInRoute {
			get {
				return indexInRoute;
			}
			set {
				SetField (ref indexInRoute, value, () => IndexInRoute); 
			}
		}

		int bottlesReturned;

		public int BottlesReturned{
			get{
				return bottlesReturned; 
			}
			set{ 
				SetField(ref bottlesReturned, value, () => BottlesReturned);	
			}
		}

		decimal depositsCollected;

		public decimal DepositsCollected{
			get{
				return depositsCollected; 
			}
			set{ 
				SetField(ref depositsCollected, value, () => DepositsCollected);	
			}
		}

		decimal totalPrice;

		public decimal TotalPrice{
			get{
				return totalPrice;
			}
			set{
				SetField(ref totalPrice, value, () => TotalPrice);
			}

		}

		private Dictionary<int, int> goodsByRouteColumns;

		public Dictionary<int, int> GoodsByRouteColumns{
			get {
				if(goodsByRouteColumns == null)
				{
					goodsByRouteColumns = Order.OrderItems.Where (i => i.Nomenclature.RouteListColumn != null)
						.GroupBy (i => i.Nomenclature.RouteListColumn.Id, i => i.Count)
						.ToDictionary (g => g.Key, g => g.Sum ());
				}
				return goodsByRouteColumns;
			}
		}

		public int GetGoodsAmountForColumn(int columnId)
		{
			if (GoodsByRouteColumns.ContainsKey (columnId))
				return GoodsByRouteColumns [columnId];
			else
				return 0;
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


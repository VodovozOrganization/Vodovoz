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

		RouteListItemStatus status;

		public virtual RouteListItemStatus Status {
			get{ return status; }
			set{
				SetField(ref status, value, () => Status);
			}
		}

		DateTime? statusLastUpdate;
		public virtual DateTime? StatusLastUpdate {
			get{ return statusLastUpdate; }
			set{
				SetField(ref statusLastUpdate, value, () => StatusLastUpdate);
			}
		}

		bool withoutForwarder;

		public bool WithoutForwarder{
			get{
				return withoutForwarder;
			}
			set{
				SetField(ref withoutForwarder, value, () => WithoutForwarder);
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

		decimal totalCash;

		public decimal TotalCash{
			get{
				return totalCash;
			}
			set{
				SetField(ref totalCash, value, () => TotalCash);
			}
		}

		decimal driverWage;
		public virtual decimal DriverWage{
			get{
				return driverWage;	
			}
			set{ 
				SetField(ref driverWage, value, () => DriverWage);
			}
		}

		decimal defaultDriverWage=-1;
		public virtual decimal DefaultDriverWage{
			get{ 
				if (defaultDriverWage == -1) {
					defaultDriverWage = DriverWage;
				}
				return defaultDriverWage; 
			}
			set{
				SetField(ref defaultDriverWage, value, () => DefaultDriverWage);
			}
		}

		public bool HasUserSpecifiedDriverWage(){
			return DriverWage != DefaultDriverWage;
		}

		decimal forwarderWage;
		public virtual decimal ForwarderWage{
			get{
				return forwarderWage;
			}
			set{ 
				SetField(ref forwarderWage, value, () => ForwarderWage);
			}
		}

		decimal defaultForwarderWage=-1;
		public virtual decimal DefaultForwarderWage{
			get{
				if (defaultForwarderWage == -1) {
					defaultForwarderWage = ForwarderWage;
				}
				return defaultForwarderWage;
			}
			set{
				SetField(ref defaultForwarderWage, value, () => DefaultForwarderWage);
			}
		}


		public virtual bool HasUserSpecifiedForwarderWage(){
			return ForwarderWage != DefaultForwarderWage;
		}

		public virtual void RecalculateWages()
		{
			if (!HasUserSpecifiedDriverWage())
			{
				DriverWage = CalculateDriverWage();
				DefaultDriverWage = DriverWage;
			}
			if (!HasUserSpecifiedForwarderWage())
			{
				ForwarderWage = CalculateForwarderWage();
				DefaultForwarderWage = ForwarderWage;
			}
		}

		public virtual decimal CalculateDriverWage(){
			bool withForwarder = RouteList.Forwarder!=null;
			var rates = Wages.GetDriverRates(withForwarder);

			var firstOrderForAddress = RouteList.Addresses
				.Select(item => item.Order)
				.First(ord => ord.DeliveryPoint.Id == Order.DeliveryPoint.Id).Id == Order.Id;

			var paymentForAddress = firstOrderForAddress ? rates.PaymentPerAddress : 0;

			var fullBottleCount = Order.OrderItems
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
				.Sum(item => item.Count);
			bool largeOrder = fullBottleCount >= rates.LargeOrderMinimumBottles;

			var bottleCollectionOrder = Order.CollectBottles;

			decimal paymentPerEmptyBottle = largeOrder 
				? rates.LargeOrderEmptyBottleRate 
				: rates.EmptyBottleRate;
			var largeFullBottlesPayment = largeOrder 
				? fullBottleCount * rates.LargeOrderFullBottleRate 
				: fullBottleCount * rates.FullBottleRate;

			var payForEquipment = fullBottleCount == 0
				&& (Order.OrderEquipments.Count(item => item.Direction == Direction.Deliver) > 0
					|| bottleCollectionOrder);
			var equpmentPayment = payForEquipment ? rates.CoolerRate : 0;

			var contractCancelationPayment = bottleCollectionOrder ? rates.ContractCancelationRate : 0;
			var emptyBottlesPayment = bottleCollectionOrder ? 0 : paymentPerEmptyBottle*bottlesReturned;
			var smallFullBottlesPayment = rates.SmallFullBottleRate*Order.OrderItems.Count(item=>item.NomenclatureString=="Вода 6л");

			var wage = equpmentPayment + largeFullBottlesPayment
			           + contractCancelationPayment + emptyBottlesPayment
			           + smallFullBottlesPayment + paymentForAddress;
			
			return wage;
		}

		public virtual decimal CalculateForwarderWage(){
			var rates = Wages.GetForwarderRates();
			var fullBottleCount = Order.OrderItems
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
				.Sum(item => item.Count);
			if (WithoutForwarder)
				return 0;
			bool largeOrder = fullBottleCount >= rates.LargeOrderMinimumBottles;

			var bottleCollectionOrder = Order.CollectBottles;

			decimal paymentPerEmptyBottle = largeOrder 
				? rates.LargeOrderEmptyBottleRate 
				: rates.EmptyBottleRate;
			var largeFullBottlesPayment = largeOrder 
				? fullBottleCount * rates.LargeOrderFullBottleRate 
				: fullBottleCount * rates.FullBottleRate;
			
			var payForEquipment = fullBottleCount == 0
			                   && (Order.OrderEquipments.Count(item => item.Direction == Direction.Deliver) > 0
			                   || bottleCollectionOrder);
			var equpmentPayment = payForEquipment ? rates.CoolerRate : 0;

			var contractCancelationPayment = bottleCollectionOrder ? rates.ContractCancelationRate : 0;
			var emptyBottlesPayment = bottleCollectionOrder ? 0 : paymentPerEmptyBottle*bottlesReturned;
			var smallFullBottlesPayment = rates.SmallFullBottleRate*Order.OrderItems.Count(item=>item.NomenclatureString=="Вода 6л"); //TODO Fix

			var wage = equpmentPayment + largeFullBottlesPayment 
				+ contractCancelationPayment + emptyBottlesPayment 
				+ smallFullBottlesPayment;
			
			return wage;
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

	public enum RouteListItemStatus{
		[Display(Name="В пути")]
		EnRoute,
		[Display(Name="Выполнен")]
		Completed,
		[Display(Name="Отмена клиентом")]
		Canceled,
		[Display(Name="Опоздали")]
		Overdue
	}

	public class RouteListItemStatusStringType : NHibernate.Type.EnumStringType
	{
		public RouteListItemStatusStringType () : base (typeof(RouteListItemStatus))
		{
		}
	}

	public class Wages{
		public static Rates GetDriverRates(bool withForwarder=false){
			return new Rates
			{
				PhoneServiceCompensationRate = 2,
				FullBottleRate = withForwarder ? 10 : 15,
				EmptyBottleRate = 5,
				CoolerRate = withForwarder ? 20 : 30,
				PaymentPerAddress = 50,
				LargeOrderFullBottleRate = withForwarder ? 7 : 9,
				LargeOrderEmptyBottleRate = 1,
				LargeOrderMinimumBottles = 100,
				SmallFullBottleRate = withForwarder ? 2 : 3,
				ContractCancelationRate = withForwarder ? 20 : 30,
			};
		}

		public static Rates GetForwarderRates(){
			return new Rates
			{
				 PhoneServiceCompensationRate=0,
				 FullBottleRate = 5,
				 EmptyBottleRate = 5,
				 CoolerRate = 10,
				 LargeOrderFullBottleRate = 4,
				 LargeOrderEmptyBottleRate = 1,
				 SmallFullBottleRate = 1,
				 LargeOrderMinimumBottles = 100,
				 ContractCancelationRate = 10
			};
		}

		public class Rates{
			public decimal PhoneServiceCompensationRate;
			public decimal FullBottleRate;
			public decimal EmptyBottleRate;
			public decimal CoolerRate;
			public decimal PaymentPerAddress;
			public decimal LargeOrderFullBottleRate;
			public decimal LargeOrderEmptyBottleRate;
			public int LargeOrderMinimumBottles;
			public decimal SmallFullBottleRate;
			public decimal ContractCancelationRate;
		}
	}				
}


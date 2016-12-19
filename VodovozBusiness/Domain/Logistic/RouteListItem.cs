using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QSOrmProject;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Logistic
{

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "адреса маршрутного листа",
		Nominative = "адрес маршрутного листа")]
	public class RouteListItem : PropertyChangedBase, IDomainObject
	{
		#region Свойства

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

		string comment;
		public virtual string Comment
		{
			get{ return comment; }
			set
			{
				SetField(ref comment, value, () => Comment);
			}
		}

		bool withForwarder;

		public virtual bool WithForwarder{
			get{
				return withForwarder;
			}
			set{
				SetField(ref withForwarder, value, () => WithForwarder);
			}
		}

		int indexInRoute;

		public virtual int IndexInRoute {
			get {
				return indexInRoute;
			}
			set {
				SetField (ref indexInRoute, value, () => IndexInRoute); 
			}
		}

		int bottlesReturned;

		public virtual int BottlesReturned{
			get{
				return bottlesReturned; 
			}
			set{ 
				SetField(ref bottlesReturned, value, () => BottlesReturned);	
			}
		}

		int? driverBottlesReturned;

		public virtual int? DriverBottlesReturned{
			get{
				return driverBottlesReturned; 
			}
			set{ 
				SetField(ref driverBottlesReturned, value, () => DriverBottlesReturned);	
			}
		}

		decimal depositsCollected;

		public virtual decimal DepositsCollected{
			get{
				return depositsCollected; 
			}
			set{ 
				SetField(ref depositsCollected, value, () => DepositsCollected);	
			}
		}

		decimal totalCash;

		public virtual decimal TotalCash{
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

		decimal defaultTotalCash=-1;
		public virtual decimal DefaultTotalCash{
			get{ 
				if (defaultTotalCash == -1) {
					defaultTotalCash = TotalCash;
				}
				return defaultTotalCash; 
			}
			set{
				SetField(ref defaultTotalCash, value, () => DefaultTotalCash);
			}
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

		#endregion

		#region Расчетные

		public virtual bool HasUserSpecifiedForwarderWage(){
			return ForwarderWage != DefaultForwarderWage;
		}

		public virtual bool HasUserSpecifiedDriverWage(){
			return DriverWage != DefaultDriverWage;
		}

		public virtual bool HasUserSpecifiedTotalCash(){
			return TotalCash != DefaultTotalCash;
		}

		public virtual string Title{
			get{
				return String.Format("Адрес в МЛ {0}", Order.DeliveryPoint.CompiledAddress);
			}
		}

		#endregion

		public virtual void UpdateStatus(RouteListItemStatus status)
		{
			if(Status != status)
				StatusLastUpdate = DateTime.Now;
			Status = status;
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

		public virtual void RecalculateTotalCash()
		{
			if (!HasUserSpecifiedTotalCash())
			{
				TotalCash = CalculateTotalCash();
				DefaultTotalCash = TotalCash;
			}
		}

		public virtual decimal CalculateDriverWage(){
			bool withForwarder = RouteList.Forwarder!=null;
			var rates = Wages.GetDriverRates(withForwarder);

			if (!IsDelivered())
				return 0;
			
			var firstOrderForAddress = RouteList.Addresses
				.Where(address=>address.IsDelivered())
				.Select(item => item.Order)
				.First(ord => ord.DeliveryPoint.Id == Order.DeliveryPoint.Id).Id == Order.Id;

			var paymentForAddress = firstOrderForAddress ? rates.PaymentPerAddress : 0;

			var fullBottleCount = Order.OrderItems
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
				.Sum(item => item.ActualCount);
			bool largeOrder = fullBottleCount >= rates.LargeOrderMinimumBottles;

			var bottleCollectionOrder = Order.CollectBottles;

			decimal paymentPerEmptyBottle = largeOrder 
				? rates.LargeOrderEmptyBottleRate 
				: rates.EmptyBottleRate;
			var largeFullBottlesPayment = largeOrder 
				? fullBottleCount * rates.LargeOrderFullBottleRate 
				: fullBottleCount * rates.FullBottleRate;

			var payForEquipment = fullBottleCount == 0
				&& (Order.OrderEquipments.Count(item => item.Direction == Direction.Deliver && item.Confirmed) > 0
					|| bottleCollectionOrder);
			var equpmentPayment = payForEquipment ? rates.CoolerRate : 0;

			var contractCancelationPayment = bottleCollectionOrder ? rates.ContractCancelationRate : 0;
			var emptyBottlesPayment = bottleCollectionOrder ? 0 : paymentPerEmptyBottle*bottlesReturned;
			var smallFullBottlesPayment = rates.SmallFullBottleRate*Order.OrderItems.Count(item=>item.Nomenclature.Category==NomenclatureCategory.disposableBottleWater);

			var wage = equpmentPayment + largeFullBottlesPayment
			           + contractCancelationPayment + emptyBottlesPayment
			           + smallFullBottlesPayment + paymentForAddress;

			#if SHORT

			var payForEquipmentShort = fullBottleCount == 0
					&& (!(Order.ToClientText.ToLower().Contains("раст") 
					&& string.IsNullOrWhiteSpace(Order.ToClientText))
					|| bottleCollectionOrder);
			var equpmentPaymentShort = payForEquipmentShort ? rates.CoolerRate : 0;

			wage += equpmentPaymentShort;
			#endif
			
			return wage;
		}

		public virtual decimal CalculateForwarderWage(){
			var rates = Wages.GetForwarderRates();
			var fullBottleCount = Order.OrderItems
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
				.Sum(item => item.ActualCount);
			if (!WithForwarder || RouteList.Forwarder==null)
				return 0;

			if (!IsDelivered())
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
				&& (Order.OrderEquipments.Count(item => item.Direction == Direction.Deliver && item.Confirmed) > 0
			                   || bottleCollectionOrder);
			var equpmentPayment = payForEquipment ? rates.CoolerRate : 0;

			var contractCancelationPayment = bottleCollectionOrder ? rates.ContractCancelationRate : 0;
			var emptyBottlesPayment = bottleCollectionOrder ? 0 : paymentPerEmptyBottle*bottlesReturned;
			var smallFullBottlesPayment = rates.SmallFullBottleRate*Order.OrderItems.Count(item=>item.Nomenclature.Category==NomenclatureCategory.disposableBottleWater);

			var wage = equpmentPayment + largeFullBottlesPayment 
				+ contractCancelationPayment + emptyBottlesPayment 
				+ smallFullBottlesPayment;
			
			return wage;
		}

		public virtual decimal CalculateTotalCash()
		{
			if (!IsDelivered())
				return 0;

			return Order.OrderItems.Sum(item => item.ActualCount * item.Price * (1 - item.Discount/100));
		}

		public virtual int CoolersToClient{
			get{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PlannedCoolersToClient{
			get{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PumpsToClient{
			get{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int PlannedPumpsToClient{
			get{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int UncategorisedEquipmentToClient{
			get{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.WithoutCard);
			}
		}

		public virtual int PlannedUncategorisedEquipmentToClient{
			get{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.WithoutCard);
			}
		}			

		public virtual int CoolersFromClient{
			get{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Confirmed).Where(item=>item.Equipment!=null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PumpsFromClient{
			get{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Confirmed).Where(item=>item.Equipment!=null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int PlannedCoolersFromClient{
			get{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item=>item.Equipment!=null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PlannedPumpsFromClient{
			get{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item=>item.Equipment!=null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual bool IsDelivered()
		{
			var routeListUnloaded = (RouteList.Status == RouteListStatus.OnClosing) || 
				(RouteList.Status == RouteListStatus.MileageCheck) ||
				(RouteList.Status==RouteListStatus.Closed);
			return Status == RouteListItemStatus.Completed || Status == RouteListItemStatus.EnRoute && routeListUnloaded; 
		}

		public virtual int GetFullBottlesDeliveredCount(){
			return Order.OrderItems.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
				.Sum(item => item.ActualCount);
		}

		private Dictionary<int, int> goodsByRouteColumns;

		public virtual Dictionary<int, int> GoodsByRouteColumns{
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

		public virtual int GetGoodsAmountForColumn(int columnId)
		{
			if (GoodsByRouteColumns.ContainsKey (columnId))
				return GoodsByRouteColumns [columnId];
			else
				return 0;
		}

		public virtual int GetGoodsActualAmountForColumn(int columnId)
		{
			return Order.OrderItems.Where(i => i.Nomenclature.RouteListColumn != null)
				.Where(i => i.Nomenclature.RouteListColumn.Id == columnId)
				.Sum(i => i.ActualCount);
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
			if (routeList.Status == RouteListStatus.EnRoute)
			{
				this.Order = order;
			}
		}

		public virtual void RemovedFromRoute()
		{
			Order.OrderStatus = OrderStatus.Accepted;
		}

		#region Проброс полей для редактирования в заказе.
		public virtual string FromClientText {
			get{
				return Order.FromClientText;
			}
			set{
				Order.FromClientText = value;
			}
		}

		public virtual string ToClientText {
			get{
				return Order.ToClientText;
			}
			set{
				Order.ToClientText = value;
			}
		}

		#endregion
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
}


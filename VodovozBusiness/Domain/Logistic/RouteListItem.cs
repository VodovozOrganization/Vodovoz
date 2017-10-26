using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Employees;
using Vodovoz.Repository;
using Vodovoz.Tools.Logistic;
using System.Text.RegularExpressions;

namespace Vodovoz.Domain.Logistic
{

	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "адреса маршрутного листа",
		Nominative = "адрес маршрутного листа")]
	public class RouteListItem : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Свойства

		public virtual int Id { get; set; }

		Orders.Order order;

		[Display(Name = "Заказ")]
		public virtual Orders.Order Order
		{
			get { return order; }
			set { SetField(ref order, value, () => Order); }
		}

		RouteList routeList;

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList
		{
			get { return routeList; }
			set
			{
				SetField(ref routeList, value, () => RouteList);
			}
		}

		RouteListItemStatus status;

		public virtual RouteListItemStatus Status
		{
			get { return status; }
			protected set
			{
				SetField(ref status, value, () => Status);
			}
		}

		DateTime? statusLastUpdate;
		public virtual DateTime? StatusLastUpdate
		{
			get { return statusLastUpdate; }
			set
			{
				SetField(ref statusLastUpdate, value, () => StatusLastUpdate);
			}
		}

		private RouteListItem transferedTo;

		[Display(Name = "Перенесен в другой маршрутный лист")]
		public virtual RouteListItem TransferedTo
		{
			get { return transferedTo; }
			set
			{
				SetField(ref transferedTo, value, () => TransferedTo);
				if (value != null)
					this.Status = RouteListItemStatus.Transfered;
			}
		}

		private bool needToReload;

		[Display(Name = "Необходима повторная загрузка")]
		public virtual bool NeedToReload
		{
			get { return needToReload; }
			set { SetField(ref needToReload, value, () => NeedToReload); }
		}

		private bool wasTransfered;

		[Display(Name = "Был перенесен")]
		public virtual bool WasTransfered
		{
			get { return wasTransfered; }
			set { SetField(ref wasTransfered, value, () => WasTransfered); }
		}

		private string cashierComment;

		[Display(Name = "Комментарий кассира")]
		public virtual string CashierComment
		{
			get { return cashierComment; }
			set
			{
				SetField(ref cashierComment, value, () => CashierComment);
			}
		}

		private DateTime? cashierCommentCreateDate;

		[Display(Name = "Дата создания комментария кассира")]
		public virtual DateTime? CashierCommentCreateDate
		{
			get { return cashierCommentCreateDate; }
			set { SetField(ref cashierCommentCreateDate, value, () => CashierCommentCreateDate); }
		}

		private DateTime? cashierCommentLastUpdate;

		[Display(Name = "Дата обновления комментария кассира")]
		public virtual DateTime? CashierCommentLastUpdate
		{
			get { return cashierCommentLastUpdate; }
			set { SetField(ref cashierCommentLastUpdate, value, () => CashierCommentLastUpdate); }
		}

		private Employee cashierCommentAuthor;

		[Display(Name = "Автор комментария")]
		public virtual Employee CashierCommentAuthor
		{
			get { return cashierCommentAuthor; }
			set { SetField(ref cashierCommentAuthor, value, () => CashierCommentAuthor); }
		}

		string comment;
		public virtual string Comment
		{
			get { return comment; }
			set
			{
				SetField(ref comment, value, () => Comment);
			}
		}

		bool withForwarder;

		public virtual bool WithForwarder
		{
			get
			{
				return withForwarder;
			}
			set
			{
				SetField(ref withForwarder, value, () => WithForwarder);
			}
		}

		int indexInRoute;

		public virtual int IndexInRoute
		{
			get
			{
				return indexInRoute;
			}
			set
			{
				SetField(ref indexInRoute, value, () => IndexInRoute);
			}
		}

		int bottlesReturned;

		public virtual int BottlesReturned
		{
			get
			{
				return bottlesReturned;
			}
			set
			{
				SetField(ref bottlesReturned, value, () => BottlesReturned);
			}
		}

		int? driverBottlesReturned;

		public virtual int? DriverBottlesReturned
		{
			get
			{
				return driverBottlesReturned;
			}
			set
			{
				SetField(ref driverBottlesReturned, value, () => DriverBottlesReturned);
			}
		}

		decimal depositsCollected;

		public virtual decimal DepositsCollected
		{
			get
			{
				return depositsCollected;
			}
			set
			{
				SetField(ref depositsCollected, value, () => DepositsCollected);
			}
		}

		decimal equipmentDepositsCollected;

		public virtual decimal EquipmentDepositsCollected
		{
			get
			{
				return equipmentDepositsCollected;
			}
			set
			{
				SetField(ref equipmentDepositsCollected, value, () => EquipmentDepositsCollected);
			}
		}

		decimal totalCash;

		public virtual decimal TotalCash
		{
			get
			{
				return totalCash;
			}
			set
			{
				if(defaultTotalCash == -1)
					defaultTotalCash = value;
				
				SetField(ref totalCash, value, () => TotalCash);
			}
		}

		decimal driverWage;
		public virtual decimal DriverWage
		{
			get
			{
				return driverWage;
			}
			set
			{
				SetField(ref driverWage, value, () => DriverWage);
			}
		}

		decimal driverWageSurcharge;
		public virtual decimal DriverWageSurcharge
		{
			get
			{
				return driverWageSurcharge;
			}
			set
			{
				SetField(ref driverWageSurcharge, value, () => DriverWageSurcharge);
			}
		}

		public virtual decimal DriverWageTotal { get { return DriverWage + DriverWageSurcharge; } }

		decimal defaultTotalCash = -1;

		decimal forwarderWage;
		public virtual decimal ForwarderWage
		{
			get
			{
				return forwarderWage;
			}
			set
			{
				SetField(ref forwarderWage, value, () => ForwarderWage);
			}
		}

		decimal forwarderWageSurcharge;
		public virtual decimal ForwarderWageSurcharge
		{
			get
			{
				return forwarderWageSurcharge;
			}
			set
			{
				SetField(ref forwarderWageSurcharge, value, () => ForwarderWageSurcharge);
			}
		}

		public virtual decimal ForwarderWageTotal { get { return ForwarderWage + ForwarderWageSurcharge; } }

		public virtual bool Notified30Minutes { get; set; }
		public virtual bool NotifiedTimeout { get; set; }

		private TimeSpan? planTimeStart;

		[Display(Name = "Запланированное время приезда min")]
		public virtual TimeSpan? PlanTimeStart {
			get { return planTimeStart; }
			set { SetField(ref planTimeStart, value, () => PlanTimeStart); }
		}

		private TimeSpan? planTimeEnd;

		[Display(Name = "Запланированное время приезда max")]
		public virtual TimeSpan? PlanTimeEnd {
			get { return planTimeEnd; }
			set { SetField(ref planTimeEnd, value, () => PlanTimeEnd); }
		}

		#endregion

		#region Расчетные

		public virtual bool HasUserSpecifiedTotalCash()
		{
			return TotalCash != defaultTotalCash;
		}

		public virtual string Title
		{
			get
			{
				return String.Format("Адрес в МЛ {0}", Order.DeliveryPoint.CompiledAddress);
			}
		}
		//FIXME запуск оборудования - временный фикс
		public virtual int CoolersToClient
		{
			get
			{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Confirmed)
					        .Count(item =>item.Equipment != null ? item.Equipment.Nomenclature.Category == NomenclatureCategory.equipment 
					               : (item.Nomenclature.Category == NomenclatureCategory.equipment));
					//.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PlannedCoolersToClient
		{
			get
			{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PumpsToClient
		{
			get
			{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int PlannedPumpsToClient
		{
			get
			{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int UncategorisedEquipmentToClient
		{
			get
			{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.WithoutCard);
			}
		}

		public virtual int PlannedUncategorisedEquipmentToClient
		{
			get
			{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.WithoutCard);
			}
		}
		//FIXME запуск оборудования - временный фикс
		public virtual int CoolersFromClient
		{
			get
			{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Confirmed).Where(item => item.Equipment != null)
					        .Count(item => item.Equipment.Nomenclature.Category == NomenclatureCategory.equipment);
					//.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PumpsFromClient
		{
			get
			{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Confirmed).Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int PlannedCoolersFromClient
		{
			get
			{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PlannedPumpsFromClient
		{
			get
			{
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		#endregion

		#region Проброс свойств для редактирования в заказе.

		public virtual string FromClientText
		{
			get
			{
				return Order.FromClientText;
			}
			set
			{
				Order.FromClientText = value;
			}
		}

		public virtual string ToClientText
		{
			get
			{
				return Order.ToClientText;
			}
			set
			{
				Order.ToClientText = value;
				OnPropertyChanged(() => ToClientText);
			}
		}

		#endregion

		public RouteListItem()
		{
		}

		//Конструктор создания новой строки
		public RouteListItem(RouteList routeList, Order order, RouteListItemStatus status)
		{
			this.routeList = routeList;
			if (order.OrderStatus == OrderStatus.Accepted)
			{
				order.OrderStatus = OrderStatus.InTravelList;
			}
			this.Order = order;
			this.status = status;
		}

		#region Функции

		public virtual void UpdateStatus(IUnitOfWork uow, RouteListItemStatus status)
		{
			if (Status == status)
				return;

			Status = status;
			StatusLastUpdate = DateTime.Now;

			switch (Status)
			{
				case RouteListItemStatus.Canceled:
					Order.ChangeStatus(OrderStatus.DeliveryCanceled);
					break;
				case RouteListItemStatus.Completed:
					Order.ChangeStatus(OrderStatus.Shipped);
					break;
				case RouteListItemStatus.EnRoute:
					Order.ChangeStatus(OrderStatus.OnTheWay);
					break;
				case RouteListItemStatus.Overdue:
					Order.ChangeStatus(OrderStatus.NotDelivered);
					break;
			}
			uow.Save(Order);
		}

		public virtual void RecalculateWages()
		{
			DriverWage = CalculateDriverWage();
			ForwarderWage = CalculateForwarderWage();
		}

		public virtual void RecalculateTotalCash()
		{
			if (!HasUserSpecifiedTotalCash())
			{
				TotalCash = CalculateTotalCash();
				defaultTotalCash = TotalCash;
			}
		}

		public virtual decimal CalculateDriverWage()
		{
			if(!IsDelivered())
				return 0;

			if(RouteList.Driver.WageCalcType == WageCalculationType.fixedDay || RouteList.Driver.WageCalcType == WageCalculationType.fixedRoute)
			{
				return 0;
			}

			if(RouteList.Driver.WageCalcType == WageCalculationType.percentage)
				return this.Order.TotalSum * RouteList.Driver.WageCalcRate / 100;

			bool withForwarder = RouteList.Forwarder != null;
			bool ich = RouteList.Car.IsCompanyHavings;
			var rates = ich ? Wages.GetDriverRatesWithOurCar(RouteList.Date) : Wages.GetDriverRates(RouteList.Date, withForwarder);

			return CalculateWage(rates);
		}

		public virtual decimal CalculateForwarderWage()
		{
			if(!WithForwarder || RouteList.Forwarder == null)
				return 0;

			if(!IsDelivered())
				return 0;

			if(RouteList.Forwarder.WageCalcType == WageCalculationType.fixedDay || RouteList.Forwarder.WageCalcType == WageCalculationType.fixedRoute)
			{
				return 0;
			}

			if(RouteList.Forwarder.WageCalcType == WageCalculationType.percentage)
				return this.Order.TotalSum * RouteList.Forwarder.WageCalcRate /100;

			var rates = Wages.GetForwarderRates();

			return CalculateWage(rates);
		}

		public virtual decimal CalculateWage(Wages.Rates rates)
		{
			var firstOrderForAddress = RouteList.Addresses
			.Where(address => address.IsDelivered())
			.Select(item => item.Order)
			.First(ord => ord.DeliveryPoint?.Id == Order.DeliveryPoint?.Id).Id == Order.Id;

			var paymentForAddress = firstOrderForAddress ? rates.PaymentPerAddress : 0;

			var fullBottleCount = Order.OrderItems
							.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
							.Sum(item => item.ActualCount);

			var smallBottleCount = Order.OrderItems
			                      //    .Where(item => item.Nomenclature.Category == NomenclatureCategory.disposableBottleWater)
			                            .Where(item => Regex.Match(item.Nomenclature.Name, @".*(0[\.,]6).*").Length > 0)
										.Sum(item => item.ActualCount);

			bool largeOrder = fullBottleCount >= rates.LargeOrderMinimumBottles;

			var bottleCollectionOrder = Order.CollectBottles;

			decimal paymentPerEmptyBottle = largeOrder
				? rates.LargeOrderEmptyBottleRate
				: rates.EmptyBottleRate;
			var largeFullBottlesPayment = largeOrder
				? fullBottleCount * rates.LargeOrderFullBottleRate
				: fullBottleCount * rates.FullBottleRate;

			var smallBottlePayment = Math.Truncate((smallBottleCount * rates.SmallBottleRate)/36);

			var payForEquipment = fullBottleCount == 0
				&& (Order.OrderEquipments.Count(item => item.Direction == Direction.Deliver && item.Confirmed) > 0
							   || bottleCollectionOrder);
			var equpmentPayment = payForEquipment ? rates.CoolerRate : 0;

			var contractCancelationPayment = bottleCollectionOrder ? rates.ContractCancelationRate : 0;
			var emptyBottlesPayment = bottleCollectionOrder ? 0 : paymentPerEmptyBottle * bottlesReturned;
			var smallFullBottlesPayment =
				rates.SmallFullBottleRate * Order.OrderItems
					.Where(item => item.Nomenclature.Category == NomenclatureCategory.disposableBottleWater
						&& item.Nomenclature.Weight == 6.0)
					.Sum(item => item.ActualCount);

			var wage = equpmentPayment + largeFullBottlesPayment + smallBottlePayment
				+ contractCancelationPayment + emptyBottlesPayment
				+ smallFullBottlesPayment + paymentForAddress;

#if SHORT

			var payForEquipmentShort = fullBottleCount == 0
				&& (!string.IsNullOrWhiteSpace(Order.ToClientText)
					|| bottleCollectionOrder
					|| Order.OrderItems.Where(i => i.Nomenclature.Category == NomenclatureCategory.additional)
					.FirstOrDefault(i => i.ActualCount > 0) != null);
			var equpmentPaymentShort = payForEquipmentShort ? rates.CoolerRate : 0;

			wage += equpmentPaymentShort;

			if (Order.ToClientText?.ToLower().Contains("раст") == true)
				wage = 10;
#endif

			return wage;
		}

		public virtual decimal CalculateTotalCash()
		{
			if (!IsDelivered())
				return 0;

			return Order.OrderItems
				.Where(item => item.Order.PaymentType == Client.PaymentType.cash)
				.Sum(item => item.ActualCount * item.Price * (1 - (decimal)item.Discount / 100));
		}

		/// <summary>
		/// Ises the delivered.
		/// </summary>
		/// <returns><c>true</c>, if delivered was ised, <c>false</c> otherwise.</returns>
		public virtual bool IsDelivered()
		{
			var routeListUnloaded = (RouteList.Status >= RouteListStatus.OnClosing);
			return Status == RouteListItemStatus.Completed || Status == RouteListItemStatus.EnRoute && routeListUnloaded;
		}

		public virtual int GetFullBottlesDeliveredCount()
		{
			return Order.OrderItems.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
				.Sum(item => item.ActualCount);
		}

		public virtual int GetFullBottlesToDeliverCount()
		{
			return Order.OrderItems.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
				.Sum(item => item.Count);
		}

		/// <summary>
		/// Функция вызывается при переходе адреса в закрытие.
		/// Если адрес в пути, при закрытии МЛ он считается автоматически доставленным.
		/// </summary>
		/// <param name="uow">Uow.</param>
		public virtual void FirstFillClosing(IUnitOfWork uow)
		{
			//В этом месте изменяем статус для подстраховки.
			if (Status == RouteListItemStatus.EnRoute)
				Status = RouteListItemStatus.Completed;

			foreach (var item in Order.OrderItems)
			{
				item.ActualCount = IsDelivered() ? item.Count : 0;
			}
			PerformanceHelper.AddTimePoint(logger, "Обработали номенклатуры");
			BottlesReturned = IsDelivered()
				? (DriverBottlesReturned ?? Order.BottlesReturn) : 0;
			TotalCash = IsDelivered() &&
				Order.PaymentType == Client.PaymentType.cash
				? Order.SumToReceive : 0;
			var bottleDepositPrice = NomenclatureRepository.GetBottleDeposit(uow).GetPrice(Order.BottlesReturn);
			DepositsCollected = IsDelivered()
								? Order.GetExpectedBottlesDepositsCount() * bottleDepositPrice : 0;
			PerformanceHelper.AddTimePoint("Получили прайс");
			RecalculateWages();
			PerformanceHelper.AddTimePoint("Пересчет");
		}

		private Dictionary<int, int> goodsByRouteColumns;

		public virtual Dictionary<int, int> GoodsByRouteColumns
		{
			get
			{
				if (goodsByRouteColumns == null)
				{
					goodsByRouteColumns = Order.OrderItems.Where(i => i.Nomenclature.RouteListColumn != null)
						.GroupBy(i => i.Nomenclature.RouteListColumn.Id, i => i.Count)
						.ToDictionary(g => g.Key, g => g.Sum());
				}
				return goodsByRouteColumns;
			}
		}

		public virtual int GetGoodsAmountForColumn(int columnId)
		{
			if (GoodsByRouteColumns.ContainsKey(columnId))
				return GoodsByRouteColumns[columnId];
			else
				return 0;
		}

		public virtual int GetGoodsActualAmountForColumn(int columnId)
		{
			return Order.OrderItems.Where(i => i.Nomenclature.RouteListColumn != null)
				.Where(i => i.Nomenclature.RouteListColumn.Id == columnId)
				.Sum(i => i.ActualCount);
		}


		public virtual void RemovedFromRoute()
		{
			Order.OrderStatus = OrderStatus.Accepted;
		}

		public virtual void SetStatusWithoutOrderChange(RouteListItemStatus status)
		{
			Status = status;
		}

		public virtual string GetTransferText(RouteListItem item) // Скопировано из RouteListClosingItemsView, отображает передавшего и принявшего адрес.
		{
			if (item.Status == RouteListItemStatus.Transfered)
			{
				if (item.TransferedTo != null)
					return String.Format("Заказ был перенесен в МЛ №{0} водителя {1}.",
									 item.TransferedTo.RouteList.Id,
									 item.TransferedTo.RouteList.Driver.ShortName
									);
				else
					return "ОШИБКА! Адрес имеет статус перенесенного в другой МЛ, но куда он перенесен не указано.";
			}
			if (item.WasTransfered)
			{
				var transferedFrom = Repository.Logistics.RouteListItemRepository.GetTransferedFrom(RouteList.UoW, item);
				if (transferedFrom != null)
					return String.Format("Заказ из МЛ №{0} водителя {1}.",
										 transferedFrom.RouteList.Id,
										 transferedFrom.RouteList.Driver.ShortName
										);
				else
					return "ОШИБКА! Адрес помечен как перенесенный из другого МЛ, но строка откуда он был перенесен не найдена.";
			}
			return null;
		}

		public virtual void SetDriversWage(decimal wage)
		{
			this.DriverWage = wage;
		}

		public virtual void SetForwardersWage(decimal wage)
		{
			this.ForwarderWage = wage;
		}

		#endregion

		#region Для расчетов в логистике

		/// <summary>
		/// Время в минутах.
		/// </summary>
		public virtual int TimeOnPoint{
			get{
				return Order.CalculateTimeOnPoint(RouteList.Forwarder != null);
			}
		}

		public virtual TimeSpan CalculatePlanedTime(RouteGeometryCalculator sputnikCache){
			DateTime time = default(DateTime);

			for (int ix = 0; ix < RouteList.Addresses.Count; ix++)
			{
				var address = RouteList.Addresses[ix];
				if (ix == 0)
					time = time.Add(RouteList.Addresses[ix].Order.DeliverySchedule.From);
				else
					time = time.AddSeconds(sputnikCache.TimeSec(RouteList.Addresses[ix-1].Order.DeliveryPoint, RouteList.Addresses[ix].Order.DeliveryPoint));

				if (address == this)
					break;

				time = time.AddMinutes(RouteList.Addresses[ix].TimeOnPoint);
			}
			return time.TimeOfDay;
		}

		#endregion
	}

	public enum RouteListItemStatus
	{
		[Display(Name = "В пути")]
		EnRoute,
		[Display(Name = "Выполнен")]
		Completed,
		[Display(Name = "Отмена клиентом")]
		Canceled,
		[Display(Name = "Опоздали")]
		Overdue,
		[Display(Name = "Передан")]
		Transfered
	}

	public class RouteListItemStatusStringType : NHibernate.Type.EnumStringType
	{
		public RouteListItemStatusStringType() : base(typeof(RouteListItemStatus))
		{
		}
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Tools;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Domain.Logistic
{

	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "адреса маршрутного листа",
		Nominative = "адрес маршрутного листа")]
	[HistoryTrace]
	public class RouteListItem : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Свойства

		public virtual int Id { get; set; }

		Orders.Order order;

		[Display(Name = "Заказ")]
		public virtual Orders.Order Order {
			get => order;
			set => SetField(ref order, value, () => Order);
		}

		RouteList routeList;

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList {
			get => routeList;
			set => SetField(ref routeList, value, () => RouteList);
		}

		RouteListItemStatus status;
		[Display(Name = "Статус адреса")]
		public virtual RouteListItemStatus Status {
			get => status;
			protected set => SetField(ref status, value, () => Status);
		}

		DateTime? statusLastUpdate;
		[Display(Name = "Время изменения статуса")]
		public virtual DateTime? StatusLastUpdate {
			get => statusLastUpdate;
			set => SetField(ref statusLastUpdate, value, () => StatusLastUpdate);
		}

		private RouteListItem transferedTo;

		[Display(Name = "Перенесен в другой маршрутный лист")]
		public virtual RouteListItem TransferedTo {
			get => transferedTo;
			set {
				if(SetField(ref transferedTo, value, () => TransferedTo) && value != null)
					Status = RouteListItemStatus.Transfered;
			}
		}

		private bool needToReload;

		[Display(Name = "Необходима повторная загрузка")]
		public virtual bool NeedToReload {
			get => needToReload;
			set => SetField(ref needToReload, value, () => NeedToReload);
		}

		private bool wasTransfered;

		[Display(Name = "Был перенесен")]
		public virtual bool WasTransfered {
			get => wasTransfered;
			set => SetField(ref wasTransfered, value, () => WasTransfered);
		}

		private string cashierComment;

		[Display(Name = "Комментарий кассира")]
		public virtual string CashierComment {
			get => cashierComment;
			set => SetField(ref cashierComment, value, () => CashierComment);
		}

		private DateTime? cashierCommentCreateDate;

		[Display(Name = "Дата создания комментария кассира")]
		public virtual DateTime? CashierCommentCreateDate {
			get => cashierCommentCreateDate;
			set => SetField(ref cashierCommentCreateDate, value, () => CashierCommentCreateDate);
		}

		private DateTime? cashierCommentLastUpdate;

		[Display(Name = "Дата обновления комментария кассира")]
		public virtual DateTime? CashierCommentLastUpdate {
			get => cashierCommentLastUpdate;
			set => SetField(ref cashierCommentLastUpdate, value, () => CashierCommentLastUpdate);
		}

		private Employee cashierCommentAuthor;

		[Display(Name = "Автор комментария")]
		public virtual Employee CashierCommentAuthor {
			get => cashierCommentAuthor;
			set => SetField(ref cashierCommentAuthor, value, () => CashierCommentAuthor);
		}

		string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		bool withForwarder;
		[Display(Name = "С экспедитором")]
		public virtual bool WithForwarder {
			get => withForwarder;
			set => SetField(ref withForwarder, value, () => WithForwarder);
		}

		int indexInRoute;
		[Display(Name = "Порядковый номер в МЛ")]
		public virtual int IndexInRoute {
			get => indexInRoute;
			set => SetField(ref indexInRoute, value, () => IndexInRoute);
		}

		int bottlesReturned;
		[Display(Name = "Возвращено бутылей")]
		public virtual int BottlesReturned {
			get => bottlesReturned;
			set => SetField(ref bottlesReturned, value, () => BottlesReturned);
		}

		int? driverBottlesReturned;
		[Display(Name = "Возвращено бутылей - водитель")]
		public virtual int? DriverBottlesReturned {
			get => driverBottlesReturned;
			set => SetField(ref driverBottlesReturned, value, () => DriverBottlesReturned);
		}

		decimal oldBottleDepositsCollected;
		/// <summary>
		/// Устаревший залог за бутыли. Который раньше вводился пользователем вручную при закрытии МЛ
		/// </summary>
		[Display(Name = "Старый залог за бутыли")]
		public virtual decimal OldBottleDepositsCollected {
			get => oldBottleDepositsCollected;
			set => SetField(ref oldBottleDepositsCollected, value, () => OldBottleDepositsCollected);
		}

		public virtual decimal BottleDepositsCollected {
			get {
				if(Order.PaymentType != Client.PaymentType.cash && Order.PaymentType != Client.PaymentType.BeveragesWorld) {
					return 0;
				}

				if(oldBottleDepositsCollected != 0m) {
					return oldBottleDepositsCollected;
				}

				return 0 - Order.BottleDepositSum;
			}
		}

		decimal oldEquipmentDepositsCollected;

		/// <summary>
		/// Устаревший залог за оборудование. Который раньше вводился пользователем вручную при закрытии МЛ
		/// </summary>
		[Display(Name = "Старый залог за оборудование")]
		public virtual decimal OldEquipmentDepositsCollected {
			get => oldEquipmentDepositsCollected;
			set => SetField(ref oldEquipmentDepositsCollected, value, () => OldEquipmentDepositsCollected);
		}

		public virtual decimal EquipmentDepositsCollected {
			get {
				if(Order.PaymentType != Client.PaymentType.cash && Order.PaymentType != Client.PaymentType.BeveragesWorld) {
					return 0;
				}

				if(oldEquipmentDepositsCollected != 0m) {
					return oldEquipmentDepositsCollected;
				}

				return 0 - Order.EquipmentDepositSum;
			}
		}

		public virtual decimal AddressCashSum {
			get {
				if(!IsDelivered()) {
					return 0;
				}
				if(Order.PaymentType != Client.PaymentType.cash && Order.PaymentType != Client.PaymentType.BeveragesWorld) {
					return 0;
				}
				return Order.OrderCashSum + OldBottleDepositsCollected + OldEquipmentDepositsCollected + ExtraCash;
			}
		}

		decimal totalCash;
		[Display(Name = "Всего наличных")]
		public virtual decimal TotalCash {
			get => totalCash;
			set => SetField(ref totalCash, value, () => TotalCash);
		}

		decimal extraCash;
		[Display(Name = "Дополнительно наличных")]
		public virtual decimal ExtraCash {
			get => extraCash;
			set => SetField(ref extraCash, value, () => ExtraCash);
		}

		private string terminalPaymentNumber;

		[Display(Name = "№ оплаты(по терминалу)")]
		public virtual string TerminalPaymentNumber {
			get { return terminalPaymentNumber; }
			set { SetField(ref terminalPaymentNumber, value, () => TerminalPaymentNumber); }
		}

		decimal driverWage;
		[Display(Name = "ЗП водителя")]
		public virtual decimal DriverWage {
			get => driverWage;
			set => SetField(ref driverWage, value, () => DriverWage);
		}

		decimal driverWageSurcharge;
		[Display(Name = "Надбавка к ЗП водителя")]
		public virtual decimal DriverWageSurcharge {
			get => driverWageSurcharge;
			set => SetField(ref driverWageSurcharge, value, () => DriverWageSurcharge);
		}

		decimal forwarderWage;
		[Display(Name = "ЗП экспедитора")]
		//Зарплана с уже включенной надбавкой ForwarderWageSurcharge
		public virtual decimal ForwarderWage {
			get => forwarderWage;
			set => SetField(ref forwarderWage, value, () => ForwarderWage);
		}

		[Display(Name = "Оповещение за 30 минут")]
		[IgnoreHistoryTrace]
		public virtual bool Notified30Minutes { get; set; }

		[Display(Name = "Время оповещения прошло")]
		[IgnoreHistoryTrace]
		public virtual bool NotifiedTimeout { get; set; }

		private TimeSpan? planTimeStart;

		[Display(Name = "Запланированное время приезда min")]
		public virtual TimeSpan? PlanTimeStart {
			get => planTimeStart;
			set => SetField(ref planTimeStart, value, () => PlanTimeStart);
		}

		private TimeSpan? planTimeEnd;

		[Display(Name = "Запланированное время приезда max")]
		public virtual TimeSpan? PlanTimeEnd {
			get => planTimeEnd;
			set => SetField(ref planTimeEnd, value, () => PlanTimeEnd);
		}

		WageDistrictLevelRate forwarderWageCalulationMethodic;
		[Display(Name = "Методика расчёта ЗП экспедитора")]
		public virtual WageDistrictLevelRate ForwarderWageCalculationMethodic {
			get => forwarderWageCalulationMethodic;
			set => SetField(ref forwarderWageCalulationMethodic, value);
		}

		WageDistrictLevelRate driverWageCalulationMethodic;
		[Display(Name = "Методика расчёта ЗП водителя")]
		public virtual WageDistrictLevelRate DriverWageCalculationMethodic {
			get => driverWageCalulationMethodic;
			set => SetField(ref driverWageCalulationMethodic, value);
		}

		#endregion

		#region Runtime свойства (не мапятся)

		public virtual bool AddressIsValid { get; set; } = true;

		#endregion

		#region Расчетные

		public virtual string Title => string.Format("Адрес в МЛ №{0} - {1}", RouteList.Id, Order.DeliveryPoint.CompiledAddress);

		//FIXME запуск оборудования - временный фикс
		public virtual int CoolersToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver)
					.Where(item => item.Confirmed)
							.Count(item => item.Equipment != null ? item.Equipment.Nomenclature.Category == NomenclatureCategory.equipment
								   : (item.Nomenclature.Category == NomenclatureCategory.equipment));
			}
		}

		public virtual int PlannedCoolersToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver).Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PumpsToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver).Where(item => item.Equipment != null)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int PlannedPumpsToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver).Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int UncategorisedEquipmentToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver).Where(item => item.Equipment != null)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.WithoutCard);
			}
		}

		public virtual int PlannedUncategorisedEquipmentToClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.Deliver).Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.WithoutCard);
			}
		}

		//FIXME запуск оборудования - временный фикс
		public virtual int CoolersFromClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Confirmed).Where(item => item.Equipment != null)
							.Count(item => item.Equipment.Nomenclature.Category == NomenclatureCategory.equipment);
			}
		}

		public virtual int PumpsFromClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp).Where(item => item.Equipment != null)
					.Where(item => item.Confirmed)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual int PlannedCoolersFromClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			}
		}

		public virtual int PlannedPumpsFromClient {
			get {
				return Order.OrderEquipments.Where(item => item.Direction == Direction.PickUp)
					.Where(item => item.Equipment != null)
					.Count(item => item.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			}
		}

		public virtual string EquipmentsToClientText {
			get {
				//Если это старый заказ со старой записью оборудования в виде строки, то выводим только его
				if(!string.IsNullOrWhiteSpace(Order.ToClientText))
					return Order.ToClientText;

				var orderEquipment = string.Join(
								Environment.NewLine,
								Order.OrderEquipments
									.Where(x => x.Direction == Direction.Deliver)
								    .Select(x => $"{x.NameString}: {x.Count}")
				);

				var orderItemEquipment = string.Join(
								Environment.NewLine,
								Order.OrderItems
									.Where(x => x.Nomenclature.Category == NomenclatureCategory.equipment)
							  		.Select(x => $"{x.Nomenclature.Name}: {x.Count}")
				);

				if(String.IsNullOrWhiteSpace(orderItemEquipment))
					return orderEquipment;
				return $"{orderEquipment}{Environment.NewLine}{orderItemEquipment}";
			}
		}

		public virtual string EquipmentsFromClientText {
			get {
				//Если это старый заказ со старой записью оборудования в виде строки, то выводим только его
				if(!string.IsNullOrWhiteSpace(Order.FromClientText))
					return Order.FromClientText;

				return string.Join("\n",
								   Order.OrderEquipments
										   .Where(x => x.Direction == Direction.PickUp)
								   .Select(x => $"{x.NameString}: {x.Count}")
								  );
			}
		}

		public virtual WageDistrictLevelRate DriverWageCalcMethodicTemporaryStore { get; set; }

		public virtual WageDistrictLevelRate ForwarderWageCalcMethodicTemporaryStore { get; set; }

		#endregion

		public RouteListItem() { }

		//Конструктор создания новой строки
		public RouteListItem(RouteList routeList, Order order, RouteListItemStatus status)
		{
			this.routeList = routeList;
			if(order.OrderStatus == OrderStatus.Accepted) {
				order.OrderStatus = OrderStatus.InTravelList;
			}
			this.Order = order;
			this.status = status;
		}

		#region Функции

		public virtual void UpdateStatus(IUnitOfWork uow, RouteListItemStatus status, CallTaskWorker callTaskWorker)
		{
			if(Status == status)
				return;

			Status = status;
			StatusLastUpdate = DateTime.Now;

			switch(Status) {
				case RouteListItemStatus.Canceled:
					Order.ChangeStatus(OrderStatus.DeliveryCanceled, callTaskWorker);
					Order.TimeDelivered = null;
					FillCountsOnCanceled();
					break;
				case RouteListItemStatus.Completed:
					Order.ChangeStatus(OrderStatus.Shipped, callTaskWorker);
					Order.TimeDelivered = DateTime.Now;
					RestoreOrder();
					break;
				case RouteListItemStatus.EnRoute:
					Order.ChangeStatus(OrderStatus.OnTheWay, callTaskWorker);
					Order.TimeDelivered = null;
					RestoreOrder();
					break;
				case RouteListItemStatus.Overdue:
					Order.ChangeStatus(OrderStatus.NotDelivered, callTaskWorker);
					Order.TimeDelivered = null;
					FillCountsOnCanceled();
					break;
			}
			uow.Save(Order);
		}

		public virtual void RecalculateTotalCash() => TotalCash = CalculateTotalCash();

		public virtual decimal CalculateTotalCash() => IsDelivered() ? AddressCashSum : 0;

		public virtual bool IsDelivered()
		{
			var routeListUnloaded = new[] { RouteListStatus.OnClosing, RouteListStatus.Closed }.Contains(RouteList.Status);
			return Status == RouteListItemStatus.Completed || Status == RouteListItemStatus.EnRoute && routeListUnloaded;
		}

		public virtual int GetFullBottlesDeliveredCount()
		{
			return Order.OrderItems.Where(item => item.Nomenclature.Category == NomenclatureCategory.water && item.Nomenclature.TareVolume == TareVolume.Vol19L)
								   .Sum(item => item.ActualCount ?? 0);
		}

		public virtual int GetFullBottlesToDeliverCount()
		{
			return Order.OrderItems.Where(item => item.Nomenclature.Category == NomenclatureCategory.water && item.Nomenclature.TareVolume == TareVolume.Vol19L)
								   .Sum(item => item.Count);
		}

		/// <summary>
		/// Функция вызывается при переходе адреса в закрытие.
		/// Если адрес в пути, при закрытии МЛ он считается автоматически доставленным.
		/// </summary>
		/// <param name="uow">Uow.</param>
		public virtual void FirstFillClosing(IUnitOfWork uow, WageParameterService wageParameterService)
		{
			//В этом месте изменяем статус для подстраховки.
			if(Status == RouteListItemStatus.EnRoute)
				Status = RouteListItemStatus.Completed;

			foreach(var item in Order.OrderItems)
				item.ActualCount = IsDelivered() ? item.Count : 0;

			foreach(var equip in Order.OrderEquipments)
				equip.ActualCount = IsDelivered() ? equip.Count : 0;

			foreach(var deposit in Order.OrderDepositItems)
				deposit.ActualCount = IsDelivered() ? deposit.Count : 0;

			Order.BottlesByStockActualCount = Order.BottlesByStockCount;

			PerformanceHelper.AddTimePoint(logger, "Обработали номенклатуры");
			BottlesReturned = IsDelivered() ? (DriverBottlesReturned ?? Order.BottlesReturn ?? 0) : 0;
			RecalculateTotalCash();
			RouteList.RecalculateWagesForRouteListItem(this, wageParameterService);
		}

		/// <summary>
		/// Обнуляет фактическое количетво
		/// Использовать если заказ отменен или полностью не доставлен
		/// </summary>
		public virtual void FillCountsOnCanceled()
		{
			foreach(var item in Order.OrderItems) {
				if(!item.OriginalDiscountMoney.HasValue || !item.OriginalDiscount.HasValue) {
					item.OriginalDiscountMoney = item.DiscountMoney > 0 ? (decimal?)item.DiscountMoney : null;
					item.OriginalDiscount = item.Discount > 0 ? (decimal?)item.Discount : null;
					item.OriginalDiscountReason = (item.DiscountMoney > 0 || item.Discount > 0) ? item.DiscountReason : null;
				}
				item.ActualCount = 0;
			}
			foreach(var equip in Order.OrderEquipments)
				equip.ActualCount = 0;

			foreach(var deposit in Order.OrderDepositItems)
				deposit.ActualCount = 0;
		}

		public virtual void RestoreOrder()
		{
			foreach(var item in Order.OrderItems) {
				if(item.OriginalDiscountMoney.HasValue || item.OriginalDiscount.HasValue) {
					item.DiscountMoney = item.OriginalDiscountMoney ?? 0;
					item.DiscountReason = item.OriginalDiscountReason;
					item.Discount = item.OriginalDiscount ?? 0;
					item.OriginalDiscountMoney = null;
					item.OriginalDiscountReason = null;
					item.OriginalDiscount = null;
				}
				item.ActualCount = item.Count;
			}
			foreach(var equip in Order.OrderEquipments)
				equip.ActualCount = equip.Count;
			foreach(var deposit in Order.OrderDepositItems)
				deposit.ActualCount = deposit.Count;
		}

		private Dictionary<int, int> goodsByRouteColumns;

		public virtual Dictionary<int, int> GoodsByRouteColumns {
			get {
				if(goodsByRouteColumns == null) {
					goodsByRouteColumns = Order.OrderItems.Where(i => i.Nomenclature.RouteListColumn != null)
						.GroupBy(i => i.Nomenclature.RouteListColumn.Id, i => i.Count)
						.ToDictionary(g => g.Key, g => g.Sum());
				}
				return goodsByRouteColumns;
			}
		}

		public virtual int GetGoodsAmountForColumn(int columnId) => GoodsByRouteColumns.ContainsKey(columnId) ? GoodsByRouteColumns[columnId] : 0;

		public virtual int GetGoodsActualAmountForColumn(int columnId)
		{
			if(Status == RouteListItemStatus.Transfered)
				return 0;
			return Order.OrderItems.Where(i => i.Nomenclature.RouteListColumn != null && i.Nomenclature.RouteListColumn.Id == columnId)
								   .Sum(i => i.ActualCount ?? 0);
		}

		public virtual void RemovedFromRoute() => Order.OrderStatus = OrderStatus.Accepted;

		public virtual void SetStatusWithoutOrderChange(RouteListItemStatus status) => Status = status;

		// Скопировано из RouteListClosingItemsView, отображает передавшего и принявшего адрес.
		public virtual string GetTransferText(RouteListItem item)
		{
			if(item.Status == RouteListItemStatus.Transfered) {
				if(item.TransferedTo != null)
					return string.Format("Заказ был перенесен в МЛ №{0} водителя {1}.",
									 item.TransferedTo.RouteList.Id,
									 item.TransferedTo.RouteList.Driver.ShortName
									);
				else
					return "ОШИБКА! Адрес имеет статус перенесенного в другой МЛ, но куда он перенесен не указано.";
			}
			if(item.WasTransfered) {
				var transferedFrom = new RouteListItemRepository().GetTransferedFrom(RouteList.UoW, item);
				if(transferedFrom != null)
					return string.Format("Заказ из МЛ №{0} водителя {1}.",
										 transferedFrom.RouteList.Id,
										 transferedFrom.RouteList.Driver.ShortName
										);
				else
					return "ОШИБКА! Адрес помечен как перенесенный из другого МЛ, но строка откуда он был перенесен не найдена.";
			}
			return null;
		}

		#endregion

		#region Для расчетов в логистике

		/// <summary>
		/// Время разгрузки на адресе в секундах.
		/// </summary>
		public virtual int TimeOnPoint => Order.CalculateTimeOnPoint(RouteList.Forwarder != null);

		public virtual TimeSpan CalculatePlanedTime(RouteGeometryCalculator sputnikCache)
		{
			DateTime time = default(DateTime);

			for(int ix = 0; ix < RouteList.Addresses.Count; ix++) {
				var address = RouteList.Addresses[ix];
				if(ix == 0)
					time = time.Add(RouteList.Addresses[ix].Order.DeliverySchedule.From);
				else
					time = time.AddSeconds(sputnikCache.TimeSec(RouteList.Addresses[ix - 1].Order.DeliveryPoint, RouteList.Addresses[ix].Order.DeliveryPoint));

				if(address == this)
					break;

				time = time.AddSeconds(RouteList.Addresses[ix].TimeOnPoint);
			}
			sputnikCache?.Dispose();
			return time.TimeOfDay;
		}

		#endregion

		#region Зарплата

		public virtual IRouteListItemWageCalculationSource DriverWageCalculationSrc => new RouteListItemWageCalculationSource(this, EmployeeCategory.driver);

		public virtual IRouteListItemWageCalculationSource ForwarderWageCalculationSrc => new RouteListItemWageCalculationSource(this, EmployeeCategory.forwarder);

		public virtual void SaveWageCalculationMethodics()
		{
			DriverWageCalculationMethodic = DriverWageCalcMethodicTemporaryStore;
			ForwarderWageCalculationMethodic = ForwarderWageCalcMethodicTemporaryStore;
		}

		#endregion Зарплата
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
		public RouteListItemStatusStringType() : base(typeof(RouteListItemStatus)) { }
	}
}
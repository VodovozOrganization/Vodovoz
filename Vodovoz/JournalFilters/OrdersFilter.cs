using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	public partial class OrdersFilter : RepresentationFilterBase<OrdersFilter>
	{
		protected override void ConfigureWithUow()
		{
			enumcomboStatus.ItemsEnum = typeof(OrderStatus);
			enumcomboPaymentType.ItemsEnum = typeof(PaymentType);
			entryreferenceClient.RepresentationModel = new ViewModel.CounterpartyVM(new CounterpartyFilter(UoW));
			daysToAft = -CurrentUserSettings.Settings.JournalDaysToAft;
			daysToFwd = CurrentUserSettings.Settings.JournalDaysToFwd;
			dateperiodOrders.StartDateOrNull = DateTime.Today.AddDays(daysToAft);
			dateperiodOrders.EndDateOrNull = DateTime.Today.AddDays(daysToFwd);
		}

		public OrdersFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public OrdersFilter()
		{
			this.Build();
		}

		int daysToAft = 0;
		int daysToFwd = 0;

		public OrderStatus? RestrictStatus {
			get { return enumcomboStatus.SelectedItem as OrderStatus?; }
			set {
				enumcomboStatus.SelectedItem = value;
				enumcomboStatus.Sensitive = false;
			}
		}

		Object[] hideStatuses;
		/// <summary>
		/// Скрыть заказы со статусом из массива
		/// </summary>
		/// <value>массив скрываемых статусов</value>
		public Object[] HideStatuses {
			get => hideStatuses;
			set {
				enumcomboStatus.AddEnumToHideList(value);
				hideStatuses = value;
			}
		}

		OrderStatus[] allowStatuses;
		/// <summary>
		/// Скрыть заказы со статусом из массива
		/// </summary>
		/// <value>массив скрываемых статусов</value>
		public OrderStatus[] AllowStatuses {
			get => allowStatuses;
			set {
				allowStatuses = value;
				List<Object> hides = new List<object>();
				foreach(OrderStatus item in Enum.GetValues(typeof(OrderStatus))) {
					if(!value.Contains(item)) {
						hides.Add(item);
					}
				}
				enumcomboStatus.ClearEnumHideList();
				enumcomboStatus.AddEnumToHideList(hides.ToArray());
			}
		}

		PaymentType? restrictPaymentType;
		public PaymentType? RestrictPaymentType {
			get => restrictPaymentType;
			set { 
				restrictPaymentType = value;
				enumcomboPaymentType.SelectedItem = value;
				enumcomboPaymentType.Sensitive = false;
			}
		}

		PaymentType[] allowPaymentTypes;
		/// <summary>
		/// Скрыть заказы со статусом из массива
		/// </summary>
		/// <value>массив скрываемых статусов</value>
		public PaymentType[] AllowPaymentTypes {
			get => allowPaymentTypes;
			set {
				allowPaymentTypes = value;
				List<Object> hides = new List<object>();
				foreach(PaymentType item in Enum.GetValues(typeof(PaymentType))) {
					if(!value.Contains(item)) {
						hides.Add(item);
					}
				}
				enumcomboPaymentType.ClearEnumHideList();
				enumcomboPaymentType.AddEnumToHideList(hides.ToArray());
			}
		}

		public Counterparty RestrictCounterparty {
			get { return entryreferenceClient.Subject as Counterparty; }
			set {
				entryreferenceClient.Subject = value;
				entryreferenceClient.Sensitive = false;
			}
		}

		public DeliveryPoint RestrictDeliveryPoint {
			get { return entryreferencePoint.Subject as DeliveryPoint; }
			set {
				entryreferencePoint.Subject = value;
				entryreferencePoint.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate {
			get { return dateperiodOrders.StartDateOrNull; }
			set {
				dateperiodOrders.StartDateOrNull = value;
				dateperiodOrders.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate {
			get { return dateperiodOrders.EndDateOrNull; }
			set {
				dateperiodOrders.EndDateOrNull = value;
				dateperiodOrders.Sensitive = false;
			}
		}

		public bool RestrictOnlyWithoutCoodinates {
			get { return checkWithoutCoordinates.Active; }
			set {
				checkWithoutCoordinates.Active = value;
				checkWithoutCoordinates.Sensitive = false;
			}
		}

		bool? restrictSelfDelivery;

		public bool? RestrictSelfDelivery {
			get {
				return checkOnlySelfDelivery.Active ? true : restrictSelfDelivery;
			}
			set {
				restrictSelfDelivery = value;
				checkOnlySelfDelivery.Active = value == true;
				checkOnlySelfDelivery.Sensitive = false;
			}
		}

		bool? restrictWithoutSelfDelivery;

		public bool? RestrictWithoutSelfDelivery {
			get {
				return checkWithoutSelfDelivery.Active ? true : restrictWithoutSelfDelivery;
			}
			set {
				restrictWithoutSelfDelivery = value;
				checkWithoutSelfDelivery.Active = value == true;
				checkWithoutSelfDelivery.Sensitive = false;
			}
		}

		bool? restrictLessThreeHours;

		public bool? RestrictLessThreeHours {
			get {
				return checkLessThreeHours.Active ? true : restrictLessThreeHours;
			}
			set {
				restrictLessThreeHours = value;
				checkLessThreeHours.Active = value == true;
				checkLessThreeHours.Sensitive = false;
			}
		}

		bool? restrictHideService;

		public bool? RestrictHideService {
			get {
				return checkHideService.Active ? true : restrictHideService;
			}
			set {
				restrictHideService = value;
				checkHideService.Active = value == true;
				checkHideService.Sensitive = false;
			}
		}

		bool? restrictOnlyService;

		public bool? RestrictOnlyService {
			get {
				return checkOnlyService.Active ? true : restrictOnlyService;
			}
			set {
				restrictOnlyService = value;
				checkOnlyService.Active = value == true;
				checkOnlyService.Sensitive = false;
			}
		}

		public int[] ExceptIds { get; set; }

		protected void OnEntryreferenceClientChanged(object sender, EventArgs e)
		{
			CheckLimitationForDate();

			entryreferencePoint.Sensitive = RestrictCounterparty != null;
			if(RestrictCounterparty == null)
				entryreferencePoint.Subject = null;
			else {
				entryreferencePoint.Subject = null;
				entryreferencePoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM(UoW, RestrictCounterparty);
			}
			OnRefiltered();
		}

		protected void OnEntryreferencePointChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnDateperiodOrdersPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
			if(CurrentUserSettings.Settings.JournalDaysToAft != daysToAft
			   || CurrentUserSettings.Settings.JournalDaysToFwd != daysToFwd) {
				CurrentUserSettings.Settings.JournalDaysToAft = daysToAft;
				CurrentUserSettings.Settings.JournalDaysToFwd = daysToFwd;
				CurrentUserSettings.SaveSettings();
			}
		}

		protected void OnEnumcomboStatusChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnCheckWithoutCoordinatesToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnCheckOnlySelfDeliveryToggled(object sender, EventArgs e)
		{
			checkWithoutSelfDelivery.Active = checkOnlySelfDelivery.Active ? false : checkWithoutSelfDelivery.Active;
			OnRefiltered();
		}

		protected void OnCheckWithoutSelfDeliveryToggled(object sender, EventArgs e)
		{
			checkOnlySelfDelivery.Active = checkWithoutSelfDelivery.Active ? false : checkOnlySelfDelivery.Active;
			OnRefiltered();
		}

		protected void OnCheckLessThreeHoursToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnCheckServiceToggled(object sender, EventArgs e)
		{
			checkOnlyService.Active = checkHideService.Active ? false : checkOnlyService.Active;
			OnRefiltered();
		}

		protected void OnCheckOnlyServiceToggled(object sender, EventArgs e)
		{
			checkHideService.Active = checkOnlyService.Active ? false : checkHideService.Active;
			OnRefiltered();
		}

		protected void OnDateperiodOrdersStartDateChanged(object sender, EventArgs e)
		{
			CheckLimitationForDate();
			//сохранение диапазона дат
			daysToAft = (DateTime.Today - dateperiodOrders.StartDate).Days;
			daysToAft = daysToAft > 14 ? 14 : daysToAft; //ограничение на сохранение не более 15 дней назад
			daysToAft = daysToAft < 0 ? 0 : daysToAft; //проверка на корректность выбора дат
		}

		protected void OnDateperiodOrdersEndDateChanged(object sender, EventArgs e)
		{
			CheckLimitationForDate();
			//сохранение диапазона дат
			daysToFwd = (dateperiodOrders.EndDate - DateTime.Today).Days;
			daysToFwd = daysToFwd > 14 ? 14 : daysToFwd; //ограничение на сохранение не более 15 дней вперёд
			daysToFwd = daysToFwd < 0 ? 0 : daysToFwd; //проверка на корректность выбора дат
		}

		/// <summary>
		/// Установка ограничения на дату "до", если не выбран контрагент
		/// </summary>
		void CheckLimitationForDate()
		{
			if(entryreferenceClient.Subject == null
			   && dateperiodOrders.EndDate.Date > dateperiodOrders.StartDate.Date.AddMonths(1))
				dateperiodOrders.EndDateOrNull = dateperiodOrders.StartDate.Date.AddMonths(1);
		}

		protected void OnEnumcomboPaymentTypeChanged(object sender, EventArgs e)
		{
		}
	}
}


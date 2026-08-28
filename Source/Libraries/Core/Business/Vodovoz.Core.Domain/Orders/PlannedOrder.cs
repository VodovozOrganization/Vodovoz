using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Планируемый заказ
	/// Данные о точке доставки (или самовывозе) клиента, по которой ожидается плановый заказ,
	/// для создания сделки в Битрикс24
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "планируемые заказы",
		Nominative = "планируемый заказ"
	)]
	public class PlannedOrder : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationDate;
		private PlannedOrderBitrixDealStage _stage;
		private int _counterpartyId;
		private int? _deliveryPointId;
		private bool _isSelfDelivery;
		private string _counterpartyName;
		private string _counterpartyInn;
		private string _phoneNumber;
		private string _emailAddress;
		private string _deliveryPointAddress;
		private DateTime _lastOrderDeliveryDate;
		private DateTime _plannedOrderDate;
		private int _lastOrderBottlesCount;
		private int? _bottlesDebtByAddress;
		private int _bottlesDebtByCounterparty;
		private int _delayDaysForCounterparty;
		private decimal _debtorDebt;
		private int? _lastOrderId;
		private int? _salesManagerId;
		private string _salesManagerName;
		private long? _bitrixDealId;
		private int? _createdOrderId;
		private DateTime? _createdOrderDeliveryDate;
		private DateTime? _lastUpdateDate;
		private DateTime? _lastSynchronizedDate;
		private string _lastError;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Дата и время сбора данных
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		/// <summary>
		/// Стадия работы со сделкой по планируемому заказу
		/// </summary>
		[Display(Name = "Стадия")]
		public virtual PlannedOrderBitrixDealStage Stage
		{
			get => _stage;
			set => SetField(ref _stage, value);
		}

		/// <summary>
		/// Id контрагента
		/// </summary>
		[Display(Name = "Код контрагента")]
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}

		/// <summary>
		/// Id точки доставки, null - для самовывоза
		/// </summary>
		[Display(Name = "Код точки доставки")]
		public virtual int? DeliveryPointId
		{
			get => _deliveryPointId;
			set => SetField(ref _deliveryPointId, value);
		}

		/// <summary>
		/// Признак самовывоза
		/// </summary>
		[Display(Name = "Самовывоз")]
		public virtual bool IsSelfDelivery
		{
			get => _isSelfDelivery;
			set => SetField(ref _isSelfDelivery, value);
		}

		/// <summary>
		/// Наименование контрагента
		/// </summary>
		[Display(Name = "Наименование контрагента")]
		public virtual string CounterpartyName
		{
			get => _counterpartyName;
			set => SetField(ref _counterpartyName, value);
		}

		/// <summary>
		/// ИНН контрагента
		/// </summary>
		[Display(Name = "ИНН контрагента")]
		public virtual string CounterpartyInn
		{
			get => _counterpartyInn;
			set => SetField(ref _counterpartyInn, value);
		}

		/// <summary>
		/// Номер для связи, указанный в последнем выполненном заказе
		/// </summary>
		[Display(Name = "Номер телефона")]
		public virtual string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}

		/// <summary>
		/// Адрес электронной почты
		/// </summary>
		[Display(Name = "Адрес электронной почты")]
		public virtual string EmailAddress
		{
			get => _emailAddress;
			set => SetField(ref _emailAddress, value);
		}

		/// <summary>
		/// Адрес точки доставки, пусто - для самовывоза
		/// </summary>
		[Display(Name = "Адрес точки доставки")]
		public virtual string DeliveryPointAddress
		{
			get => _deliveryPointAddress;
			set => SetField(ref _deliveryPointAddress, value);
		}

		/// <summary>
		/// Номер последнего выполненного заказа, по которому рассчитана дата планируемого заказа
		/// </summary>
		[Display(Name = "Номер последнего заказа")]
		public virtual int? LastOrderId
		{
			get => _lastOrderId;
			set => SetField(ref _lastOrderId, value);
		}

		/// <summary>
		/// Дата доставки последнего выполненного заказа
		/// </summary>
		[Display(Name = "Дата доставки последнего заказа")]
		public virtual DateTime LastOrderDeliveryDate
		{
			get => _lastOrderDeliveryDate;
			set => SetField(ref _lastOrderDeliveryDate, value);
		}

		/// <summary>
		/// Дата планируемого заказа
		/// </summary>
		[Display(Name = "Дата планируемого заказа")]
		public virtual DateTime PlannedOrderDate
		{
			get => _plannedOrderDate;
			set => SetField(ref _plannedOrderDate, value);
		}

		/// <summary>
		/// Количество бутылей в последнем выполненном заказе
		/// </summary>
		[Display(Name = "Количество бутылей в последнем заказе")]
		public virtual int LastOrderBottlesCount
		{
			get => _lastOrderBottlesCount;
			set => SetField(ref _lastOrderBottlesCount, value);
		}

		/// <summary>
		/// Долг по бутылям по адресу, null - для самовывоза
		/// </summary>
		[Display(Name = "Долг по бутылям по адресу")]
		public virtual int? BottlesDebtByAddress
		{
			get => _bottlesDebtByAddress;
			set => SetField(ref _bottlesDebtByAddress, value);
		}

		/// <summary>
		/// Долг по бутылям по клиенту
		/// </summary>
		[Display(Name = "Долг по бутылям по клиенту")]
		public virtual int BottlesDebtByCounterparty
		{
			get => _bottlesDebtByCounterparty;
			set => SetField(ref _bottlesDebtByCounterparty, value);
		}

		/// <summary>
		/// Отсрочка по оплате для контрагента в днях, 0 - для физических лиц
		/// </summary>
		[Display(Name = "Отсрочка по оплате, дней")]
		public virtual int DelayDaysForCounterparty
		{
			get => _delayDaysForCounterparty;
			set => SetField(ref _delayDaysForCounterparty, value);
		}

		/// <summary>
		/// Общая дебиторская задолженность, 0 - для физических лиц
		/// </summary>
		[Display(Name = "Общая дебиторская задолженность")]
		public virtual decimal DebtorDebt
		{
			get => _debtorDebt;
			set => SetField(ref _debtorDebt, value);
		}

		/// <summary>
		/// Id закрепленного за контрагентом менеджера по продажам
		/// </summary>
		[Display(Name = "Код менеджера по продажам")]
		public virtual int? SalesManagerId
		{
			get => _salesManagerId;
			set => SetField(ref _salesManagerId, value);
		}

		/// <summary>
		/// ФИО закрепленного за контрагентом менеджера по продажам
		/// </summary>
		[Display(Name = "Менеджер по продажам")]
		public virtual string SalesManagerName
		{
			get => _salesManagerName;
			set => SetField(ref _salesManagerName, value);
		}

		/// <summary>
		/// Id созданной сделки в Битрикс24
		/// </summary>
		[Display(Name = "Код сделки Битрикс24")]
		public virtual long? BitrixDealId
		{
			get => _bitrixDealId;
			set => SetField(ref _bitrixDealId, value);
		}

		/// <summary>
		/// Номер созданного клиентом заказа, найденного по планируемому заказу
		/// </summary>
		[Display(Name = "Номер созданного заказа")]
		public virtual int? CreatedOrderId
		{
			get => _createdOrderId;
			set => SetField(ref _createdOrderId, value);
		}

		/// <summary>
		/// Дата доставки созданного клиентом заказа
		/// </summary>
		[Display(Name = "Дата доставки созданного заказа")]
		public virtual DateTime? CreatedOrderDeliveryDate
		{
			get => _createdOrderDeliveryDate;
			set => SetField(ref _createdOrderDeliveryDate, value);
		}

		/// <summary>
		/// Дата последнего изменения записи
		/// </summary>
		[Display(Name = "Дата обновления")]
		public virtual DateTime? LastUpdateDate
		{
			get => _lastUpdateDate;
			set => SetField(ref _lastUpdateDate, value);
		}

		/// <summary>
		/// Дата последней успешной синхронизации с Битрикс24
		/// </summary>
		[Display(Name = "Дата синхронизации")]
		public virtual DateTime? LastSynchronizedDate
		{
			get => _lastSynchronizedDate;
			set => SetField(ref _lastSynchronizedDate, value);
		}

		/// <summary>
		/// Последняя ошибка синхронизации с Битрикс24
		/// </summary>
		[Display(Name = "Последняя ошибка")]
		public virtual string LastError
		{
			get => _lastError;
			set => SetField(ref _lastError, value);
		}
	}
}

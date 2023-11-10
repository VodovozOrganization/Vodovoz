using fyiReporting.RDL;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Exceptions;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Services;
using QS.Services;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Models;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Orders;
using CounterpartyContractFactory = Vodovoz.Factories.CounterpartyContractFactory;
using IOrganizationProvider = Vodovoz.Models.IOrganizationProvider;
using Type = Vodovoz.Domain.Orders.Documents.Type;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "заказы",
		Nominative = "заказ",
		Prepositional = "заказе",
		PrepositionalPlural = "заказах"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class Order : BusinessObjectBase<Order>, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private static readonly IOrderRepository _orderRepository = new OrderRepository();
		private static readonly IPaymentItemsRepository _paymentItemsRepository = new PaymentItemsRepository();
		private static readonly IPaymentsRepository _paymentsRepository = new PaymentsRepository();

		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository = new UndeliveredOrdersRepository();
		private readonly IPaymentFromBankClientController _paymentFromBankClientController =
			new PaymentFromBankClientController(_paymentItemsRepository, _orderRepository, _paymentsRepository);

		private readonly INomenclatureRepository _nomenclatureRepository =
			new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));

		private readonly ICashReceiptRepository _cashReceiptRepository = new CashReceiptRepository(UnitOfWorkFactory.GetDefaultFactory,
			new OrderParametersProvider(new ParametersProvider()));

		private readonly IEmailRepository _emailRepository = new EmailRepository();

		private static readonly IGeneralSettingsParametersProvider _generalSettingsParameters =
			new GeneralSettingsParametersProvider(new ParametersProvider());

		private static readonly IOrderParametersProvider _orderParametersProvider =
			new OrderParametersProvider(new ParametersProvider());

		private static readonly IDeliveryScheduleParametersProvider _deliveryScheduleParametersProvider =
			new DeliveryScheduleParametersProvider(new ParametersProvider());

		private readonly OrderItemComparerForCopyingFromUndelivery _itemComparerForCopyingFromUndelivery =
			new OrderItemComparerForCopyingFromUndelivery();
		private readonly double _futureDeliveryDaysLimit = 30;

		private bool _isBottleStockDiscrepancy;

		#region Платная доставка

		private int paidDeliveryNomenclatureId;
		private int PaidDeliveryNomenclatureId
		{
			get
			{
				if(paidDeliveryNomenclatureId == default(int))
				{
					paidDeliveryNomenclatureId = new NomenclatureParametersProvider(new ParametersProvider()).PaidDeliveryNomenclatureId;
				}

				return paidDeliveryNomenclatureId;
			}
		}

		#endregion

		private Phone _contactPhone;
		private DateTime? _commentOPManagerUpdatedAt;
		private Employee _commentOPManagerChangedBy;
		private bool? _canCreateOrderInAdvance;
		private int? counterpartyExternalOrderId;

		private int? _callBeforeArrivalMinutes;

		#region Cвойства

		public virtual int Id { get; set; }

		public virtual IInteractiveService InteractiveService { get; set; }

		private DateTime version;
		[Display(Name = "Версия")]
		public virtual DateTime Version {
			get => version;
			set => SetField(ref version, value, () => Version);
		}

		private DateTime? createDate;
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreateDate {
			get => createDate;
			set => SetField(ref createDate, value, () => CreateDate);
		}

		private bool isFirstOrder;
		[Display(Name = "Первый заказ")]
		public virtual bool IsFirstOrder {
			get => isFirstOrder;
			set => SetField(ref isFirstOrder, value, () => IsFirstOrder);
		}

		bool _isSecondOrder;
		[Display(Name = "Второй заказ клиента")]
		public virtual bool IsSecondOrder
		{
			get => _isSecondOrder;
			set => SetField(ref _isSecondOrder, value);
		}

		private bool _isFastDelivery;
		[Display(Name = "Доставка за час")]
		public virtual bool IsFastDelivery
		{
			get => _isFastDelivery;
			set
			{
				if(SetField(ref _isFastDelivery, value) && value)
				{
					CallBeforeArrivalMinutes = null;
				}
			}
		}

		private OrderStatus orderStatus;

		[Display(Name = "Статус заказа")]
		public virtual OrderStatus OrderStatus {
			get => orderStatus;
			set => SetField(ref orderStatus, value, () => OrderStatus);
		}

		private OrderPaymentStatus orderPaymentStatus;
		[Display(Name = "Статус оплаты заказа")]
		public virtual OrderPaymentStatus OrderPaymentStatus {
			get => orderPaymentStatus;
			set => SetField(ref orderPaymentStatus, value);
		}

		private OrderAddressType _orderAddressType;
		[Display(Name = "Тип доставки заказа")]
		public virtual OrderAddressType OrderAddressType
		{
			get => _orderAddressType;
			set => SetField(ref _orderAddressType, value);
		}

		private Employee author;

		[Display(Name = "Создатель заказа")]
		public virtual Employee Author {
			get => author;
			set => SetField(ref author, value, () => Author);
		}

		private Employee acceptedOrderEmployee;
		[Display(Name = "Заказ подтвердил")]
		public virtual Employee AcceptedOrderEmployee {
			get => acceptedOrderEmployee;
			set => SetField(ref acceptedOrderEmployee, value);
		}

		private Counterparty client;
		[Display(Name = "Клиент")]
		public virtual Counterparty Client {
			get => client;
			set {
				if(value == client)
					return;
				if(_orderRepository.GetOnClosingOrderStatuses().Contains(OrderStatus)) {
					OnChangeCounterparty(value);
				} else if(client != null && !CanChangeContractor()) {
					OnPropertyChanged(nameof(Client));
					if(InteractiveService == null)
						throw new InvalidOperationException("Нельзя изменить клиента для заполненного заказа.");

					InteractiveService.ShowMessage(ImportanceLevel.Warning, "Нельзя изменить клиента для заполненного заказа.");
					return;
				}
				var oldClient = client;
				if(SetField(ref client, value, () => Client)) {
					if(Client == null || (DeliveryPoint != null && NHibernate.NHibernateUtil.IsInitialized(Client.DeliveryPoints) && !Client.DeliveryPoints.Any(d => d.Id == DeliveryPoint.Id))) {
						//FIXME Убрать когда поймем что проблемы с пропаданием точек доставки нет.
						logger.Warn("Очищаем точку доставки, при установке клиента. Возможно это не нужно.");
						DeliveryPoint = null;
					}
					if(oldClient != null) {
						UpdateContract();
					}

					RefreshContactPhone();

					IsSecondOrderSetter();
				}
			}
		}

		private DeliveryPoint deliveryPoint;

		[Display(Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint {
			get => deliveryPoint;
			set {
				var oldDeliveryPoint = deliveryPoint;
				if(SetField(ref deliveryPoint, value, () => DeliveryPoint) && value != null) {
					if(DeliverySchedule == null)
						DeliverySchedule = value.DeliverySchedule;

					if(Id == 0)
					{
						AddCertificates = DeliveryPoint.Category?.Id == EducationalInstitutionDeliveryPointCategoryId
										  && (DeliveryPoint.AddCertificatesAlways || Client.FirstOrder == null);
					}

					if(oldDeliveryPoint != null) {
						UpdateContract();
						RefreshContactPhone();
					}
				}
			}
		}

		private DateTime? timeDelivered;

		[Display(Name = "Время доставки")]
		public virtual DateTime? TimeDelivered {
			get => timeDelivered;
			set => SetField(ref timeDelivered, value, () => TimeDelivered);
		}

		private DateTime? _deliveryDate;

		[Display(Name = "Дата доставки")]
		[HistoryDateOnly]
		public virtual DateTime? DeliveryDate {
			get => _deliveryDate;
			set
			{
				var lastDate = _deliveryDate;
				if(SetField(ref _deliveryDate, value) &&
				   Contract != null && Contract.Id == 0)
				{
					UpdateContract();
				}
				if(Contract != null && Contract.Id != 0 && DeliveryDate.HasValue
				   && lastDate == Contract.IssueDate
				   && Contract.IssueDate != DeliveryDate.Value
				   && _orderRepository.CanChangeContractDate(UoW, Client, DeliveryDate.Value, Id)
				   && OrderStatus != OrderStatus.Closed)
				{
					Contract.IssueDate = DeliveryDate.Value.Date;
					InteractiveService.ShowMessage(ImportanceLevel.Warning,
						"Дата договора будет изменена при сохранении текущего заказа!");
				}
			}
		}

		private DateTime billDate = DateTime.Now;

		[Display(Name = "Дата счета")]
		[HistoryDateOnly]
		public virtual DateTime BillDate {
			get => billDate;
			set => SetField(ref billDate, value, () => BillDate);
		}

		private DeliverySchedule _deliverySchedule;

		[Display(Name = "Время доставки")]
		public virtual DeliverySchedule DeliverySchedule
		{
			get => _deliverySchedule;
			set 
			{
				SetField(ref _deliverySchedule, value);

				if(_deliverySchedule != null)
				{
					IsSecondOrderSetter();
				}
			}
		}

		private string deliverySchedule1c;

		[Display(Name = "Время доставки из 1С")]
		public virtual string DeliverySchedule1c {
			get => string.IsNullOrWhiteSpace(deliverySchedule1c)
				  ? "Время доставки из 1С не загружено"
				  : deliverySchedule1c;
			set => SetField(ref deliverySchedule1c, value, () => DeliverySchedule1c);
		}

		private bool selfDelivery;

		[Display(Name = "Самовывоз")]
		public virtual bool SelfDelivery
		{
			get => selfDelivery;
			set
			{
				if(SetField(ref selfDelivery, value) && value)
				{
					IsContractCloser = false;
					CallBeforeArrivalMinutes = null;
				}
			}
		}

		private bool payAfterShipment;

		[Display(Name = "Оплата после отгрузки")]
		public virtual bool PayAfterShipment {
			get => payAfterShipment;
			set => SetField(ref payAfterShipment, value, () => PayAfterShipment);
		}

		private Employee loadAllowedBy;

		[Display(Name = "Отгрузку разрешил")]
		public virtual Employee LoadAllowedBy {
			get => loadAllowedBy;
			set => SetField(ref loadAllowedBy, value, () => LoadAllowedBy);
		}

		private Order previousOrder;

		[Display(Name = "Предыдущий заказ")]
		public virtual Order PreviousOrder {
			get => previousOrder;
			set => SetField(ref previousOrder, value, () => PreviousOrder);
		}

		private string odzComment;
		[Display(Name = "Комментарий ОДЗ")]
		public virtual string ODZComment {
			get => odzComment;
			set => SetField(ref odzComment, value);
		}

		#region OPComment

		private string opComment;
		[Display(Name = "Комментарий ОП")]
		public virtual string OPComment {
			get => opComment;
			set => SetField(ref opComment, value);
		}

		[Display(Name = "Последнее изменение комментария менеджера")]
		public virtual DateTime? CommentOPManagerUpdatedAt
		{
			get => _commentOPManagerUpdatedAt;
			set => SetField(ref _commentOPManagerUpdatedAt, value);
		}

		[Display(Name = "Последний редактировал комментарий менеджера")]
		public virtual Employee CommentOPManagerChangedBy
		{
			get => _commentOPManagerChangedBy;
			set => SetField(ref _commentOPManagerChangedBy, value);
		}
		
		public virtual string LastOPCommentUpdate
		{
			get => CommentOPManagerChangedBy != null && CommentOPManagerUpdatedAt != null
				? $"Последнее изменение вносил(а) {CommentOPManagerChangedBy.FullName} в {CommentOPManagerUpdatedAt:HH:mm dd.MM.yy}"
				: string.Empty;
		}

		#endregion

		private int? bottlesReturn;

		[Display(Name = "Бутылей на возврат")]
		public virtual int? BottlesReturn {
			get => bottlesReturn;
			set => SetField(ref bottlesReturn, value, () => BottlesReturn);
		}

		private string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		[Display(Name = "Отзвон за")]
		public virtual int? CallBeforeArrivalMinutes
		{
			get => _callBeforeArrivalMinutes;
			set => SetField(ref _callBeforeArrivalMinutes, value);
		}

		private string commentLogist;

		[Display(Name = "Комментарий логиста")]
		public virtual string CommentLogist {
			get => commentLogist;
			set => SetField(ref commentLogist, value, () => CommentLogist);
		}

		private string clientPhone;

		[Display(Name = "Номер телефона")]
		public virtual string ClientPhone {
			get => clientPhone;
			set => SetField(ref clientPhone, value, () => ClientPhone);
		}

		private OrderSignatureType? signatureType;

		[Display(Name = "Подписание документов")]
		public virtual OrderSignatureType? SignatureType {
			get => signatureType;
			set => SetField(ref signatureType, value, () => SignatureType);
		}

		private decimal extraMoney;

		[Display(Name = "Доплата\\Переплата")]
		[PropertyChangedAlso(nameof(OrderCashSum))]
		public virtual decimal ExtraMoney {
			get => extraMoney;
			set => SetField(ref extraMoney, value, () => ExtraMoney);
		}

		private string sumDifferenceReason;

		[Display(Name = "Причина переплаты/недоплаты")]
		public virtual string SumDifferenceReason {
			get => sumDifferenceReason;
			set => SetField(ref sumDifferenceReason, value, () => SumDifferenceReason);
		}

		private bool shipped;

		[Display(Name = "Отгружено по платежке")]
		public virtual bool Shipped {
			get => shipped;
			set => SetField(ref shipped, value, () => Shipped);
		}

		private PaymentType _paymentType;

		[Display(Name = "Форма оплаты")]
		public virtual PaymentType PaymentType {
			get => _paymentType;
			set {
				if(value != _paymentType && SetField(ref _paymentType, value)) 
				{
					if(PaymentType != PaymentType.PaidOnline)
					{
						PaymentByCardFrom = null;
					}

					if(PaymentType != PaymentType.Terminal)
					{
						PaymentByTerminalSource = null;
					}

					if(PaymentType == PaymentType.Terminal && PaymentByTerminalSource == null)
					{
						PaymentByTerminalSource = Domain.Client.PaymentByTerminalSource.ByCard;
					}

					UpdateContractOnPaymentTypeChanged();
				}
			}
		}

		private PaymentByTerminalSource? _paymentByTerminalSource;

		[Display(Name = "Подтип оплаты по терминалу")]
		public virtual PaymentByTerminalSource? PaymentByTerminalSource
		{
			get => _paymentByTerminalSource;
			set => SetField(ref _paymentByTerminalSource, value);
		}

		private CounterpartyContract contract;

		[Display(Name = "Договор")]
		public virtual CounterpartyContract Contract {
			get => contract;
			set => SetField(ref contract, value, () => Contract);
		}

		[Display(Name = "Номер для связи")]
		public virtual Phone ContactPhone
		{
			get => _contactPhone;
			set => SetField(ref _contactPhone, value);
		}

		private MoneyMovementOperation moneyMovementOperation;
		[IgnoreHistoryTrace]
		public virtual MoneyMovementOperation MoneyMovementOperation {
			get => moneyMovementOperation;
			set => SetField(ref moneyMovementOperation, value, () => MoneyMovementOperation);
		}

		private BottlesMovementOperation bottlesMovementOperation;
		[IgnoreHistoryTrace]
		public virtual BottlesMovementOperation BottlesMovementOperation {
			get => bottlesMovementOperation;
			set => SetField(ref bottlesMovementOperation, value, () => BottlesMovementOperation);
		}

		private IList<DepositOperation> depositOperations;

		public virtual IList<DepositOperation> DepositOperations {
			get => depositOperations;
			set => SetField(ref depositOperations, value, () => DepositOperations);
		}

		private bool collectBottles;

		public virtual bool CollectBottles {
			get => collectBottles;
			set => SetField(ref collectBottles, value, () => CollectBottles);
		}

		private DefaultDocumentType? documentType;

		[Display(Name = "Тип безналичных документов")]
		public virtual DefaultDocumentType? DocumentType {
			get => documentType;
			set => SetField(ref documentType, value, () => DocumentType);
		}

		private string code1c;

		[Display(Name = "Код 1С")]
		public virtual string Code1c {
			get => code1c;
			set => SetField(ref code1c, value, () => Code1c);
		}

		private string address1c;

		[Display(Name = "Адрес 1С")]
		public virtual string Address1c {
			get => address1c;
			set => SetField(ref address1c, value, () => Address1c);
		}

		private string address1cCode;

		[Display(Name = "Код адреса 1С")]
		public virtual string Address1cCode {
			get => address1cCode;
			set => SetField(ref address1cCode, value, () => Address1cCode);
		}

		private string toClientText;

		[Display(Name = "Оборудование к клиенту")]
		public virtual string ToClientText {
			get => toClientText;
			set => SetField(ref toClientText, value, () => ToClientText);
		}

		private string fromClientText;

		[Display(Name = "Оборудование от клиента")]
		public virtual string FromClientText {
			get => fromClientText;
			set => SetField(ref fromClientText, value, () => FromClientText);
		}

		private NonReturnReason tareNonReturnReason;
		[Display(Name = "Причина несдачи тары")]
		public virtual NonReturnReason TareNonReturnReason {
			get => tareNonReturnReason;
			set => SetField(ref tareNonReturnReason, value, () => TareNonReturnReason);
		}

		private PaymentFrom _paymentByCardFrom;

		[Display(Name = "Место, откуда проведена оплата")]
		public virtual PaymentFrom PaymentByCardFrom
		{
			get => _paymentByCardFrom;
			set
			{
				if(SetField(ref _paymentByCardFrom, value, () => PaymentByCardFrom))
				{
					UpdateContract();
				}
			}
		}

		[Display(Name = "Колонка МЛ от клиента")]
		public virtual string EquipmentsFromClient {
			get {
				string result = "";
				List<OrderEquipment> equipments = OrderEquipments.Where(x => x.Direction == Direction.PickUp).ToList();
				foreach(var equip in equipments) {
					result += equip.NameString;
					result += " " + equip.Count.ToString() + "шт.";
					if(equip != equipments.Last()) {
						result += ", ";
					}
				}
				return result;
			}
		}

		[Display(Name = "Колонка МЛ к клиенту")]
		public virtual string EquipmentsToClient {
			get {
				string result = "";
				List<OrderEquipment> equipments = OrderEquipments.Where(x => x.Direction == Direction.Deliver).ToList();
				foreach(var equip in equipments) {
					result += equip.NameString;
					result += " " + equip.Count.ToString() + "шт.";
					if(equip != equipments.Last()) {
						result += ", ";
					}
				}
				return result;
			}
		}

		private int? dailyNumber;

		/// <summary>
		/// Уникальный номер в пределах одного дня
		/// </summary>
		[Display(Name = "Ежедневный номер")]
		public virtual int? DailyNumber {
			get => dailyNumber;
			set => SetField(ref dailyNumber, value, () => DailyNumber);
		}

		private Employee lastEditor;

		[Display(Name = "Последний редактор")]
		[IgnoreHistoryTrace]
		public virtual Employee LastEditor {
			get => lastEditor;
			set => SetField(ref lastEditor, value, () => LastEditor);
		}

		private DateTime lastEditedTime;

		[Display(Name = "Последние изменения")]
		[IgnoreHistoryTrace]
		public virtual DateTime LastEditedTime {
			get => lastEditedTime;
			set => SetField(ref lastEditedTime, value, () => LastEditedTime);
		}

		private string commentManager;
		/// <summary>
		/// Комментарий менеджера ответственного за водительский телефон
		/// </summary>
		[Display(Name = "Комментарий менеджера")]
		public virtual string CommentManager {
			get => commentManager;
			set => SetField(ref commentManager, value, () => CommentManager);
		}

		private string _driverMobileAppComment;
		[Display(Name = "Комментарий водителя из приложения")]
		public virtual string DriverMobileAppComment
		{
			get => _driverMobileAppComment;
			set => SetField(ref _driverMobileAppComment, value);
		}

		private DateTime? _driverMobileAppCommentTime;
		[Display(Name = "Время установки комментария водителя из приложения")]
		public virtual DateTime? DriverMobileAppCommentTime
		{
			get => _driverMobileAppCommentTime;
			set => SetField(ref _driverMobileAppCommentTime, value);
		}

		private int? returnedTare;

		[Display(Name = "Возвратная тара")]
		public virtual int? ReturnedTare {
			get => returnedTare;
			set => SetField(ref returnedTare, value, () => ReturnedTare);
		}

		private string informationOnTara;

		[Display(Name = "Информация о таре")]
		public virtual string InformationOnTara {
			get => informationOnTara;
			set => SetField(ref informationOnTara, value, () => InformationOnTara);
		}

		private bool isBottleStock;
		[Display(Name = "Акция \"Бутыль\" ")]
		public virtual bool IsBottleStock {
			get => isBottleStock;
			set => SetField(ref isBottleStock, value, () => IsBottleStock);
		}

		[Display(Name = "Расхождение между кол-вом фактически сданных и ожидаемых бутылей по акции \"Бутыль\"")]
		public virtual bool IsBottleStockDiscrepancy
		{
			get => _isBottleStockDiscrepancy;
			set => SetField(ref _isBottleStockDiscrepancy, value);
		}

		private bool isSelfDeliveryPaid;

		[Display(Name = "Самовывоз оплачен")]
		public virtual bool IsSelfDeliveryPaid
		{
			get => isSelfDeliveryPaid;
			set => SetField(ref isSelfDeliveryPaid, value);
		}

		private int bottlesByStockCount;
		[Display(Name = "Количество бутылей по акции")]
		public virtual int BottlesByStockCount {
			get => bottlesByStockCount;
			set => SetField(ref bottlesByStockCount, value, () => BottlesByStockCount);
		}

		private int bottlesByStockActualCount;
		[Display(Name = "Фактическое количество бутылей по акции")]
		public virtual int BottlesByStockActualCount {
			get => bottlesByStockActualCount;
			set => SetField(ref bottlesByStockActualCount, value, () => BottlesByStockActualCount);
		}

		private string onRouteEditReason;

		[Display(Name = "Причина редактирования заказа")]
		[Obsolete("Кусок выпиленного функционала от I-1060. Даша сказала пока не удалять, но скрыть зачем-то.")]
		public virtual string OnRouteEditReason {
			get => onRouteEditReason;
			set => SetField(ref onRouteEditReason, value, () => OnRouteEditReason);
		}

		private DriverCallType driverCallType;

		[Display(Name = "Водитель отзвонился")]
		public virtual DriverCallType DriverCallType {
			get => driverCallType;
			set => SetField(ref driverCallType, value, () => DriverCallType);
		}

		private int? driverCallId;

		[Display(Name = "Номер звонка водителя")]
		public virtual int? DriverCallId {
			get => driverCallId;
			set => SetField(ref driverCallId, value, () => DriverCallId);
		}

		private int? _trifle;

		[Display(Name = "Сдача с")]
		public virtual int? Trifle
		{
			get => _trifle;
			set => SetField(ref _trifle, value);
		}

		private int? onlineOrder;

		[Display(Name = "Номер онлайн заказа")]
		public virtual int? OnlineOrder {
			get => onlineOrder;
			set => SetField(ref onlineOrder, value, () => OnlineOrder);
		}

		private int? eShopOrder;
		[Display(Name = "Заказ из интернет магазина")]
		public virtual int? EShopOrder {
			get => eShopOrder;
			set => SetField(ref eShopOrder, value);
		}

		[Display(Name = "Идентификатор заказа в ИС контрагента")]
		public virtual int? CounterpartyExternalOrderId
		{
			get => counterpartyExternalOrderId;
			set => SetField(ref counterpartyExternalOrderId, value);
		}

		private bool isContractCloser;

		[Display(Name = "Заказ - закрывашка по контракту?")]
		public virtual bool IsContractCloser {
			get => isContractCloser;
			set => SetField(ref isContractCloser, value, () => IsContractCloser);
		}

		private bool isTareNonReturnReasonChangedByUser;
		[Display(Name = "Причина невозврата тары указана пользователем")]
		[IgnoreHistoryTrace]
		public virtual bool IsTareNonReturnReasonChangedByUser {
			get => isTareNonReturnReasonChangedByUser;
			set => SetField(ref isTareNonReturnReasonChangedByUser, value, () => IsTareNonReturnReasonChangedByUser);
		}

		private bool hasCommentForDriver;
		[Display(Name = "Есть комментарий для водителя?")]
		[IgnoreHistoryTrace]
		public virtual bool HasCommentForDriver {
			get => hasCommentForDriver;
			set => SetField(ref hasCommentForDriver, value, () => HasCommentForDriver);
		}

		private OrderSource orderSource = OrderSource.VodovozApp;

		[Display(Name = "Источник заказа")]
		public virtual OrderSource OrderSource {
			get => orderSource;
			set => SetField(ref orderSource, value);
		}

		private bool addCertificates;
		[Display(Name = "Добавить сертификаты продукции")]
		public virtual bool AddCertificates {
			get => addCertificates;
			set => SetField(ref addCertificates, value, () => AddCertificates);
		}

		private bool contactlessDelivery;
		[Display(Name = "Бесконтактная доставка")]
		public virtual bool ContactlessDelivery {
			get => contactlessDelivery;
			set => SetField(ref contactlessDelivery, value, () => ContactlessDelivery);
		}

		private ReturnTareReason returnTareReason;
		[Display(Name = "Причина забора тары")]
		public virtual ReturnTareReason ReturnTareReason {
			get => returnTareReason;
			set => SetField(ref returnTareReason, value);
		}

		private ReturnTareReasonCategory returnTareReasonCategory;
		[Display(Name = "Категория причины забора тары")]
		public virtual ReturnTareReasonCategory ReturnTareReasonCategory {
			get => returnTareReasonCategory;
			set => SetField(ref returnTareReasonCategory, value);
		}

		private bool _isCopiedFromUndelivery;
		[Display(Name = "Перенос из недовоза")]
		public virtual bool IsCopiedFromUndelivery {
			get => _isCopiedFromUndelivery;
			set => SetField(ref _isCopiedFromUndelivery, value);
		}

		private LogisticsRequirements _logisticsRequirements;
		[Display(Name = "Требования к логистике")]
		public virtual LogisticsRequirements LogisticsRequirements
		{
			get => _logisticsRequirements;
			set => SetField(ref _logisticsRequirements, value);
		}

		#endregion

		#region SecondOrderDiscount
		private void IsSecondOrderSetter()
		{
			if(!_generalSettingsParameters.GetIsClientsSecondOrderDiscountActive)
			{
				return;
			}

			var closingDocumentDeliveryScheduleId = _deliveryScheduleParametersProvider.ClosingDocumentDeliveryScheduleId;

			if(IsFirstOrder || DeliverySchedule?.Id == closingDocumentDeliveryScheduleId)
			{
				IsSecondOrder = false;
				return;
			}

			if(Id > 0 || IsSecondOrder)
			{
				return;
			}

			var firstOrderAvailableStatuses = new OrderStatus[] { OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };

			var firstOrder = UoW.GetAll<Order>()
				.Where(o =>
					o.Client == Client
					&& o.IsFirstOrder
					&& firstOrderAvailableStatuses.Contains(o.OrderStatus))
				.ToList();

			bool hasFirstOrder = firstOrder.Count() > 0;

			if(!hasFirstOrder)
			{
				return;
			}

			bool isFirstOrderIsClosingDocuments = false;

			if(firstOrder[0].DeliverySchedule?.Id == closingDocumentDeliveryScheduleId)
			{
				isFirstOrderIsClosingDocuments = true;
			}

			var nextOrdersAvailableStatuses = new OrderStatus[] { OrderStatus.Canceled, OrderStatus.NotDelivered };

			var nextOrders = UoW.GetAll<Order>()
				.Where(o =>
					o.Client == Client
					&& !o.IsFirstOrder
					&& o.Id != Id
					&& o.DeliverySchedule.Id != closingDocumentDeliveryScheduleId
					&& !nextOrdersAvailableStatuses.Contains(o.OrderStatus))
				.ToList();

			IsSecondOrder =
				isFirstOrderIsClosingDocuments
				? nextOrders.Count() == 1
				: nextOrders.Count() == 0;
		}

		public virtual void UpdateClientSecondOrderDiscount(IOrderDiscountsController discountsController)
		{
			if(!_generalSettingsParameters.GetIsClientsSecondOrderDiscountActive)
			{
				return;
			}

			int discountReasonId = _orderParametersProvider.GetClientsSecondOrderDiscountReasonId;

			if(IsSecondOrder)
			{
				SetClientSecondOrderDiscount(discountsController, discountReasonId);
				return;
			}

			ResetClientSecondOrderDiscount(discountsController, discountReasonId);
		}

		private void SetClientSecondOrderDiscount(IOrderDiscountsController discountsController, int discountReasonId)
		{
			if(IsSecondOrder)
			{
				foreach(var item in ObservableOrderItems)
				{
					if(item.DiscountReason?.Id != discountReasonId)
					{
						SetClientSecondOrderDiscount(discountsController, item, discountReasonId);
					}
				}
			}
		}

		private void ResetClientSecondOrderDiscount(IOrderDiscountsController discountsController, int discountReasonId)
		{
			if(!IsSecondOrder)
			{
				var orderItemsHavingClientsSecondOrderDiscount = new List<OrderItem>();

				foreach(var item in ObservableOrderItems)
				{
					if(item.DiscountReason?.Id == discountReasonId)
					{
						orderItemsHavingClientsSecondOrderDiscount.Add(item);
					}
				}
				discountsController.RemoveDiscountFromOrder(orderItemsHavingClientsSecondOrderDiscount);
			}
		}

		private void SetClientSecondOrderDiscount(IOrderDiscountsController discountsController, OrderItem orderItem, int discountReasonId)
		{
			if(!IsSecondOrder)
			{
				return;
			}

			if(orderItem.DiscountReason != null
				|| orderItem.PromoSet != null)
			{
				return;
			}

			var discountReason = UoW.GetById<DiscountReason>(discountReasonId);

			if(discountReason != null)
			{
				discountsController.SetDiscountFromDiscountReasonForOrderItem(discountReason, orderItem, true, out string message);

				if(message != null)
				{
					ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						$"Не удалось применить скидку для второго заказа клиента!");
				}
			}
		}
		#endregion

		public virtual bool CanChangeContractor()
		{
			return (!NHibernate.NHibernateUtil.IsInitialized(OrderDocuments) || !OrderDocuments.Any())
				&& (!NHibernate.NHibernateUtil.IsInitialized(InitialOrderService) || !InitialOrderService.Any())
				&& (!NHibernate.NHibernateUtil.IsInitialized(FinalOrderService) || !FinalOrderService.Any());
		}

		private IList<OrderDepositItem> orderDepositItems = new List<OrderDepositItem>();

		[Display(Name = "Залоги заказа")]
		public virtual IList<OrderDepositItem> OrderDepositItems {
			get => orderDepositItems;
			set => SetField(ref orderDepositItems, value, () => OrderDepositItems);
		}

		private GenericObservableList<OrderDepositItem> observableOrderDepositItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderDepositItem> ObservableOrderDepositItems {
			get {
				if(observableOrderDepositItems == null) {
					observableOrderDepositItems = new GenericObservableList<OrderDepositItem>(OrderDepositItems);
					observableOrderDepositItems.ListContentChanged += ObservableOrderDepositItems_ListContentChanged;
				}
				return observableOrderDepositItems;
			}
		}

		private IList<OrderDocument> orderDocuments = new List<OrderDocument>();

		[Display(Name = "Документы заказа")]
		public virtual IList<OrderDocument> OrderDocuments {
			get => orderDocuments;
			set => SetField(ref orderDocuments, value, () => OrderDocuments);
		}

		private GenericObservableList<OrderDocument> _observableOrderDocuments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderDocument> ObservableOrderDocuments => 
			_observableOrderDocuments?.ReconnectToObject(OrderDocuments) ?? (_observableOrderDocuments = new GenericObservableList<OrderDocument>(OrderDocuments));

		private IList<OrderItem> orderItems = new List<OrderItem>();

		[Display(Name = "Строки заказа")]
		public virtual IList<OrderItem> OrderItems {
			get => orderItems;
			set => SetField(ref orderItems, value, () => OrderItems);
		}

		private GenericObservableList<OrderItem> observableOrderItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderItem> ObservableOrderItems {
			get {
				if(observableOrderItems == null) {
					observableOrderItems = new GenericObservableList<OrderItem>(orderItems);
					observableOrderItems.ListContentChanged += ObservableOrderItems_ListContentChanged;
				}

				return observableOrderItems;
			}
		}

		private IList<OrderEquipment> orderEquipments = new List<OrderEquipment>();

		[Display(Name = "Список оборудования")]
		public virtual IList<OrderEquipment> OrderEquipments {
			get => orderEquipments;
			set => SetField(ref orderEquipments, value, () => OrderEquipments);
		}

		private GenericObservableList<OrderEquipment> observableOrderEquipments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderEquipment> ObservableOrderEquipments {
			get {
				if(observableOrderEquipments == null)
					observableOrderEquipments = new GenericObservableList<OrderEquipment>(orderEquipments);
				return observableOrderEquipments;
			}
		}

		private IList<ServiceClaim> initialOrderService = new List<ServiceClaim>();

		[Display(Name = "Список заявок на сервис")]
		public virtual IList<ServiceClaim> InitialOrderService {
			get => initialOrderService;
			set => SetField(ref initialOrderService, value, () => InitialOrderService);
		}

		private GenericObservableList<ServiceClaim> observableInitialOrderService;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ServiceClaim> ObservableInitialOrderService {
			get {
				if(observableInitialOrderService == null)
					observableInitialOrderService = new GenericObservableList<ServiceClaim>(InitialOrderService);
				return observableInitialOrderService;
			}
		}

		private IList<ServiceClaim> finalOrderService = new List<ServiceClaim>();

		[Display(Name = "Список заявок на сервис")]
		public virtual IList<ServiceClaim> FinalOrderService {
			get => finalOrderService;
			set => SetField(ref finalOrderService, value, () => FinalOrderService);
		}

		private GenericObservableList<ServiceClaim> observableFinalOrderService;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ServiceClaim> ObservableFinalOrderService {
			get {
				if(observableFinalOrderService == null)
					observableFinalOrderService = new GenericObservableList<ServiceClaim>(FinalOrderService);
				return observableFinalOrderService;
			}
		}

		private IList<PromotionalSet> promotionalSets = new List<PromotionalSet>();
		[Display(Name = "Промонаборы заказа")]
		public virtual IList<PromotionalSet> PromotionalSets {
			get => promotionalSets;
			set => SetField(ref promotionalSets, value, () => PromotionalSets);
		}

		private GenericObservableList<PromotionalSet> observablePromotionalSets;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PromotionalSet> ObservablePromotionalSets {
			get {
				if(observablePromotionalSets == null) {
					observablePromotionalSets = new GenericObservableList<PromotionalSet>(PromotionalSets);
					observablePromotionalSets.ElementRemoved += ObservablePromotionalSets_ElementRemoved;
				}
				return observablePromotionalSets;
			}
		}

		private Organization _ourOrganization;
		[Display(Name = "Наша организация")]
		public virtual Organization OurOrganization
		{
			get => _ourOrganization;
			set => SetField(ref _ourOrganization, value);
		}

		public Order()
		{
			Comment = string.Empty;
			OrderStatus = OrderStatus.NewOrder;
			OrderPaymentStatus = OrderPaymentStatus.None;
			SumDifferenceReason = string.Empty;
			ClientPhone = string.Empty;
		}

		public static Order CreateFromServiceClaim(ServiceClaim service, Employee author)
		{
			var order = new Order {
				client = service.Counterparty,
				DeliveryPoint = service.DeliveryPoint,
				DeliveryDate = service.ServiceStartDate,
				PaymentType = service.Payment,
				Author = author
			};
			service.InitialOrder = order;
			order.AddServiceClaimAsInitial(service);
			return order;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(DeliveryDate == null || DeliveryDate == default(DateTime))
				yield return new ValidationResult("В заказе не указана дата доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryDate) });

			if(validationContext.Items.ContainsKey("NewStatus")) {
				OrderStatus newStatus = (OrderStatus)validationContext.Items["NewStatus"];
				if((newStatus == OrderStatus.Accepted || newStatus == OrderStatus.WaitForPayment) && Client != null) {

					var key = new OrderStateKey(this, newStatus);
					var messages = new List<string>();
					if(!OrderAcceptProhibitionRulesRepository.CanAcceptOrder(key, ref messages)) {
						foreach(var msg in messages) {
							yield return new ValidationResult(msg);
						}
					}

					if(!SelfDelivery && DeliverySchedule == null)
						yield return new ValidationResult("В заказе не указано время доставки.",
							new[] { this.GetPropertyName(o => o.DeliverySchedule) });

					if(!IsLoadedFrom1C && PaymentType == PaymentType.Cashless && Client.TypeOfOwnership != "ИП" && !SignatureType.HasValue)
						yield return new ValidationResult("В заказе не указано как будут подписаны документы.",
							new[] { this.GetPropertyName(o => o.SignatureType) });

					if(!IsLoadedFrom1C && bottlesReturn == null && this.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.water && !x.Nomenclature.IsDisposableTare))
						yield return new ValidationResult("В заказе не указана планируемая тара.",
							new[] { this.GetPropertyName(o => o.Contract) });
					if(bottlesReturn.HasValue && bottlesReturn > 0 && GetTotalWater19LCount() == 0 && ReturnTareReason == null)
						yield return new ValidationResult("Необходимо указать причину забора тары.",
							new[] { nameof(ReturnTareReason) });
					if(bottlesReturn.HasValue && bottlesReturn > 0 && GetTotalWater19LCount() == 0 && ReturnTareReasonCategory == null)
						yield return new ValidationResult("Необходимо указать категорию причины забора тары.",
							new[] { nameof(ReturnTareReasonCategory) });

					if(!IsLoadedFrom1C && _trifle == null && (PaymentType == PaymentType.Cash) && this.OrderSum > 0m)
						yield return new ValidationResult("В заказе не указана сдача.",
							new[] { this.GetPropertyName(o => o.Trifle) });
					if(ObservableOrderItems.Any(x => x.Count <= 0) || ObservableOrderEquipments.Any(x => x.Count <= 0))
						yield return new ValidationResult("В заказе должно быть указано количество во всех позициях товара и оборудования");
					//если ни у точки доставки, ни у контрагента нет ни одного номера телефона
					if(!IsLoadedFrom1C && !((DeliveryPoint != null && DeliveryPoint.Phones.Any()) || Client.Phones.Any()))
						yield return new ValidationResult("Ни для контрагента, ни для точки доставки заказа не указано ни одного номера телефона.");

					if(!IsLoadedFrom1C && DeliveryPoint != null) {
						if(string.IsNullOrWhiteSpace(DeliveryPoint.Entrance)) {
							yield return new ValidationResult("Не заполнена парадная в точке доставки");
						}
						if(string.IsNullOrWhiteSpace(DeliveryPoint.Floor)) {
							yield return new ValidationResult("Не заполнен этаж в точке доставки");
						}
						if(string.IsNullOrWhiteSpace(DeliveryPoint.Room)) {
							yield return new ValidationResult("Не заполнен номер помещения в точке доставки");
						}
					}

					List<string> incorrectPriceItems = new List<string>();
					string priceResult = "В заказе неверно указаны цены на следующие товары:\n";
					
					if(!IsCopiedFromUndelivery)
					{
						OrderItemsPriceValidation(ObservableOrderItems, incorrectPriceItems);
					}
					else //если копия из недовоза сверяем цены с переносимым заказом
					{
						var currentCopiedItems = ObservableOrderItems.Where(oi => oi.CopiedFromUndelivery != null).ToArray();
					
						//сначала проверяем все позиции у которых можно менять цену из старого заказа
						CopiedOrderItemsPriceValidation(currentCopiedItems, incorrectPriceItems);

						//затем смотрим у новых добавленных, если таковые имеются
						var newAddedItems = ObservableOrderItems.Where(oi => oi.CopiedFromUndelivery == null).ToArray();

						if(newAddedItems.Any())
						{
							OrderItemsPriceValidation(newAddedItems, incorrectPriceItems);
						}
					}

					if(incorrectPriceItems.Any())
					{
						foreach(string item in incorrectPriceItems)
						{
							priceResult += item;
						}

						yield return new ValidationResult(priceResult);
					}
					// Конец проверки цен

					//создание нескольких заказов на одну дату и точку доставки

					var canCreateSeveralOrdersValidationResult = ValidateCanCreateSeveralOrderForDateAndDeliveryPoint(validationContext);

					if(canCreateSeveralOrdersValidationResult != ValidationResult.Success)
					{
						yield return canCreateSeveralOrdersValidationResult;
					}

					if(Client.IsDeliveriesClosed
						&& PaymentType != PaymentType.Cash
						&& PaymentType != PaymentType.PaidOnline
						&& PaymentType != PaymentType.Terminal
						&& PaymentType != PaymentType.DriverApplicationQR
						&& PaymentType != PaymentType.SmsQR)
						yield return new ValidationResult(
							"В заказе неверно указан тип оплаты (для данного клиента закрыты поставки)",
							new[] { nameof(PaymentType) }
						);

					//FIXME Исправить изменение данных. В валидации нельзя менять объекты.
					if(DeliveryPoint != null && !DeliveryPoint.FindAndAssociateDistrict(UoW))
						yield return new ValidationResult(
							"Район доставки не найден. Укажите правильные координаты или разметьте район доставки.",
							new[] { this.GetPropertyName(o => o.DeliveryPoint) }
					);

					if(Client.DoNotMixMarkedAndUnmarkedGoodsInOrder && HasMarkedAndUnmarkedOrderItems())
					{
						var doNotMixMarkedAndUnmarkedGoodsInOrderName =
							Client.GetPropertyInfo(c => c.DoNotMixMarkedAndUnmarkedGoodsInOrder)
							.GetCustomAttribute<DisplayAttribute>(true).Name;

						yield return new ValidationResult(
							$"У клиента стоит признак \"{doNotMixMarkedAndUnmarkedGoodsInOrderName}\"",
							new[] { nameof(OrderItems) });
					}
				}

				if(newStatus == OrderStatus.Closed) {
					foreach(var equipment in OrderEquipments.Where(x => x.Direction == Direction.PickUp)) {
						if(!equipment.Confirmed && string.IsNullOrWhiteSpace(equipment.ConfirmedComment))
							yield return new ValidationResult(
								string.Format("Забор оборудования {0} по заказу {1} не произведен, а в комментарии не указана причина.",
									equipment.NameString, Id),
								new[] { this.GetPropertyName(o => o.OrderEquipments) });
					}
				}

				if(OrderAddressType == OrderAddressType.Service && PaymentType == PaymentType.Cashless
				   && newStatus == OrderStatus.Accepted
				   && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_accept_cashles_service_orders"))
				   {
					yield return new ValidationResult(
						"Недостаточно прав для подтверждения безнального сервисного заказа. Обратитесь к руководителю.",
						new[] { this.GetPropertyName(o => o.OrderStatus) }
					);
				}

				if(IsContractCloser && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_contract_closer"))
				{
					yield return new ValidationResult(
						"Недостаточно прав для подтверждения зыкрывашки по контракту. Обратитесь к руководителю.",
						new[] { this.GetPropertyName(o => o.IsContractCloser) }
					);
				}
			}

			bool isTransferedAddress = validationContext.Items.ContainsKey("AddressStatus") && (RouteListItemStatus)validationContext.Items["AddressStatus"] == RouteListItemStatus.Transfered;
			if (validationContext.Items.ContainsKey("cash_order_close") && (bool)validationContext.Items["cash_order_close"] )
				if (PaymentType == PaymentType.Terminal && OnlineOrder == null && !_orderRepository.GetUndeliveryStatuses().Contains(OrderStatus) && !isTransferedAddress)
					yield return new ValidationResult($"В заказе с оплатой по терминалу №{Id} отсутствует номер оплаты.");

			if (ObservableOrderItems.Any(x => x.Discount > 0 && x.DiscountReason == null && x.PromoSet == null))
				yield return new ValidationResult("Если в заказе указана скидка на товар, то обязательно должно быть заполнено поле 'Основание'.");

			if(!SelfDelivery && DeliveryPoint == null)
				yield return new ValidationResult("В заказе необходимо заполнить точку доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryPoint) });
			if(DeliveryPoint != null && (!DeliveryPoint.Latitude.HasValue || !DeliveryPoint.Longitude.HasValue))
			{
				yield return new ValidationResult($"В точке доставки №{DeliveryPoint.Id} {DeliveryPoint.ShortAddress} необходимо указать координаты.",
				new[] { nameof(DeliveryPoint) });
			}

			if(DriverCallId != null && string.IsNullOrWhiteSpace(CommentManager)){
				yield return new ValidationResult("Необходимо заполнить комментарий менеджера водительского телефона.",
					new[] { this.GetPropertyName(o => o.CommentManager) });
			}

			if (Client == null)
				yield return new ValidationResult("В заказе необходимо заполнить поле \"клиент\".",
					new[] { this.GetPropertyName(o => o.Client) });

			if(PaymentType == PaymentType.PaidOnline && OnlineOrder == null)
				yield return new ValidationResult("Если в заказе выбран тип оплаты по карте, необходимо заполнить номер онлайн заказа.",
												  new[] { this.GetPropertyName(o => o.OnlineOrder) });

			if(PaymentType == PaymentType.PaidOnline && PaymentByCardFrom == null)
				yield return new ValidationResult(
					"Выбран тип оплаты по карте. Необходимо указать откуда произведена оплата.",
					new[] { this.GetPropertyName(o => o.PaymentByCardFrom) }
				);

			if(
				ObservableOrderEquipments
			   .Where(x => x.Nomenclature.Category == NomenclatureCategory.equipment)
			   .Any(x => x.OwnType == OwnTypes.None)
			  )
				yield return new ValidationResult("У оборудования в заказе должна быть выбрана принадлежность.");

			if(
				ObservableOrderEquipments
			   .Where(x => x.Nomenclature.Category == NomenclatureCategory.equipment)
			   .Where(x => x.DirectionReason == DirectionReason.None && x.OwnType != OwnTypes.Duty)
			   .Any(x => x.Nomenclature?.SaleCategory != SaleCategory.forSale)
			  )
				yield return new ValidationResult("У оборудования в заказе должна быть указана причина забор-доставки.");

			if(ObservableOrderDepositItems.Any(x => x.ActualSum < 0)) {
				yield return new ValidationResult("В возврате залогов в заказе необходимо вводить положительную сумму.");
			}

			if(!_canCreateOrderInAdvance.HasValue)
			{
				_canCreateOrderInAdvance =
					ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_can_create_order_in_advance");
			}
			if(!_canCreateOrderInAdvance.Value
			   && DeliveryDate.HasValue && DeliveryDate.Value < DateTime.Today
			   && OrderStatus <= OrderStatus.Accepted) {
				yield return new ValidationResult(
					"Указана дата заказа более ранняя чем сегодняшняя. Укажите правильную дату доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryDate) }
				);
			}

			if(DeliveryDate > DateTime.Now.AddDays(_futureDeliveryDaysLimit))
			{
				yield return new ValidationResult(
					$"Дата доставки заказа должна быть не более {_futureDeliveryDaysLimit} дней вперед. Измените, пожалуйста, дату доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryDate) }
				);
			}

			if(SelfDelivery && PaymentType == PaymentType.ContractDocumentation) {
				yield return new ValidationResult(
					"Тип оплаты - контрактная документация невозможен для самовывоза",
					new[] { this.GetPropertyName(o => o.PaymentType) }
				);
			}

			if(SelfDelivery && PaymentType == PaymentType.DriverApplicationQR)
			{
				yield return new ValidationResult(
					"Тип оплаты - Qr-код МП водителя невозможен для самовывоза",
					new[] { this.GetPropertyName(o => o.PaymentType) }
				);
			}

			if(IsFastDelivery)
			{
				AddFastDeliveryNomenclatureIfNeeded();
			}

			if(!PaymentTypesFastDeliveryAvailableFor.Contains(PaymentType) && IsFastDelivery)
			{
				yield return new ValidationResult(
					$"Доставку за час можно выбрать только для заказа с формой оплаты из списка: {string.Join(", ", PaymentTypesFastDeliveryAvailableFor)}",
					new[] { nameof(PaymentType) });
			}

			var deliveryParameters = validationContext.GetService<IDeliveryRulesParametersProvider>();
			var isFastDeliverySchedule = DeliverySchedule?.Id == deliveryParameters.FastDeliveryScheduleId;
			if(IsFastDelivery != isFastDeliverySchedule)
			{
				yield return new ValidationResult(
					"Свойство заказа 'Доставка за час' должно совпадать с графиком доставки заказа 'Доставка за час'",
					new[] { nameof(PaymentType) });
			}
			if(IsFastDelivery && (DeliveryPoint == null || SelfDelivery))
			{
				yield return new ValidationResult("Нельзя выбрать доставку за час для заказа-самовывоза", new[] { nameof(DeliveryPoint) });
			}

			var orderParametersProvider = validationContext.GetService(typeof(IOrderParametersProvider)) as IOrderParametersProvider;

			if(SelfDelivery && PaymentType == PaymentType.PaidOnline && PaymentByCardFrom != null && OnlineOrder == null)
			{
				if(orderParametersProvider == null)
				{
					throw new ArgumentNullException(nameof(IOrderParametersProvider));
				}
				if(PaymentByCardFrom.Id == orderParametersProvider.PaymentFromTerminalId)
				{
					yield return new ValidationResult($"В заказe №{Id} с формой оплаты По карте и источником оплаты Терминал отсутствует номер оплаты.");
				}
			}

			if((new[] { PaymentType.Cash, PaymentType.Terminal }.Contains(PaymentType)
				   || (PaymentType == PaymentType.PaidOnline
					   && PaymentByCardFrom != null
					   && !(orderParametersProvider ?? throw new ArgumentNullException(nameof(IOrderParametersProvider)))
						   .PaymentsByCardFromNotToSendSalesReceipts.Contains(PaymentByCardFrom.Id)))
			   && Contract?.Organization != null && Contract.Organization.CashBoxId == null)
			{
				yield return new ValidationResult(
					"Невозможно сохранить заказ.\n" +
					$"К нашей организации '{Contract.Organization.Name}' не привязан кассовый аппарат.\n" +
					"Измените в заказе либо форму оплаты, либо нашу организацию",
					new[] { nameof(Contract.Organization) });
			}

			if (OrderItems.Any(oi => !string.IsNullOrWhiteSpace(oi.Nomenclature.OnlineStoreExternalId))
				&& EShopOrder == null)
			{
				yield return new ValidationResult(
					"В заказе есть товары ИМ, но не указан номер заказа ИМ",
					new[] { this.GetPropertyName(o => o.EShopOrder) }
				);
			}

			if (DeliveryPoint != null)
			{
				if (DeliveryPoint.MinimalOrderSumLimit != 0 && OrderSum < DeliveryPoint.MinimalOrderSumLimit)
				{
					yield return new ValidationResult(
						"Сумма заказа меньше минимальной погоровой установленной для точки доставки",
						new[] { this.GetPropertyName(o => o.OrderSum) }
					);
				}

				if (DeliveryPoint.MaximalOrderSumLimit != 0 && OrderSum > DeliveryPoint.MaximalOrderSumLimit)
				{
					yield return new ValidationResult(
						"Сумма заказа больше максимальной погоровой установленной для точки доставки",
						new[] { this.GetPropertyName(o => o.OrderSum) }
					);
				}
			}

			if(ContactPhone != null && ContactPhone.Counterparty?.Id != Client?.Id && ContactPhone.DeliveryPoint?.Id != DeliveryPoint?.Id)
			{
				yield return new ValidationResult($"Номер для связи с Id {ContactPhone.Id} : {ContactPhone.Number} не найден в списке телефонных номеров ни контрагента, ни точки доставки.",
					new[] { nameof(ContactPhone) });
			}

			if(OPComment?.Length > 255)
			{
				yield return new ValidationResult($"Значение поля \"Комментарий ОП/ОСК\" не должно превышать 255 символов",
					new[] { nameof(OPComment) });
			}

			if(ODZComment?.Length > 255)
			{
				yield return new ValidationResult($"Значение поля \"Комментарий ОДЗ\" не должно превышать 255 символов",
					new[] { nameof(OPComment) });
			}

			#region Валидация, если уже есть чек

			var hasReceipts = _orderRepository.OrderHasSentReceipt(UoW, Id);

			if(hasReceipts)
			{
				var incorrectReceiptItems = new List<string>();

				using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Валидация заказа, если уже есть чек"))
				{
					if(uow.GetById<Order>(Id) is Order oldOrder)
					{
						incorrectReceiptItems = ValidateChangesInOrderWithReceipt(uow, oldOrder);
					}
				}

				if(incorrectReceiptItems.Any())
				{
					yield return new ValidationResult(string.Join("\n", incorrectReceiptItems));
				}
			}

			#endregion
		}

		private void CopiedOrderItemsPriceValidation(OrderItem[] currentCopiedItems, List<string> incorrectPriceItems)
		{
			for(var i = 0; i < currentCopiedItems.Length; i++)
			{
				var currentItem = currentCopiedItems[i];
				var copiedItem = UoW.GetById<OrderItem>(currentItem.CopiedFromUndelivery.Id);
				
				if(currentItem.Price < copiedItem.Price)
				{
					incorrectPriceItems.Add(
						$"{currentItem.NomenclatureString} - цена: {currentItem.Price}, должна быть: {copiedItem.Price}\n");
				}
			}
		}

		private void OrderItemsPriceValidation(IEnumerable<OrderItem> validatedOrderItems, IList<string> incorrectPriceItems)
		{
			// Проверка соответствия цен в заказе ценам в номенклатуре
			foreach(var item in validatedOrderItems)
			{
				decimal fixedPrice = GetFixedPrice(item);
				decimal nomenclaturePrice = GetNomenclaturePrice(item, false);
				var alternativeNomenclaturePrice = GetNomenclaturePrice(item, true);

				var conditionForNomenclaturePrice = nomenclaturePrice > default(decimal)
				          && item.Price < nomenclaturePrice
				          && (alternativeNomenclaturePrice == default(decimal)
							  || item.Price < alternativeNomenclaturePrice);

				var conditionForAlternativeNomenclaturePrice = alternativeNomenclaturePrice > default(decimal)
				          && item.Price < alternativeNomenclaturePrice
						  && (nomenclaturePrice == default(decimal)
						      || item.Price < nomenclaturePrice);

				if(fixedPrice > 0m)
				{
					if(item.Price < fixedPrice)
					{
						incorrectPriceItems.Add(string.Format("{0} - цена: {1}, должна быть: {2}\n",
							item.NomenclatureString,
							item.Price,
							fixedPrice));
					}
				}
				else if(conditionForNomenclaturePrice || conditionForAlternativeNomenclaturePrice)
				{
					incorrectPriceItems.Add(
							$"{item.NomenclatureString} - цена: {item.Price}, должна быть: {nomenclaturePrice}, либо {alternativeNomenclaturePrice}");
				}
			}
		}

		private List<string> ValidateChangesInOrderWithReceipt(IUnitOfWork uow, Order oldOrder)
		{
			List<string> incorrectReceiptItems = new List<string>();

			var oldOrderItems = _orderRepository.GetIsAccountableInTrueMarkOrderItems(uow, Id).GroupBy(x => x.Nomenclature.Id).ToArray();

			if(oldOrder.Client.Id != Client.Id)
			{
				incorrectReceiptItems.Add($"Нельзя менять клиента у заказа, по которому сформирован чек.");
			}

			var newOrderItems = OrderItems
				.Where(x => x.Nomenclature.IsAccountableInTrueMark)
				.GroupBy(x => x.Nomenclature.Id)
				.ToArray();

			var missingInNewOrderIds = oldOrderItems
				.Select(x => x.Key)
				.Except(newOrderItems.Select(x => x.Key))
				.ToArray();

			if(missingInNewOrderIds.Any())
			{
				var missingNames = oldOrderItems
					.Where(x => missingInNewOrderIds.Contains(x.Key))
					.Select(x => x.First().Nomenclature.Name);

				incorrectReceiptItems.Add($"Нельзя удалять номенклатуры, по которым сформирован чек: {string.Join(", ", missingNames)}.");
			}

			var missingInOldOrderIds = newOrderItems
				.Select(x => x.Key)
				.Except(oldOrderItems.Select(x => x.Key))
				.ToArray();

			if(missingInOldOrderIds.Any())
			{
				var newNames = newOrderItems
					.Where(x => missingInOldOrderIds.Contains(x.Key))
					.Select(x => x.First().Nomenclature.Name);

				incorrectReceiptItems.Add($"Нельзя добавлять новые номенклатуры в заказ, по которому сформирован чек: {string.Join(", ", newNames)}.");
			}

			foreach(var oldItem in oldOrderItems)
			{
				var newItem = newOrderItems
					.Where(x => x.Key == oldItem.Key)
					.ToArray();

				if(!newItem.Any())
				{
					continue;
				}

				var oldCount = oldItem.Sum(x => x.Count);
				var newCount = newItem.First().Sum(x => x.Count);

				if(oldCount != newCount)
				{
					incorrectReceiptItems.Add($"Нельзя менять кол-во номенклатуры {newItem.First().First().Nomenclature.Name}, " +
					                          $"по которой сформирован чек (было {oldCount:F0} стало {newCount:F0}).");
				}
			}

			return incorrectReceiptItems;
		}

		#endregion

		#region Вычисляемые

		public virtual bool IsLoadedFrom1C => !string.IsNullOrEmpty(Code1c);

		public override string ToString() => IsLoadedFrom1C ? string.Format("Заказ №{0}({1})", Id, Code1c) : string.Format("Заказ №{0}", Id);

		public virtual string Title => string.Format("Заказ №{0} от {1:d}", Id, DeliveryDate);

		public virtual int Total19LBottlesToDeliver =>
			(int)OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water &&
									   x.Nomenclature.TareVolume == TareVolume.Vol19L).Sum(x => x.Count);

		public virtual int Total6LBottlesToDeliver =>
			(int)OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water &&
									   x.Nomenclature.TareVolume == TareVolume.Vol6L).Sum(x => x.Count);

		public virtual int Total600mlBottlesToDeliver =>
			(int)OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water &&
									   x.Nomenclature.TareVolume == TareVolume.Vol600ml).Sum(x => x.Count);

		public virtual int TotalWeight =>
			(int)OrderItems.Sum(x => x.Count * (decimal) x.Nomenclature.Weight);

		public virtual decimal TotalVolume =>
			OrderItems.Sum(x => x.Count * (decimal) x.Nomenclature.Volume);

		[Display(Name = "Наличных к получению")]
		public virtual decimal OrderCashSum
		{
			get => PaymentType == PaymentType.Cash ? OrderSum : 0;
			protected set {; }
		}

		/// <summary>
		/// Полная сумма заказа
		/// </summary>
		[PropertyChangedAlso(nameof(OrderCashSum))]
		public virtual decimal OrderSum => OrderPositiveSum - OrderNegativeSum;

		/// <summary>
		/// Вся положительная сумма заказа
		/// </summary>
		public virtual decimal OrderPositiveSum {
			get {
				decimal sum = 0;
				foreach(OrderItem item in ObservableOrderItems) {
					sum += item.ActualSum;
				}
				return sum;
			}
		}

		/// <summary>
		/// Вся положительная изначальная сумма заказа
		/// </summary>
		public virtual decimal OrderPositiveOriginalSum
		{
			get
			{
				decimal sum = 0;
				foreach(OrderItem item in ObservableOrderItems)
				{
					sum += item.Sum;
				}
				return sum;
			}
		}

		/// <summary>
		/// Вся отрицательная сумма заказа
		/// </summary>
		public virtual decimal OrderNegativeSum {
			get {
				decimal sum = 0;
				foreach(OrderDepositItem dep in ObservableOrderDepositItems) {
					sum += dep.ActualSum;
				}
				return sum;
			}
		}

		public virtual decimal BottleDepositSum => ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Bottles).Sum(x => x.ActualSum);
		public virtual decimal EquipmentDepositSum => ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Equipment).Sum(x => x.ActualSum);

		[Obsolete("Должно быть не актуально после ввода новой системы расчёта ЗП (I-2150)")]
		public virtual decimal MoneyForMaster =>
			ObservableOrderItems.Where(i => i.Nomenclature.Category == NomenclatureCategory.master && i.ActualCount.HasValue)
								.Sum(i => (decimal)i.Nomenclature.PercentForMaster / 100 * i.ActualCount.Value * i.Price);

		public virtual decimal? ActualGoodsTotalSum =>
			OrderItems.Sum(item => Decimal.Round(item.Price * item.ActualCount - item.DiscountMoney ?? 0, 2));

		public virtual bool CanBeMovedFromClosedToAcepted =>
			new RouteListItemRepository().WasOrderInAnyRouteList(UoW, this)
				&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_move_order_from_closed_to_acepted");

		public virtual bool HasItemsNeededToLoad => ObservableOrderItems.Any(orderItem =>
				!Nomenclature.GetCategoriesNotNeededToLoad().Contains(orderItem.Nomenclature.Category) && !orderItem.Nomenclature.NoDelivery)
			|| ObservableOrderEquipments.Any(orderEquipment =>
				!Nomenclature.GetCategoriesNotNeededToLoad().Contains(orderEquipment.Nomenclature.Category) && !orderEquipment.Nomenclature.NoDelivery);

		public virtual bool IsCashlessPaymentTypeAndOrganizationWithoutVAT => PaymentType == PaymentType.Cashless
			&& (Contract?.Organization?.WithoutVAT ?? false);

		public virtual void RefreshContactPhone()
		{
			if(ContactPhone?.Counterparty?.Id != Client?.Id && ContactPhone?.DeliveryPoint?.Id != DeliveryPoint?.Id)
			{
				ContactPhone = null;
			}
		}

		public virtual bool HasPermissionsForAlternativePrice => _generalSettingsParameters.SubdivisionsForAlternativePrices.Contains(Author.Subdivision.Id);

		#endregion

		#region Автосоздание договоров, при изменении подтвержденного заказа

		private void OnChangeCounterparty(Counterparty newClient)
		{
			if(newClient == null || Client == null || newClient.Id == Client.Id) {
				return;
			}
			UpdateContract();
		}

		private void UpdateContractOnPaymentTypeChanged()
		{
			UpdateContract(true);
		}

		private OrderOrganizationProviderFactory orderOrganizationProviderFactory;
		private IOrganizationProvider orderOrganizationProvider;
		private CounterpartyContractRepository counterpartyContractRepository;
		private CounterpartyContractFactory counterpartyContractFactory;

		/// <summary>
		/// <b>Не должен вызываться при создании сущности NHibernate'ом</b>
		/// </summary>
		private void UpdateContract(bool onPaymentTypeChanged = false)
		{
			//Если Initialize вызывается при создании сущности NHibernate'ом,
			//то почему-то не загружаются OrderItems и OrderDocuments (А возможно и вообще все коллекции Order)
			if(!NHibernateUtil.IsInitialized(Client))
			{
				NHibernateUtil.Initialize(Client);
			}
			if(!NHibernateUtil.IsInitialized(Contract))
			{
				NHibernateUtil.Initialize(Contract);
			}
			if(!NHibernateUtil.IsInitialized(Client) || !NHibernateUtil.IsInitialized(Contract))
			{
				return;
			}

			if(CreateDate != null
				&& CreateDate <= new DateTime(2020, 12, 16)
				&& Contract != null
				&& !onPaymentTypeChanged
				&& Contract.Counterparty == Client)
			{
				return;
			}

			ForceUpdateContract();
		}
		
		public virtual void ForceUpdateContract(Organization organization = null)
		{
			if(orderOrganizationProviderFactory == null) {
				orderOrganizationProviderFactory = new OrderOrganizationProviderFactory();
				orderOrganizationProvider = orderOrganizationProviderFactory.CreateOrderOrganizationProvider();
				var parametersProvider = new ParametersProvider();
				var orderParametersProvider = new OrderParametersProvider(parametersProvider);
				var cashReceiptRepository = new CashReceiptRepository(UnitOfWorkFactory.GetDefaultFactory, orderParametersProvider);
				counterpartyContractRepository = new CounterpartyContractRepository(orderOrganizationProvider, cashReceiptRepository);
				counterpartyContractFactory = new CounterpartyContractFactory(orderOrganizationProvider, counterpartyContractRepository);
			}
			
			UpdateOrCreateContract(UoW, counterpartyContractRepository, counterpartyContractFactory, organization);
		}

		private void UpdateContractDocument()
		{
			var contractDocuments = OrderDocuments.Where(x =>
				x.Type == OrderDocumentType.Contract && x.Order == this && x.AttachedToOrder == this);
			if(!contractDocuments.Any()) {
				return;
			}

			foreach(var contractDocument in contractDocuments.ToList()) {
				ObservableOrderDocuments.Remove(contractDocument);
			}

			AddContractDocument(Contract);
		}

		#endregion

		#region Добавление/удаление товаров
		public virtual void AddOrderItem(OrderItem orderItem, bool forceUseAlternativePrice = false)
		{
			if(ObservableOrderItems.Contains(orderItem)) {
				return;
			}

			var curCount = orderItem.Nomenclature.IsWater19L ? GetTotalWater19LCount(doNotCountWaterFromPromoSets: true) : orderItem.Count;
			var isAlternativePriceCopiedFromUndelivery = orderItem.CopiedFromUndelivery != null && orderItem.IsAlternativePrice;
			var canApplyAlternativePrice = isAlternativePriceCopiedFromUndelivery
			                               || (HasPermissionsForAlternativePrice
			                                   && orderItem.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= curCount)
			                                   && orderItem.GetWaterFixedPrice() == null);

			orderItem.IsAlternativePrice = canApplyAlternativePrice;

			ObservableOrderItems.Add(orderItem);
			UpdateContract();
		}

		private void RemoveOrderItem(OrderItem orderItem)
		{
			if(!ObservableOrderItems.Contains(orderItem)) {
				return;
			}

			if (orderItem.PromoSet != null)
			{
				var itemsToRemove = ObservableOrderItems.Where(oi => oi.PromoSet == orderItem.PromoSet).ToList();
				foreach (var item in itemsToRemove)
				{
					ObservableOrderItems.Remove(item);
				}
			} else
			{
				ObservableOrderItems.Remove(orderItem);
			}

			//Если была удалена последняя номенклатура "мастер" - переходит в стандартный тип адреса
			if(!OrderItems.Any(x => x.IsMasterNomenclature) && orderItem.IsMasterNomenclature)
			{
				OrderAddressType = OrderAddressType.Delivery;
			}

			UpdateContract();
		}

		#endregion

		#region Функции

		public virtual void AddDeliveryPointCommentToOrder()
		{
			if(DeliveryPoint == null)
			{
				return;
			}

			if(string.IsNullOrWhiteSpace(Comment))
			{
				Comment = DeliveryPoint.Comment;
			}
			else
			{
				Comment += $"\n{DeliveryPoint.Comment}";
			}
		}

		public virtual void UpdateAddressType()
		{
			if(Client != null && Client.IsChainStore && !OrderItems.Any(x => x.IsMasterNomenclature))
			{
				OrderAddressType = OrderAddressType.ChainStore;
			}
			if(Client != null && !Client.IsChainStore && !OrderItems.Any(x => x.IsMasterNomenclature) && OrderAddressType != OrderAddressType.StorageLogistics)
			{
				OrderAddressType = OrderAddressType.Delivery;
			}
		}

		private DiscountReason GetDiscountReasonStockBottle(
			IOrderParametersProvider orderParametersProvider, decimal discount)
		{
			var reasonId = discount == 10m
				? orderParametersProvider.GetDiscountReasonStockBottle10PercentsId
				: orderParametersProvider.GetDiscountReasonStockBottle20PercentsId;

			var discountReasonStockBottle = UoW.GetById<DiscountReason>(reasonId)
				?? throw new InvalidProgramException($"Не возможно найти причину скидки для акции Бутыль (id:{reasonId})");

			return discountReasonStockBottle;
		}

		/// <summary>
		/// Рассчитывает скидки в товарах по акции "Бутыль"
		/// </summary>
		public virtual void CalculateBottlesStockDiscounts(IOrderParametersProvider orderParametersProvider, bool byActualCount = false)
		{
			if(orderParametersProvider == null) {
				throw new ArgumentNullException(nameof(orderParametersProvider));
			}

			var bottlesByStock = byActualCount ? BottlesByStockActualCount : BottlesByStockCount;
			decimal stockBottleDiscountPercent = 0m;
			DiscountReason stockBottleDiscountReason = null;

			if(bottlesByStock == Total19LBottlesToDeliver)
			{
				stockBottleDiscountPercent = 10m;
				stockBottleDiscountReason = GetDiscountReasonStockBottle(orderParametersProvider, stockBottleDiscountPercent);
			}
			if(bottlesByStock > Total19LBottlesToDeliver)
			{
				stockBottleDiscountPercent = 20m;
				stockBottleDiscountReason = GetDiscountReasonStockBottle(orderParametersProvider, stockBottleDiscountPercent);
			}

			foreach(OrderItem item in ObservableOrderItems
				.Where(x => x.Nomenclature.Category == NomenclatureCategory.water)
				.Where(x => !x.Nomenclature.IsDisposableTare)
				.Where(x => x.Nomenclature.TareVolume == TareVolume.Vol19L)) {
				item.SetDiscountByStock(stockBottleDiscountReason, stockBottleDiscountPercent);
			}
		}

		public virtual Email GetEmailAddressForBill()
		{
			return Client.Emails.FirstOrDefault(x => (x.EmailType?.EmailPurpose == EmailPurpose.ForBills) || x.EmailType == null);
		}

		public virtual bool NeedSendBill(IEmailRepository emailRepository)
		{
			var notSendedByEdo = _orderRepository.GetEdoContainersByOrderId(UoW, Id).Count(x=>x.Type == Type.Bill) == 0;
			var notSendedByEmail = !emailRepository.HaveSendedEmailForBill(Id);
			var notSended = notSendedByEdo && notSendedByEmail;
			if((OrderStatus == OrderStatus.NewOrder || OrderStatus == OrderStatus.Accepted || OrderStatus == OrderStatus.WaitForPayment)
			   && PaymentType == PaymentType.Cashless
			   && notSended)
			{
				//Проверка должен ли формироваться счет для текущего заказа
				var requirementDocTypes = GetRequirementDocTypes();
				return requirementDocTypes.Contains(OrderDocumentType.Bill) || requirementDocTypes.Contains(OrderDocumentType.SpecialBill);
			}
			return false;
		}

		public virtual void ParseTareReason()
		{
			if(!IsTareNonReturnReasonChangedByUser) {
				var reasons = UoW.Session.QueryOver<NonReturnReason>().OrderBy(x => x.Name).Asc.List();
				TareNonReturnReason = reasons.FirstOrDefault(x => x.Name.ToUpper() == "ПРИЧИНА НЕИЗВЕСТНА");
				if(!string.IsNullOrWhiteSpace(Comment)) {
					if(Comment.ToUpper().Contains("НОВЫЙ АДРЕС") && reasons.Any(x => x.Name.ToUpper() == "НОВЫЙ АДРЕС"))
						TareNonReturnReason = reasons.FirstOrDefault(x => x.Name.ToUpper() == "НОВЫЙ АДРЕС");
					if(Comment.ToUpper().Contains("УВЕЛИЧЕНИЕ ЗАКАЗА") && reasons.Any(x => x.Name.ToUpper() == "УВЕЛИЧЕНИЕ ЗАКАЗА"))
						TareNonReturnReason = reasons.FirstOrDefault(x => x.Name.ToUpper() == "УВЕЛИЧЕНИЕ ЗАКАЗА");
					if(Comment.ToUpper().Contains("ПЕРВЫЙ ЗАКАЗ") && reasons.Any(x => x.Name.ToUpper() == "ПЕРВЫЙ ЗАКАЗ"))
						TareNonReturnReason = reasons.FirstOrDefault(x => x.Name.ToUpper() == "ПЕРВЫЙ ЗАКАЗ");
				}
			}
		}

		public virtual void RecalculateStockBottles(IOrderParametersProvider orderParametersProvider)
		{
			if(!IsBottleStock) {
				BottlesByStockCount = 0;
				BottlesByStockActualCount = 0;
			}
			CalculateBottlesStockDiscounts(orderParametersProvider);
		}

		public virtual void AddContractDocument(CounterpartyContract contract)
		{
			if(ObservableOrderDocuments.OfType<OrderContract>().Any(x => x.Contract == contract)) {
				return;
			}
			ObservableOrderDocuments.Add(
				new OrderContract {
					Order = this,
					AttachedToOrder = this,
					Contract = contract
				}
			);
		}

		public virtual bool HasWater()
		{
			var categories = Nomenclature.GetCategoriesRequirementForWaterAgreement();
			return ObservableOrderItems.Any(x => categories.Contains(x.Nomenclature.Category));
		}

		public virtual void CheckAndSetOrderIsService()
		{
			if(OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master))
			{
				OrderAddressType = OrderAddressType.Service;
			}
		}

		public virtual void SetFirstOrder()
		{
			if(Id == 0 && Client.FirstOrder == null) {
				IsFirstOrder = true;
				Client.FirstOrder = this;
			}
		}

		public virtual void UpdateOrCreateContract(
			IUnitOfWork uow,
			ICounterpartyContractRepository contractRepository,
			CounterpartyContractFactory contractFactory,
			Organization organization = null)
		{
			if(!NHibernateUtil.IsInitialized(Client))
			{
				NHibernateUtil.Initialize(Client);
			}
			if(!NHibernateUtil.IsInitialized(Contract))
			{
				NHibernateUtil.Initialize(Contract);
			}

			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}
			if(contractRepository == null)
			{
				throw new ArgumentNullException(nameof(contractRepository));
			}
			if(contractFactory == null)
			{
				throw new ArgumentNullException(nameof(contractFactory));
			}

			if(Client == null)
			{
				return;
			}

			var counterpartyContract = organization != null
				? contractRepository.GetCounterpartyContractByOrganization(uow, this, organization)
				: contractRepository.GetCounterpartyContract(uow, this, ErrorReporter.Instance);
			
			if(counterpartyContract == null)
			{
				counterpartyContract = contractFactory.CreateContract(uow, this, DeliveryDate, organization);
			}
			else
			{
				if(DeliveryDate.HasValue && DeliveryDate.Value < counterpartyContract.IssueDate)
				{
					counterpartyContract.IssueDate = DeliveryDate.Value;
				}
			}

			Contract = counterpartyContract;

			foreach(var orderItem in OrderItems)
			{
				orderItem.CalculateVATType();
			}
			UpdateContractDocument();
			UpdateDocuments();
		}

		public virtual void RecalculateItemsPrice()
		{
			for(var i = 0; i < OrderItems.Count; i++)
			{
				if(OrderItems[i].Nomenclature.Category == NomenclatureCategory.water)
				{
					OrderItems[i].RecalculatePrice();
				}
			}
		}

		public virtual int GetTotalWater19LCount(bool doNotCountWaterFromPromoSets = false)
		{
			var water19L = ObservableOrderItems.Where(x => x.Nomenclature.IsWater19L);
			if(doNotCountWaterFromPromoSets)
				water19L = water19L.Where(x => x.PromoSet == null);
			return (int)water19L.Sum(x => x.Count);
		}

		public virtual void AddEquipmentNomenclatureToClient(Nomenclature nomenclature, IUnitOfWork UoW)
		{
			ObservableOrderEquipments.Add(
				new OrderEquipment {
					Order = this,
					Direction = Direction.Deliver,
					Equipment = null,
					OrderItem = null,
					Reason = Reason.Service,
					Confirmed = true,
					Nomenclature = nomenclature
				}
			);
			UpdateDocuments();
		}

		public virtual void AddEquipmentNomenclatureFromClient(
			Nomenclature nomenclature,
			IUnitOfWork UoW,
			int count = 0,
			Direction direction = Direction.PickUp,
			DirectionReason directionReason = DirectionReason.None,
			OwnTypes ownType = OwnTypes.None,
			Reason reason = Reason.Service)
		{
			ObservableOrderEquipments.Add(
				new OrderEquipment {
					Order = this,
					Direction = direction,
					Equipment = null,
					OrderItem = null,
					OwnType = ownType,
					DirectionReason = directionReason,
					Reason = reason,
					Confirmed = true,
					Nomenclature = nomenclature,
					Count = count
				}
			);
			UpdateDocuments();
		}

		public virtual void AddAnyGoodsNomenclatureForSale(Nomenclature nomenclature, bool isChangeOrder = false, int? cnt = null)
		{
			var acceptableCategories = Nomenclature.GetCategoriesForSale();
			if(!acceptableCategories.Contains(nomenclature.Category)) {
				return;
			}

			var count = (nomenclature.Category == NomenclatureCategory.service
						 || nomenclature.Category == NomenclatureCategory.deposit) && !isChangeOrder ? 1 : 0;

			if(cnt.HasValue)
				count = cnt.Value;

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
			                               && nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			var newItem = new OrderItem {
				Order = this,
				Count = count,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = nomenclature.GetPrice(1, canApplyAlternativePrice)
			};
			AddOrderItem(newItem);
		}

		public virtual void AddItemWithNomenclatureForSale(OrderItem orderItem)
		{
			var acceptableCategories = Nomenclature.GetCategoriesForSale();
			if(orderItem?.Nomenclature == null || !acceptableCategories.Contains(orderItem.Nomenclature.Category))
				return;

			orderItem.Order = this;
			AddOrderItem(orderItem);
		}

		/// <summary>
		/// Добавление в заказ номенклатуры типа "Выезд мастера"
		/// </summary>
		/// <param name="nomenclature">Номенклатура типа "Выезд мастера"</param>
		/// <param name="count">Количество</param>
		/// <param name="quantityOfFollowingNomenclatures">Колличество номенклатуры, указанной в параметрах БД,
		/// которые будут добавлены в заказ вместе с мастером</param>
		public virtual void AddMasterNomenclature(Nomenclature nomenclature, int count, int quantityOfFollowingNomenclatures = 0)
		{
			if(nomenclature.Category != NomenclatureCategory.master) {
				return;
			}

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
			                               && nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			var newItem = new OrderItem {
				Order = this,
				Count = count,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = nomenclature.GetPrice(1, canApplyAlternativePrice)
			};
			AddOrderItem(newItem);

			if(quantityOfFollowingNomenclatures > 0)
			{
				Nomenclature followingNomenclature = _nomenclatureRepository.GetNomenclatureToAddWithMaster(UoW);
				if(!ObservableOrderItems.Any(i => i.Nomenclature.Id == followingNomenclature.Id))
				{
					AddAnyGoodsNomenclatureForSale(followingNomenclature, false, 1);
				}
			}
		}

		public virtual void AddWaterForSale(Nomenclature nomenclature, decimal count, decimal discount = 0, bool discountInMoney = false, DiscountReason reason = null, PromotionalSet proSet = null)
		{
			if(nomenclature.Category != NomenclatureCategory.water && !nomenclature.IsDisposableTare)
				return;

			//Если номенклатура промо-набора добавляется по фиксе (без скидки), то у нового OrderItem убирается поле discountReason
			if(proSet != null && discount == 0) {
				var fixPricedNomenclaturesId = GetNomenclaturesWithFixPrices.Select(n => n.Id);
				if(fixPricedNomenclaturesId.Contains(nomenclature.Id))
					reason = null;
			}

			if(discount > 0 && reason == null && proSet == null)
			{
				throw new ArgumentException("Требуется указать причину скидки (reason), если она (discount) больше 0!");
			}

			decimal price = GetWaterPrice(nomenclature, proSet, count);

			var oi = new OrderItem {
				Order = this,
				Count = count,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = price,
				IsDiscountInMoney = discountInMoney,
				DiscountSetter = discount,
				DiscountReason = reason,
				PromoSet = proSet
			};
			AddOrderItem(oi);
		}

		public virtual void AddFlyerNomenclature(Nomenclature flyerNomenclature)
		{
			if (ObservableOrderEquipments.Any(x => x.Nomenclature.Id == flyerNomenclature.Id)) {
				return;
			}

			ObservableOrderEquipments.Add(
				new OrderEquipment {
					Order = this,
					Direction = Direction.Deliver,
					Count = 1,
					Equipment = null,
					OrderItem = null,
					Reason = Reason.Sale,
					Confirmed = true,
					Nomenclature = flyerNomenclature
				}
			);
			UpdateDocuments();
		}

		private decimal GetWaterPrice(Nomenclature nomenclature, PromotionalSet promoSet, decimal bottlesCount)
		{
			var fixedPrice = GetFixedPriceOrNull(nomenclature, GetTotalWater19LCount() + bottlesCount);
			if (fixedPrice != null && promoSet == null) {
				return fixedPrice.Price;
			}

			var count = promoSet == null ? GetTotalWater19LCount(true) : bottlesCount;

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
											&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			return nomenclature.GetPrice(count, canApplyAlternativePrice);
		}

		public virtual NomenclatureFixedPrice GetFixedPriceOrNull(Nomenclature nomenclature, decimal bottlesCount)
		{
			IList<NomenclatureFixedPrice> fixedPrices;

			if(deliveryPoint == null)
			{
				if (Contract == null)
					return null;

				fixedPrices = Contract.Counterparty.NomenclatureFixedPrices;
			}
			else
			{
				fixedPrices = deliveryPoint.NomenclatureFixedPrices;
			}

			Nomenclature influentialNomenclature = nomenclature.DependsOnNomenclature;

			if(fixedPrices.Any(x => x.Nomenclature.Id == nomenclature.Id && bottlesCount >= x.MinCount && influentialNomenclature == null)) 
			{
				return fixedPrices.OrderBy(x=>x.MinCount).Last(x => x.Nomenclature.Id == nomenclature.Id && bottlesCount >= x.MinCount);
			}

			if(influentialNomenclature != null && fixedPrices.Any(x => x.Nomenclature.Id == influentialNomenclature.Id && bottlesCount >= x.MinCount)) 
			{
				return fixedPrices.OrderBy(x => x.MinCount).Last(x => x.Nomenclature.Id == influentialNomenclature?.Id && bottlesCount >= x.MinCount);
			}

			return null;
		}

		public virtual IEnumerable<Nomenclature> GetNomenclaturesWithFixPrices{
			get {
				List<NomenclatureFixedPrice> fixedPrices = new List<NomenclatureFixedPrice>();
				fixedPrices.AddRange(Client.NomenclatureFixedPrices);
				fixedPrices.AddRange(Client.DeliveryPoints.SelectMany(x => x.NomenclatureFixedPrices));
				return fixedPrices.Select(x => x.Nomenclature).Distinct();
			}
		}

		public virtual void UpdateClientDefaultParam(
			IUnitOfWork uow,
			ICounterpartyContractRepository counterpartyContractRepository,
			IOrganizationProvider organizationProvider,
			CounterpartyContractFactory counterpartyContractFactory)
		{
			if(counterpartyContractRepository == null)
				throw new ArgumentNullException(nameof(counterpartyContractRepository));
			if(organizationProvider == null)
				throw new ArgumentNullException(nameof(organizationProvider));
			if(counterpartyContractFactory == null)
				throw new ArgumentNullException(nameof(counterpartyContractFactory));

			if(Client == null)
				return;
			if(OrderStatus != OrderStatus.NewOrder)
				return;

			DeliveryDate = null;
			DeliveryPoint = null;
			DeliverySchedule = null;
			Contract = null;
			DocumentType = Client.DefaultDocumentType ?? DefaultDocumentType.upd;

			if(!SelfDelivery && Client.DeliveryPoints?.Count == 1)
				DeliveryPoint = Client.DeliveryPoints.FirstOrDefault();

			PaymentType = Client.PaymentMethod;
		}

		public virtual void SetProxyForOrder()
		{
			if(Client == null)
				return;
			if(!DeliveryDate.HasValue)
				return;
			if(Client.PersonType != PersonType.legal && PaymentType != PaymentType.Cashless)
				return;

			bool existProxies = Client.Proxies
									  .Any(
										p => p.IsActiveProxy(DeliveryDate.Value)
										&& (
												p.DeliveryPoints == null || p.DeliveryPoints
																			 .Any(x => DomainHelper.EqualDomainObjects(x, DeliveryPoint))
										   )
									  );

			if(existProxies)
				SignatureType = OrderSignatureType.ByProxy;
		}

		public virtual bool CalculateDeliveryPrice()
		{
			OrderItem deliveryPriceItem = OrderItems.FirstOrDefault(x => x.Nomenclature.Id == PaidDeliveryNomenclatureId);

			#region перенести всё это в OrderStateKey
			bool IsDeliveryForFree = SelfDelivery
												  || OrderAddressType == OrderAddressType.Service
												  || DeliveryPoint.AlwaysFreeDelivery
												  || ObservableOrderItems.Any(n => n.Nomenclature.Category == NomenclatureCategory.spare_parts)
												  || !ObservableOrderItems.Any(n => n.Nomenclature.Id != PaidDeliveryNomenclatureId) && (BottlesReturn > 0 || ObservableOrderEquipments.Any() || ObservableOrderDepositItems.Any());

			if(IsDeliveryForFree) {
				if(deliveryPriceItem != null)
					RemoveOrderItem(deliveryPriceItem);
				return false;
			}
			#endregion

			var district = DeliveryPoint?.District;

			OrderStateKey orderKey = new OrderStateKey(this);
			var price =
				district?.GetDeliveryPrice(orderKey, ObservableOrderItems.Sum(x => x.Nomenclature?.OnlineStoreExternalId != null ? x.ActualSum : 0m )) ?? 0m;

			if(price != 0) {
				if(deliveryPriceItem == null) {
					deliveryPriceItem = new OrderItem {
						Nomenclature = UoW.GetById<Nomenclature>(PaidDeliveryNomenclatureId),
						Order = this
					};
					deliveryPriceItem.Price = price;
					deliveryPriceItem.Count = 1;

					var delivery = ObservableOrderItems.SingleOrDefault(x => x.Nomenclature.Id == PaidDeliveryNomenclatureId);

					if (delivery == null) {
						AddOrderItem(deliveryPriceItem);
						return true;
					}
					else
						return false;

				} else if(deliveryPriceItem.Price == price) {
					return false;
				}
				deliveryPriceItem.Price = price;
				deliveryPriceItem.Count = 1;
				return true;
			}

			if(deliveryPriceItem != null)
				RemoveOrderItem(deliveryPriceItem);
			return false;
		}

		/// <summary>
		/// Добавить оборудование из выбранного предыдущего заказа.
		/// </summary>
		/// <param name="orderItem">Элемент заказа.</param>
		/// <param name="UoW">IUnitOfWork</param>
		public virtual void AddNomenclatureForSaleFromPreviousOrder(OrderItem orderItem, IUnitOfWork UoW)
		{
			if(orderItem.Nomenclature.Category != NomenclatureCategory.additional)
				return;

			var newItem = new OrderItem {
				Order = this,
				Count = orderItem.Count,
				Nomenclature = orderItem.Nomenclature,
				Price = orderItem.Price
			};
			AddOrderItem(newItem);
		}

		/// <summary>
		/// Добавить номенклатуру (не вода и не оборудование из выбранного предыдущего заказа).
		/// </summary>
		/// <param name="orderItem">Элемент заказа.</param>
		public virtual void AddAnyGoodsNomenclatureForSaleFromPreviousOrder(OrderItem orderItem)
		{
			if(orderItem.Nomenclature.Category != NomenclatureCategory.additional && orderItem.Nomenclature.Category != NomenclatureCategory.bottle &&
				orderItem.Nomenclature.Category != NomenclatureCategory.service)
				return;
			var newItem = new OrderItem {
				Order = this,
				Count = orderItem.Nomenclature.Category == NomenclatureCategory.service ? 1 : 0,
				Equipment = orderItem.Equipment,
				Nomenclature = orderItem.Nomenclature,
				Price = orderItem.Price
			};
			AddOrderItem(newItem);
		}

		public virtual void AddNomenclature(
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			bool discountInMoney = false,
			DiscountReason discountReason = null,
			PromotionalSet proSet = null)
		{
			OrderItem oi = null;
			switch(nomenclature.Category) {
				case NomenclatureCategory.water:
					AddWaterForSale(nomenclature, count, discount, discountInMoney, discountReason, proSet);
					break;
				case NomenclatureCategory.master:
					contract = CreateServiceContractAddMasterNomenclature(nomenclature);
					break;
				default:
					var canApplyAlternativePrice = HasPermissionsForAlternativePrice && nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);
					oi = new OrderItem {
						Order = this,
						Count = count,
						Nomenclature = nomenclature,
						Price = nomenclature.GetPrice(1, canApplyAlternativePrice),
						IsDiscountInMoney = discountInMoney,
						DiscountSetter = discount,
						DiscountReason = discountReason,
						PromoSet = proSet
					};
					AddItemWithNomenclatureForSale(oi);
					break;
			}
		}

		/// <summary>
		/// Попытка найти и удалить промо-набор, если нет больше позиций
		/// заказа с промо-набором
		/// </summary>
		/// <param name="orderItem">Позиция заказа</param>
		public virtual void TryToRemovePromotionalSet(OrderItem orderItem)
		{
			var proSetFromOrderItem = orderItem.PromoSet;
			if(proSetFromOrderItem != null) {
				var proSetToRemove = ObservablePromotionalSets.FirstOrDefault(s => s == proSetFromOrderItem);
				if(proSetToRemove != null && !OrderItems.Any(i => i.PromoSet == proSetToRemove)) {
					foreach(PromotionalSetActionBase action in proSetToRemove.ObservablePromotionalSetActions) {
						action.Deactivate(this);
					}
					ObservablePromotionalSets.Remove(proSetToRemove);
				}
			}
		}

		private void ObservablePromotionalSets_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			if(aObject is PromotionalSet proSet) {
				foreach(OrderItem item in ObservableOrderItems)
					if(item.PromoSet == proSet) {
						item.IsUserPrice = false;
						item.PromoSet = null;
						item.DiscountReason = null;
					}
				RecalculateItemsPrice();
			}
		}

		/// <summary>
		/// Чистка списка промо-наборов заказа, если вручную удалили, изменили
		/// причину скидки или что-то ещё.
		/// </summary>
		private void ClearPromotionSets()
		{
			var oigrp = OrderItems.GroupBy(x => x.PromoSet);
			var rem = PromotionalSets.Where(s => !oigrp.Select(g => g.Key).Contains(s)).ToArray();
			foreach(var r in rem) {
				var ps = PromotionalSets.FirstOrDefault(s => s == r);
				PromotionalSets.Remove(ps);
			}
		}

		/// <summary>
		/// Проверка на возможность добавления промо-набора в заказ
		/// </summary>
		/// <returns><c>true</c>, если можно добавить промо-набор,
		/// <c>false</c> если нельзя.</returns>
		/// <param name="proSet">Рекламный набор (промо-набор)</param>
		public virtual bool CanAddPromotionalSet(PromotionalSet proSet, IPromotionalSetRepository promotionalSetRepository)
		{
			if(PromotionalSets.Any(x => x.Id == proSet.Id)) {
				InteractiveService.ShowMessage(ImportanceLevel.Warning, "В заказ нельзя добавить два одинаковых промо-набора");
				return false;
			}
			if((PromotionalSets.Count(x => !x.CanBeAddedWithOtherPromoSets) + (proSet.CanBeAddedWithOtherPromoSets ? 0 : 1)) > 1) {
				InteractiveService.ShowMessage(ImportanceLevel.Warning, "В заказ нельзя добавить больше 1 промо-набора, у которого нет свойства \"Может быть добавлен вместе с другими промонаборами\"");
				return false;
			}

			if(SelfDelivery)
			{
				return true;
			}

			if(!proSet.CanBeReorderedWithoutRestriction && CanUsedPromo(promotionalSetRepository))
			{
				string message = "По этому адресу уже была ранее отгрузка промонабора на другое физ.лицо.";
				InteractiveService.ShowMessage(ImportanceLevel.Warning, message);
				return false;
			}

			var proSetDict = promotionalSetRepository.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(UoW, this);
			
			if(proSet.CanBeReorderedWithoutRestriction | !proSetDict.Any())
			{
				return true;
			}

			var address = string.Join(", ", DeliveryPoint.City, DeliveryPoint.Street, DeliveryPoint.Building, DeliveryPoint.Room);
			StringBuilder sb = new StringBuilder(string.Format("Для адреса \"{0}\", найдены схожие точки доставки, на которые уже создавались заказы с промо-наборами:\n", address));
			foreach(var d in proSetDict) {
				var proSetTitle = UoW.GetById<PromotionalSet>(d.Key).ShortTitle;
				var orders = string.Join(
					" ,",
					UoW.GetById<Order>(d.Value).Select(o => o.Title)
				);
				sb.AppendLine(string.Format("– {0}: {1}", proSetTitle, orders));
			}
			sb.AppendLine($"Вы уверены, что хотите добавить \"{proSet.Title}\"");
			if(InteractiveService.Question(sb.ToString()))
				return true;
			return false;
		}

		/// <summary>
		/// Проверка на использование промо-набора в заказе на адрес
		/// </summary>
		/// <returns><c>true</c>, если на адрес не доставляли промо-набор,
		/// <c>false</c> если нельзя.</returns>
		public virtual bool CanUsedPromo(IPromotionalSetRepository promotionalSetRepository)
		{
			return !SelfDelivery
				&& Client.PersonType == PersonType.natural
				&& ((DeliveryPoint.RoomType == RoomType.Office) || (DeliveryPoint.RoomType == RoomType.Store))
				&& promotionalSetRepository.AddressHasAlreadyBeenUsedForPromo(UoW, deliveryPoint);
		}

		private CounterpartyContract CreateServiceContractAddMasterNomenclature(Nomenclature nomenclature)
		{
			if(Contract == null)
			{
				ForceUpdateContract();
			}
			AddMasterNomenclature(nomenclature, 1);
			return Contract;
		}

		public virtual void ClearOrderItemsList()
		{
			ObservableOrderItems.Clear();
			UpdateDocuments();
		}

		/// <summary>
		/// Удаляет дополнительные документы выделенные пользователем, которые не относятся к текущему заказу.
		/// </summary>
		/// <returns>Документы текущего заказа, которые не были удалены.</returns>
		/// <param name="documents">Список документов для удаления.</param>
		public virtual List<OrderDocument> RemoveAdditionalDocuments(OrderDocument[] documents)
		{
			if(documents == null || !documents.Any())
				return null;

			List<OrderDocument> thisOrderDocuments = new List<OrderDocument>();
			foreach(OrderDocument doc in documents) {
				if(doc.Order != this)
					ObservableOrderDocuments.Remove(doc);
				else
					thisOrderDocuments.Add(doc);
			}

			return thisOrderDocuments;
		}

		/// <summary>
		/// Добавляет дополнительные документы выбранные пользователем в диалоге,
		/// с проверкой их наличия в текущем заказе
		/// </summary>
		public virtual void AddAdditionalDocuments(IEnumerable<OrderDocument> documents)
		{
			foreach(var item in documents) {
				switch(item.Type) {
					case OrderDocumentType.Contract:
						OrderContract oc = (item as OrderContract);
						if(ObservableOrderDocuments
						   .OfType<OrderContract>()
						   .FirstOrDefault(x => x.Contract == oc.Contract
										   && x.Order == oc.Order)
						   == null) {
							ObservableOrderDocuments.Add(new OrderContract {
								Order = item.Order,
								AttachedToOrder = this,
								Contract = oc.Contract
							});
						}
						break;
					case OrderDocumentType.M2Proxy:
						OrderM2Proxy m2 = item as OrderM2Proxy;
						var hasDocument = ObservableOrderDocuments
						   .OfType<OrderM2Proxy>()
						   .Any(x => x.M2Proxy == m2.M2Proxy && x.Order == m2.Order);

						if(!hasDocument)
						{
							var newM2 = new OrderM2Proxy();
							newM2.AttachedToOrder = this;
							newM2.Order = m2.Order;
							newM2.M2Proxy = m2.M2Proxy;

							ObservableOrderDocuments.Add(newM2);
						}
						break;
					case OrderDocumentType.Bill:
						if(ObservableOrderDocuments
						   .OfType<BillDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new BillDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.DoneWorkReport:
						if(ObservableOrderDocuments
						   .OfType<DoneWorkDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new DoneWorkDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.EquipmentTransfer:
						if(ObservableOrderDocuments
						   .OfType<EquipmentTransferDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new EquipmentTransferDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.Invoice:
						if(ObservableOrderDocuments
						   .OfType<InvoiceDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new InvoiceDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.InvoiceBarter:
						if(ObservableOrderDocuments
						   .OfType<InvoiceBarterDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new InvoiceBarterDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.InvoiceContractDoc:
						if(ObservableOrderDocuments
						   .OfType<InvoiceContractDoc>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new InvoiceContractDoc {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.UPD:
						if(ObservableOrderDocuments
						   .OfType<UPDDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new UPDDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.DriverTicket:
						if(ObservableOrderDocuments
						   .OfType<DriverTicketDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new DriverTicketDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.Torg12:
						if(ObservableOrderDocuments
						   .OfType<Torg12Document>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new Torg12Document {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.ShetFactura:
						if(ObservableOrderDocuments
						   .OfType<ShetFacturaDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new ShetFacturaDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.ProductCertificate:
						if(item is NomenclatureCertificateDocument cert &&
							!ObservableOrderDocuments.OfType<NomenclatureCertificateDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new NomenclatureCertificateDocument {
									Order = item.Order,
									AttachedToOrder = this,
									Certificate = cert.Certificate
								}
							);
						}
						break;
					case OrderDocumentType.SpecialBill:
						if(item is SpecialBillDocument &&
							!ObservableOrderDocuments.OfType<SpecialBillDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new SpecialBillDocument {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					case OrderDocumentType.EquipmentReturn:
						if(item is EquipmentReturnDocument &&
							!ObservableOrderDocuments.OfType<EquipmentReturnDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new EquipmentReturnDocument {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					case OrderDocumentType.SpecialUPD:
						if(item is SpecialUPDDocument &&
							!ObservableOrderDocuments.OfType<SpecialUPDDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new SpecialUPDDocument {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					case OrderDocumentType.TransportInvoice:
						if(item is TransportInvoiceDocument &&
							!ObservableOrderDocuments.OfType<TransportInvoiceDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new TransportInvoiceDocument {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					case OrderDocumentType.Torg2:
						if(item is Torg2Document &&
							!ObservableOrderDocuments.OfType<Torg2Document>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new Torg2Document {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					case OrderDocumentType.AssemblyList:
						if(item is AssemblyListDocument &&
							!ObservableOrderDocuments.OfType<AssemblyListDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new AssemblyListDocument {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					default:
						break;
				}
			}
		}

		/// <summary>
		/// Ожидаемое количество залогов за бутыли
		/// </summary>
		public virtual int GetExpectedBottlesDepositsCount()
		{
			if(Client == null || Client.PersonType == PersonType.legal)
				return 0;

			var waterItemsCount = (int)ObservableOrderItems.Select(item => item)
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water && !item.Nomenclature.IsDisposableTare)
				.Sum(item => item.Count);

			return waterItemsCount - BottlesReturn ?? 0;
		}

		public virtual void RemoveAloneItem(OrderItem item)
		{
			if(item.Count == 0
			   && !OrderEquipments.Any(x => x.OrderItem == item)) {
				RemoveOrderItem(item);
			}
		}

		public virtual void RemoveItem(OrderItem item)
		{
			RemoveOrderItem(item);
			DeleteOrderEquipmentOnOrderItem(item);
			UpdateDocuments();
		}

		public virtual void RemoveEquipment(OrderEquipment item)
		{
			var rentDepositOrderItem = item.OrderRentDepositItem;
			var rentServiceOrderItem = item.OrderRentServiceItem;
			var totalEquipmentCountForDeposit = 0;
			var totalEquipmentCountForService = 0;

			if(rentDepositOrderItem != null)
			{
				totalEquipmentCountForDeposit = GetRentEquipmentTotalCountForDepositItem(rentDepositOrderItem);
			}
			if(rentServiceOrderItem != null)
			{
				totalEquipmentCountForService = GetRentEquipmentTotalCountForServiceItem(rentServiceOrderItem);
			}

			if(totalEquipmentCountForDeposit == item.Count || totalEquipmentCountForService == item.Count)
			{
				ObservableOrderEquipments.Remove(item);
				RemoveOrderItem(rentDepositOrderItem);
				RemoveOrderItem(rentServiceOrderItem);
			}
			else
			{
				ObservableOrderEquipments.Remove(item);
				UpdateRentsCount();
			}

			UpdateDocuments();
		}

		/// <summary>
		/// Удаляет оборудование в заказе связанное с товаром в заказе
		/// </summary>
		/// <param name="orderItem">Товар в заказе по которому будет удалятся оборудование</param>
		private void DeleteOrderEquipmentOnOrderItem(OrderItem orderItem)
		{
			var orderEquipments = ObservableOrderEquipments
				.Where(x => x.OrderItem == orderItem)
				.ToList();
			foreach(var orderEquipment in orderEquipments) {
				ObservableOrderEquipments.Remove(orderEquipment);
			}
		}

		public virtual void RemoveDepositItem(OrderDepositItem item)
		{
			ObservableOrderDepositItems.Remove(item);
			UpdateDocuments();
		}

		public virtual void AddServiceClaimAsInitial(ServiceClaim service)
		{
			if(service.InitialOrder != null && service.InitialOrder.Id == Id) {
				if(service.Equipment == null || ObservableOrderEquipments.FirstOrDefault(eq => eq.Equipment.Id == service.Equipment.Id) == null) {
					ObservableOrderEquipments.Add(new OrderEquipment {
						Order = this,
						Direction = Direction.PickUp,
						Equipment = service.Equipment,
						Nomenclature = service.Equipment == null ? service.Nomenclature : null,
						OrderItem = null,
						Reason = Reason.Service,
						ServiceClaim = service
					});
				}
				if(service.ReplacementEquipment != null) {
					ObservableOrderEquipments.Add(new OrderEquipment {
						Order = this,
						Direction = Direction.Deliver,
						Equipment = service.ReplacementEquipment,
						Nomenclature = null,
						OrderItem = null,
						Reason = Reason.Service
					});
				}
				if(ObservableInitialOrderService.FirstOrDefault(sc => sc.Id == service.Id) == null)
					ObservableInitialOrderService.Add(service);
				if(ObservableOrderDocuments.Where(doc => doc.Type == OrderDocumentType.EquipmentTransfer).Cast<EquipmentTransferDocument>()
					.FirstOrDefault() == null) {
					ObservableOrderDocuments.Add(new EquipmentTransferDocument {
						Order = this,
						AttachedToOrder = this
					});
				}
			}
		}

		public virtual void FillNewEquipment(Equipment registeredEquipment)
		{
			var newEquipment = ObservableOrderEquipments
				.Where(orderEq => orderEq.Nomenclature != null)
				.FirstOrDefault(orderEq => orderEq.Nomenclature.Id == registeredEquipment.Nomenclature.Id);
			if(newEquipment != null) {
				newEquipment.Equipment = registeredEquipment;
				newEquipment.Nomenclature = null;
			}
		}

		/// <summary>
		/// Присвоение текущему заказу статуса недовоза
		/// </summary>
		/// <param name="guilty">Ответственный в недовезении заказа</param>
		public virtual void SetUndeliveredStatus(IUnitOfWork uow, IStandartNomenclatures standartNomenclatures, CallTaskWorker callTaskWorker, GuiltyTypes? guilty = GuiltyTypes.Client)
		{
			var routeListItem = new RouteListItemRepository().GetRouteListItemForOrder(UoW, this);
			var routeList = routeListItem?.RouteList;
			switch(OrderStatus)
			{
				case OrderStatus.NewOrder:
				case OrderStatus.WaitForPayment:
				case OrderStatus.Accepted:
				case OrderStatus.InTravelList:
				case OrderStatus.OnLoading:
					ChangeStatusAndCreateTasks(OrderStatus.Canceled, callTaskWorker);
					routeList?.SetAddressStatusWithoutOrderChange(uow, routeListItem.Id, RouteListItemStatus.Overdue);
					break;
				case OrderStatus.OnTheWay:
				case OrderStatus.DeliveryCanceled:
				case OrderStatus.Shipped:
				case OrderStatus.UnloadingOnStock:
				case OrderStatus.NotDelivered:
				case OrderStatus.Closed:
					if(guilty == GuiltyTypes.Client)
					{
						ChangeStatusAndCreateTasks(OrderStatus.DeliveryCanceled, callTaskWorker);
						routeList?.SetAddressStatusWithoutOrderChange(uow, routeListItem.Id, RouteListItemStatus.Canceled);
					}
					else
					{
						ChangeStatusAndCreateTasks(OrderStatus.NotDelivered, callTaskWorker);
						routeList?.SetAddressStatusWithoutOrderChange(uow, routeListItem.Id, RouteListItemStatus.Overdue);
					}
					break;
			}
			UpdateBottleMovementOperation(uow, standartNomenclatures, 0);
		}

		public virtual void ChangeStatusAndCreateTasks(OrderStatus newStatus, ICallTaskWorker callTaskWorker)
		{
			ChangeStatus(newStatus);
			callTaskWorker.CreateTasks(this);
		}

		public virtual void ChangeStatus(OrderStatus newStatus)
		{
			var initialStatus = OrderStatus;
			OrderStatus = newStatus;
			switch(newStatus) {
				case OrderStatus.NewOrder:
					break;
				case OrderStatus.WaitForPayment:
					OnChangeStatusToWaitingForPayment();
					break;
				case OrderStatus.Accepted:
					//Удаляем операции перемещения тары, если возвращаем
					//из "закрыт" без доставки в "принят"
					if(initialStatus == OrderStatus.Closed)
						DeleteBottlesMovementOperation(UoW);
					OnChangeStatusToAccepted();
					break;
				case OrderStatus.OnLoading:
					OnChangeStatusToOnLoading();
					break;
				case OrderStatus.OnTheWay:
					break;
				case OrderStatus.Shipped:
				case OrderStatus.UnloadingOnStock:
					SendUpdToEmail();
					break;
				case OrderStatus.Closed:
					SendUpdToEmail();
					OnChangeStatusToClosed();
					break;
				case OrderStatus.DeliveryCanceled:
				case OrderStatus.NotDelivered:
				case OrderStatus.Canceled:
					if(PaymentType == PaymentType.Cashless)
					{
						_paymentFromBankClientController.ReturnAllocatedSumToClientBalance(UoW, this);
					}
					break;
				default:
					break;
			}

			if(Id == 0
			   || newStatus == OrderStatus.Canceled
			   || newStatus == OrderStatus.NotDelivered
			   || initialStatus == newStatus)
				return;

			_paymentFromBankClientController.CancelRefundedPaymentIfOrderRevertFromUndelivery(UoW, this, initialStatus);

			var undeliveries = _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, this);
			if(undeliveries.Any()) {
				var text = string.Format(
					"сменил(а) статус заказа\nс \"{0}\" на \"{1}\"",
					initialStatus.GetEnumTitle(),
					newStatus.GetEnumTitle()
				);
				foreach(var u in undeliveries) {
					u.AddCommentToTheField(UoW, CommentedFields.Reason, text);
				}
			}
		}

		public virtual void UpdateOrderPaymentStatus(decimal canceledSum)
		{
			var allocatedSum = _paymentItemsRepository.GetAllocatedSumForOrder(UoW, Id);

			if(allocatedSum == 0 || allocatedSum - canceledSum == 0)
			{
				OrderPaymentStatus = OrderPaymentStatus.UnPaid;
			}
			else if(allocatedSum - canceledSum > OrderSum)
			{
				OrderPaymentStatus = OrderPaymentStatus.Paid;
			}
			else
			{
				OrderPaymentStatus = OrderPaymentStatus.PartiallyPaid;
			}
		}

		/// <summary>
		/// Действия при закрытии заказа
		/// </summary>
		private void OnChangeStatusToOnLoading() => UpdateDocuments();

		/// <summary>
		/// Действия при закрытии заказа
		/// </summary>
		private void OnChangeStatusToClosed()
		{
			SetDepositsActualCounts();
			if(SelfDelivery) {
				UpdateDepositOperations(UoW);
				SetActualCountToSelfDelivery();
			}
		}

		/// <summary>
		/// Действия при переводе заказа в ожидание оплаты
		/// </summary>
		private void OnChangeStatusToWaitingForPayment() => UpdateDocuments();

		/// <summary>
		/// Действия при подтверждении заказа
		/// </summary>
		private void OnChangeStatusToAccepted() => UpdateDocuments();

		/// <summary>
		/// Отправка самовывоза на погрузку
		/// </summary>
		public virtual void SelfDeliveryToLoading(
			Employee employee,
			ICurrentPermissionService permissionService,
			ICallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery) {
				return;
			}
			if(OrderStatus == OrderStatus.Accepted && permissionService.ValidatePresetPermission("allow_load_selfdelivery")) {
				ChangeStatusAndCreateTasks(OrderStatus.OnLoading, callTaskWorker);
				LoadAllowedBy = employee;
			}
		}

		public virtual void SendUpdToEmail()
		{
			if(!_emailRepository.NeedSendUpdByEmail(Id) || _emailRepository.HasSendedEmailForUpd(Id))
			{
				return;
			}

			var document = OrderDocuments.FirstOrDefault(x => x.Type == OrderDocumentType.UPD || x.Type == OrderDocumentType.SpecialUPD);

			if(document == null)
			{
				return;
			}

			var emailAddress = GetEmailAddressForBill();

			if(emailAddress == null)
			{
				return;
			}

			var storedEmail = new StoredEmail
			{
				SendDate = DateTime.Now,
				StateChangeDate = DateTime.Now,
				State = StoredEmailStates.PreparingToSend,
				RecipientAddress = emailAddress.Address,
				ManualSending = false,
				Subject = document.Name,
				Author = Author
			};

			UoW.Save(storedEmail);

			var updDocumentEmail = new UpdDocumentEmail
			{
				StoredEmail = storedEmail,
				Counterparty = Client,
				OrderDocument = document
			};

			UoW.Save(updDocumentEmail);
		}

		public virtual void SetActualCountToSelfDelivery()
		{
			if(!SelfDelivery || OrderStatus != OrderStatus.Closed)
				return;

			foreach(var item in OrderItems)
				item.ActualCount = item.Count;

			foreach(var depositItem in OrderDepositItems)
				depositItem.ActualCount = depositItem.Count;
		}

		/// <summary>
		/// Принятие оплаты самовывоза по безналичному расчету
		/// </summary>
		public virtual void SelfDeliveryAcceptCashlessPaid(CallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery)
				return;
			if(PaymentType != PaymentType.Cashless && PaymentType != PaymentType.PaidOnline)
				return;
			if(OrderStatus != OrderStatus.WaitForPayment)
				return;
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("accept_cashless_paid_selfdelivery"))
				return;

			ChangeStatusAndCreateTasks(PayAfterShipment ? OrderStatus.Closed : OrderStatus.Accepted, callTaskWorker);
		}

		/// <summary>
		/// Принятие оплаты самовывоза по наличному расчету.
		/// Проверяется соответствие суммы заказа с суммой оплаченной в кассе.
		/// Если проверка пройдена заказ закрывается или переводится на погрузку.
		/// </summary>
		public virtual void SelfDeliveryAcceptCashPaid(ICallTaskWorker callTaskWorker)
		{
			decimal totalCashPaid = new CashRepository().GetIncomePaidSumForOrder(UoW, Id);
			decimal totalCashReturn = new CashRepository().GetExpenseReturnSumForOrder(UoW, Id);
			SelfDeliveryAcceptCashPaid(totalCashPaid, totalCashReturn, callTaskWorker);
		}

		public virtual void AcceptSelfDeliveryIncomeCash(decimal incomeCash, ICallTaskWorker callTaskWorker, int? incomeExcludedDoc = null)
		{
			decimal totalCashPaid = new CashRepository().GetIncomePaidSumForOrder(UoW, Id, incomeExcludedDoc) + incomeCash;
			decimal totalCashReturn = new CashRepository().GetExpenseReturnSumForOrder(UoW, Id);
			SelfDeliveryAcceptCashPaid(totalCashPaid, totalCashReturn, callTaskWorker);
		}

		public virtual void AcceptSelfDeliveryExpenseCash(decimal expenseCash, ICallTaskWorker callTaskWorker, int? expenseExcludedDoc = null)
		{
			decimal totalCashPaid = new CashRepository().GetIncomePaidSumForOrder(UoW, Id);
			decimal totalCashReturn = new CashRepository().GetExpenseReturnSumForOrder(UoW, Id, expenseExcludedDoc) + expenseCash;
			SelfDeliveryAcceptCashPaid(totalCashPaid, totalCashReturn, callTaskWorker);
		}

		/// <summary>
		/// Принятие оплаты самовывоза по наличному расчету. С указанием дополнительным сумм по приходным и расходным ордерам
		/// Проверяется соответствие суммы заказа с суммой оплаченной в кассе.
		/// Если проверка пройдена заказ закрывается или переводится на погрузку.
		/// <paramref name="expenseCash">Сумма по открытому расходному ордеру, добавляемая к ранее сохранным расходным ордерам</paramref>
		/// <paramref name="incomeCash">Сумма по открытому приходному ордеру, добавляемая к ранее сохранным приходным ордерам</paramref>
		/// </summary>
		private void SelfDeliveryAcceptCashPaid(decimal incomeCash, decimal expenseCash, ICallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery)
				return;
			if(PaymentType != PaymentType.Cash)
				return;
			if((incomeCash - expenseCash) != OrderCashSum)
				return;

			IsSelfDeliveryPaid = true;

			bool isFullyLoad = IsFullyShippedSelfDeliveryOrder(UoW, new SelfDeliveryRepository());

			if(OrderStatus == OrderStatus.WaitForPayment) {
				if(isFullyLoad) {
					ChangeStatusAndCreateTasks(OrderStatus.Closed, callTaskWorker);
					UpdateBottlesMovementOperationWithoutDelivery(
						UoW, new BaseParametersProvider(new ParametersProvider()), new RouteListItemRepository(), new CashRepository(), incomeCash, expenseCash);
				} else
					ChangeStatusAndCreateTasks(OrderStatus.OnLoading, callTaskWorker);

				return;
			}

			if(OrderStatus == OrderStatus.OnLoading && isFullyLoad)
				ChangeStatusAndCreateTasks(OrderStatus.Closed, callTaskWorker);
		}

		/// <summary>
		/// Проверяет полностью ли оплачен самовывоз и возвращены все деньги
		/// </summary>
		public virtual bool SelfDeliveryIsFullyPaid(ICashRepository cashRepository, decimal incomeCash = 0, decimal expenseCash = 0)
		{
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));

			decimal totalCash = GetSelfDeliveryTotalPayedCash(cashRepository) + incomeCash - expenseCash;

			return OrderCashSum == totalCash;
		}

		/// <summary>
		/// Проверяет полностью ли получены деньги по самовывозу
		/// </summary>
		public virtual bool SelfDeliveryIsFullyIncomePaid()
		{
			decimal totalPaid = new CashRepository().GetIncomePaidSumForOrder(UoW, Id);

			return OrderPositiveSum == totalPaid;
		}

		/// <summary>
		/// Проверяет полностью ли возвращены деньги по самовывозу
		/// </summary>
		public virtual bool SelfDeliveryIsFullyExpenseReturned()
		{
			decimal totalReturned = new CashRepository().GetExpenseReturnSumForOrder(UoW, Id);

			return OrderNegativeSum == totalReturned;
		}

		private decimal GetSelfDeliveryTotalPayedCash(ICashRepository cashRepository)
		{
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));

			decimal totalCashPaid = cashRepository.GetIncomePaidSumForOrder(UoW, Id);
			decimal totalCashReturn = cashRepository.GetExpenseReturnSumForOrder(UoW, Id);

			return totalCashPaid - totalCashReturn;
		}

		/// <summary>
		/// Принятие заказа с самовывозом
		/// </summary>
		private void AcceptSelfDeliveryOrder(ICallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery || OrderStatus != OrderStatus.NewOrder)
				return;

			if(PayAfterShipment || OrderSum == 0)
				ChangeStatusAndCreateTasks(OrderStatus.Accepted, callTaskWorker);
			else
				ChangeStatusAndCreateTasks(OrderStatus.WaitForPayment, callTaskWorker);
		}

		/// <summary>
		/// Устанавливает количество для каждого залога как actualCount,
		/// если заказ был создан только для залога.
		/// Для отображения этих данных в отчете "Акт по бутылям и залогам"
		/// </summary>
		public virtual void SetDepositsActualCounts() //TODO : проверить актуальность метода
		{
			if(OrderItems.All(x => x.Nomenclature.Id == 157))
				foreach(var oi in orderItems)
					oi.ActualCount = oi.Count > 0 ? oi.Count : (oi.ActualCount ?? 0);
		}

		public virtual void AcceptOrder(Employee currentEmployee, ICallTaskWorker callTaskWorker)
		{
			if(SelfDelivery)
				AcceptSelfDeliveryOrder(callTaskWorker);
			else if(CanSetOrderAsAccepted)
				ChangeStatusAndCreateTasks(OrderStatus.Accepted, callTaskWorker);

			AcceptedOrderEmployee = currentEmployee;
		}

		/// <summary>
		/// Статусы в которых возможно редактировать заказ
		/// </summary>
		private OrderStatus[] EditableOrderStatuses {
			get {
				if(SelfDelivery) {
					return new OrderStatus[] {
						OrderStatus.NewOrder
					};
				} else {
					return new OrderStatus[] {
						OrderStatus.NewOrder,
						OrderStatus.WaitForPayment
					};
				}
			}
		}

		public virtual bool CanEditByStatus => EditableOrderStatuses.Contains(OrderStatus);

		/// <summary>
		/// Статусы из которых возможен переход заказа в подтвержденное состояние
		/// </summary>
		public virtual OrderStatus[] SetOrderAsAcceptedStatuses {
			get {
				if(SelfDelivery) {
					return new OrderStatus[] {
						OrderStatus.NewOrder
					};
				} else {
					return new OrderStatus[] {
						OrderStatus.NewOrder,
						OrderStatus.WaitForPayment
					};
				}
			}
		}

		public virtual bool CanSetOrderAsAccepted => EditableOrderStatuses.Contains(OrderStatus);

		public virtual void EditOrder(CallTaskWorker callTaskWorker)
		{
			//Нельзя редактировать заказ с самовывозом
			if(SelfDelivery)
				return;

			if(CanSetOrderAsEditable)
			{
				if(OrderStatus == OrderStatus.Canceled)
				{
					RestoreOrder();
				}

				ChangeStatusAndCreateTasks(OrderStatus.NewOrder, callTaskWorker);
			}
		}

		/// <summary>
		/// Статусы из которых возможен переход заказа в редактируемое состояние
		/// </summary>
		public virtual OrderStatus[] SetOrderAsEditableStatuses {
			get {
				if(SelfDelivery) {
					return new OrderStatus[0];
				} else {
					return new OrderStatus[] {
						OrderStatus.Accepted,
						OrderStatus.Canceled
					};
				}
			}
		}

		public virtual bool CanSetOrderAsEditable => SetOrderAsEditableStatuses.Contains(OrderStatus);

		public virtual bool IsFullyShippedSelfDeliveryOrder(IUnitOfWork uow, ISelfDeliveryRepository selfDeliveryRepository, SelfDeliveryDocument closingDocument = null)
		{
			if(selfDeliveryRepository == null)
				throw new ArgumentNullException(nameof(selfDeliveryRepository));

			if(!SelfDelivery)
				return false;

			var categoriesForShipping = Nomenclature.GetCategoriesForShipment();
			var oItemsGrps = OrderItems.Where(x => categoriesForShipping.Contains(x.Nomenclature.Category))
									   .GroupBy(i => i.Nomenclature.Id, i => i.Count);
			var oEquipmentGrps = OrderEquipments.Where(x => categoriesForShipping.Contains(x.Nomenclature.Category))
												.Where(x => x.Direction == Direction.Deliver)
												.GroupBy(i => i.Nomenclature.Id, i => i.Count);
			var nomGrp = oItemsGrps.ToDictionary(g => g.Key, g => g.Sum());

			foreach(var g in oEquipmentGrps) {
				if(nomGrp.ContainsKey(g.Key))
					nomGrp[g.Key] += g.Sum();
				else
					nomGrp.Add(g.Key, g.Sum());
			}

			// Разрешает закрыть заказ и создать операцию движения бутылей если все товары в заказе отгружены
			bool canCloseOrder = true;
			var unloadedNoms = selfDeliveryRepository.OrderNomenclaturesUnloaded(uow, this, closingDocument);
			foreach(var nGrp in nomGrp) {
				decimal totalCount = default(decimal);
				if(unloadedNoms.ContainsKey(nGrp.Key))
					totalCount += unloadedNoms[nGrp.Key];

				if(totalCount != nGrp.Value)
					canCloseOrder = false;
			}

			return canCloseOrder;
		}

		private void UpdateSelfDeliveryActualCounts(SelfDeliveryDocument notSavedDocument = null)
		{
			var loadedDictionary = new SelfDeliveryRepository().OrderNomenclaturesLoaded(UoW, this);
			if(notSavedDocument != null && notSavedDocument.Id <= 0) {//если id > 0, то такой документ был учтён при получении словаря из репозитория
				foreach(var item in notSavedDocument.Items) {
					if(loadedDictionary.ContainsKey(item.Nomenclature.Id))
						loadedDictionary[item.Nomenclature.Id] += item.Amount;
					else
						loadedDictionary.Add(item.Nomenclature.Id, item.Amount);
				}
			}

			foreach(var item in OrderItems) {
				if(loadedDictionary.ContainsKey(item.Nomenclature.Id)) {
					//разбрасываем количества отгруженных по актуальным количествам в позициях заказа.
					int loadedCnt = (int)loadedDictionary[item.Nomenclature.Id];
					item.ActualCount = Math.Min(item.Count, loadedCnt);
					loadedDictionary[item.Nomenclature.Id] -= loadedCnt;
					if(loadedDictionary[item.Nomenclature.Id] <= 0)
						loadedDictionary.Remove(item.Nomenclature.Id);
				}
			}

			foreach(var item in OrderEquipments) {
				if(loadedDictionary.ContainsKey(item.Nomenclature.Id)) {
					//разбрасываем количества отгруженных по актуальным количествам в позициях заказа.
					int loadedCnt = (int)loadedDictionary[item.Nomenclature.Id];
					item.ActualCount = Math.Min(item.Count, loadedCnt);
					loadedDictionary[item.Nomenclature.Id] -= loadedCnt;
					if(loadedDictionary[item.Nomenclature.Id] <= 0)
						loadedDictionary.Remove(item.Nomenclature.Id);
				}
			}
		}

		/// <summary>
		/// Проверка возможности создания нескольких заказов на одну дату и точку доставки
		/// </summary>
		public virtual ValidationResult ValidateCanCreateSeveralOrderForDateAndDeliveryPoint(ValidationContext validationContext)
		{
			if(!SelfDelivery && DeliveryPoint != null
			                 && DeliveryDate.HasValue
			                 && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_several_orders_for_date_and_deliv_point")
			                 && validationContext.Items.ContainsKey("uowFactory"))
			{
				bool hasMaster = ObservableOrderItems.Any(i => i.Nomenclature.Category == NomenclatureCategory.master);

				var orderCheckedOutsideSession = _orderRepository
					.GetSameOrderForDateAndDeliveryPoint((IUnitOfWorkFactory)validationContext.Items["uowFactory"],
						DeliveryDate.Value, DeliveryPoint)
					.Where(o => o.Id != Id
					            && !_orderRepository.GetGrantedStatusesToCreateSeveralOrders().Contains(o.OrderStatus)
					            && o.OrderAddressType != OrderAddressType.Service).ToList();

				if(!hasMaster
				   && orderCheckedOutsideSession.Any())
				{
					return new ValidationResult(
						string.Format("Создать заказ нельзя, т.к. для этой даты и точки доставки уже был создан заказ {0}", orderCheckedOutsideSession.FirstOrDefault().Id),
						new[] { this.GetPropertyName(o => o.OrderEquipments) });

				}
			}

			return ValidationResult.Success;
		}


		/// <summary>
		/// Проверка на наличие воды по умолчанию в заказе для выбранной точки доставки и выдача сообщения о возможном штрафе
		/// </summary>
		/// <returns><c>true</c>, если пользователь подтвердил замену воды по умолчанию
		/// или если для точки доставки не указана вода по умолчанию
		/// или если среди товаров в заказе имеется вода по умолчанию,
		/// <c>false</c> если в заказе среди воды нет воды по умолчанию и
		/// пользователь не хочет её добавлять в заказ,
		/// <c>null</c> если данных для проверки не достаточно</returns>
		public virtual bool? DefaultWaterCheck(IInteractiveService interactiveService)
		{
			var res = IsWrongWater(out string title, out string message);
			if(res == true)
				return interactiveService.Question(message);
			return !res;
		}

		/// <summary>
		/// Проверка на наличие воды по умолчанию в заказе для выбранной
		/// точки доставки и формирование сообщения о возможном штрафе
		/// </summary>
		/// <returns><c>false</c> если для точки доставки не указана вода по
		/// умолчанию, или если в заказе есть какая-либо 19л вода, среди которой
		/// имеется вода по умолчанию <c>true</c> если в точке доставки указана
		/// вода по умолчанию и в заказе есть какая-либо 19л вода, среди которой
		/// этой умолчальной нет <c>null</c> если проверка не может быть
		/// выполнена ввиду отсутствия каких-либо данных</returns>
		public virtual bool? IsWrongWater(out string title, out string msg)
		{
			title = msg = string.Empty;
			if(DeliveryPoint == null)
				return null;
			Nomenclature defaultWater = DeliveryPoint.DefaultWaterNomenclature;
			var orderWaters = ObservableOrderItems.Where(w => w.Nomenclature.Category == NomenclatureCategory.water && !w.Nomenclature.IsDisposableTare);

			//Если имеется для точки доставки номенклатура по умолчанию,
			//если имеется вода в заказе и ни одна 19 литровая вода в заказе
			//не совпадает с номенклатурой по умолчанию, то сообщение о штрафе!
			if(defaultWater != null
			   && orderWaters.Any()
			   && !ObservableOrderItems.Any(i => i.Nomenclature.Category == NomenclatureCategory.water && !i.Nomenclature.IsDisposableTare
												   && i.Nomenclature == defaultWater)) {

				//список вод в заказе за исключением дефолтной для сообщения о штрафе
				string waterInOrder = string.Empty;
				foreach(var item in orderWaters) {
					if(item.Nomenclature != defaultWater)
						waterInOrder += string.Format(",\n\t'{0}'", item.Nomenclature.ShortOrFullName);
				}
				waterInOrder = waterInOrder.TrimStart(',');
				title = "Внимание!";
				string header = "Есть риск получить <span foreground=\"Red\" size=\"x-large\">ШТРАФ</span>!\n";
				string text = string.Format("Клиент '{0}' для адреса '{1}' заказывает фиксировано воду \n'{2}'.\nВ заказе же вы указали: {3}. \nДля подтверждения что это не ошибка, нажмите 'Да'.",
											Client.Name,
											DeliveryPoint.ShortAddress,
											defaultWater.ShortOrFullName,
											waterInOrder);
				msg = header + text;
				return true;
			}
			return false;
		}


		/// <summary>
		/// Закрывает заказ с самовывозом если по всем документам самовывоза со
		/// склада все отгружено, и произведена оплата
		/// </summary>
		public virtual bool TryCloseSelfDeliveryOrderWithCallTask(IUnitOfWork uow, IStandartNomenclatures standartNomenclatures, IRouteListItemRepository routeListItemRepository, ISelfDeliveryRepository selfDeliveryRepository, ICashRepository cashRepository, CallTaskWorker callTaskWorker, SelfDeliveryDocument closingDocument = null)
		{
			if(routeListItemRepository == null)
				throw new ArgumentNullException(nameof(routeListItemRepository));
			if(selfDeliveryRepository == null)
				throw new ArgumentNullException(nameof(selfDeliveryRepository));
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));

			bool isNotShipped = !IsFullyShippedSelfDeliveryOrder(uow, selfDeliveryRepository, closingDocument);

			if(!isNotShipped)
				UpdateBottlesMovementOperationWithoutDelivery(UoW, standartNomenclatures, routeListItemRepository, cashRepository);
			else
				return false;

			if(OrderStatus != OrderStatus.OnLoading)
				return false;

			bool isFullyPaid = SelfDeliveryIsFullyPaid(cashRepository);

			switch(PaymentType)
			{
				case PaymentType.Cash:
					ChangeStatusAndCreateTasks(isFullyPaid ? OrderStatus.Closed : OrderStatus.WaitForPayment, callTaskWorker);
					break;
				case PaymentType.Cashless:
				case PaymentType.PaidOnline:
				case PaymentType.Terminal:
					ChangeStatusAndCreateTasks(PayAfterShipment ? OrderStatus.WaitForPayment : OrderStatus.Closed, callTaskWorker);
					break;
				case PaymentType.SmsQR:
					ChangeStatusAndCreateTasks(
						PayAfterShipment 
						? OnlineOrder != null 
							? OrderStatus.Closed 
							: OrderStatus.WaitForPayment
						: OrderStatus.Closed, 
						callTaskWorker);
					break;
				case PaymentType.Barter:
				case PaymentType.ContractDocumentation:
					ChangeStatusAndCreateTasks(OrderStatus.Closed, callTaskWorker);
					break;
			}
			//обновление актуальных кол-в из документов самовывоза, включая не сохранённый
			//документ, откуда был вызов метода
			UpdateSelfDeliveryActualCounts(closingDocument);
			return true;
		}

		/// <summary>
		/// Закрывает заказ с самовывозом если по всем документам самовывоза со
		/// склада все отгружено, и произведена оплата
		/// </summary>
		public virtual bool TryCloseSelfDeliveryPayAfterShipmentOrder(
			IUnitOfWork uow,
			IStandartNomenclatures standartNomenclatures,
			IRouteListItemRepository routeListItemRepository,
			ISelfDeliveryRepository selfDeliveryRepository,
			ICashRepository cashRepository,
			SelfDeliveryDocument closingDocument = null)
		{
			if(routeListItemRepository == null)
				throw new ArgumentNullException(nameof(routeListItemRepository));
			if(selfDeliveryRepository == null)
				throw new ArgumentNullException(nameof(selfDeliveryRepository));
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));

			bool isNotShipped = !IsFullyShippedSelfDeliveryOrder(uow, selfDeliveryRepository, closingDocument);

			if(!isNotShipped)
				UpdateBottlesMovementOperationWithoutDelivery(UoW, standartNomenclatures, routeListItemRepository, cashRepository);
			else
				return false;

			if(OrderStatus == OrderStatus.WaitForPayment && PayAfterShipment)
			{
				bool isFullyPaid = SelfDeliveryIsFullyPaid(cashRepository);

				switch(PaymentType)
				{
					case PaymentType.Cash:
						ChangeStatus(isFullyPaid ? OrderStatus.Closed : OrderStatus.WaitForPayment);
						break;
					case PaymentType.Cashless:
					case PaymentType.PaidOnline:
						ChangeStatus(OrderStatus.Closed);
						break;
					case PaymentType.SmsQR:
						ChangeStatus(OrderStatus.Closed);
						break;
					case PaymentType.Barter:
					case PaymentType.ContractDocumentation:
						ChangeStatus(OrderStatus.Closed);
						break;
				}
				//обновление актуальных кол-в из документов самовывоза, включая не сохранённый
				//документ, откуда был вызов метода
				UpdateSelfDeliveryActualCounts(closingDocument);
				return true;

			}

			return false;
		}


		private void DeleteBottlesMovementOperation(IUnitOfWork uow)
		{
			if(BottlesMovementOperation != null) {
				uow.Delete(BottlesMovementOperation);
				BottlesMovementOperation = null;
			}
		}


		public virtual bool UpdateBottleMovementOperation(IUnitOfWork uow, IStandartNomenclatures standartNomenclatures, int returnByStock, int? forfeitQuantity = null)
		{
			if(IsContractCloser)
				return false;

			int amountDelivered = (int)OrderItems.Where(item => item.Nomenclature.Category == NomenclatureCategory.water && !item.Nomenclature.IsDisposableTare)
								.Sum(item => item?.ActualCount ?? 0);

			if(forfeitQuantity == null) {
				forfeitQuantity = (int)OrderItems.Where(i => i.Nomenclature.Id == standartNomenclatures.GetForfeitId())
							.Select(i => i?.ActualCount ?? 0)
							.Sum();
			}

			bool isValidCondition = amountDelivered != 0;
			isValidCondition |= returnByStock > 0;
			isValidCondition |= forfeitQuantity > 0;
			isValidCondition &= !_orderRepository.GetUndeliveryStatuses().Contains(OrderStatus);

			if(isValidCondition) {
				if(BottlesMovementOperation == null) {
					BottlesMovementOperation = new BottlesMovementOperation {
						Order = this,
					};
				}
				BottlesMovementOperation.Counterparty = Client;
				BottlesMovementOperation.DeliveryPoint = DeliveryPoint;
				BottlesMovementOperation.OperationTime = DeliveryDate.Value.Date.AddHours(23).AddMinutes(59);
				BottlesMovementOperation.Delivered = amountDelivered;
				BottlesMovementOperation.Returned = returnByStock + forfeitQuantity.Value;
				uow.Save(BottlesMovementOperation);
			} else {
				DeleteBottlesMovementOperation(uow);
			}

			return true;
		}

		/// <summary>
		/// Создание операций перемещения бутылей для заказов без доставки
		/// </summary>
		public virtual void UpdateBottlesMovementOperationWithoutDelivery(IUnitOfWork uow, IStandartNomenclatures standartNomenclatures, IRouteListItemRepository routeListItemRepository, ICashRepository cashRepository, decimal incomeCash = 0, decimal expenseCash = 0)
		{
			if(routeListItemRepository == null)
				throw new ArgumentNullException(nameof(routeListItemRepository));
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));
			if(standartNomenclatures == null)
				throw new ArgumentNullException(nameof(standartNomenclatures));

			//По заказам, у которых проставлен крыжик "Закрывашка по контракту",
			//не должны создаваться операции перемещения тары
			if(IsContractCloser) {
				DeleteBottlesMovementOperation(uow);
				return;
			}

			if(routeListItemRepository.HasRouteListItemsForOrder(uow, this))
				return;

			foreach(OrderItem item in OrderItems)
				if(!item.ActualCount.HasValue)
					item.ActualCount = item.Count;

			int? forfeitQuantity = null;

			if(!SelfDelivery || SelfDeliveryIsFullyPaid(cashRepository, incomeCash, expenseCash))
				forfeitQuantity = (int)OrderItems.Where(i => i.Nomenclature.Id == standartNomenclatures.GetForfeitId())
											.Select(i => i.ActualCount ?? 0)
											.Sum();

			UpdateBottleMovementOperation(uow, standartNomenclatures, ReturnedTare ?? 0, forfeitQuantity ?? 0);
		}

		public virtual void ChangePaymentTypeToByCardTerminal (ICallTaskWorker callTaskWorker)
		{
			PaymentType = PaymentType.Terminal;
			ChangeStatusAndCreateTasks(!PayAfterShipment ? OrderStatus.Accepted : OrderStatus.Closed, callTaskWorker);
		}

		public virtual void ChangePaymentTypeToOnline(ICallTaskWorker callTaskWorker)
		{
			PaymentType = PaymentType.PaidOnline;
			ChangeStatusAndCreateTasks(!PayAfterShipment ? OrderStatus.Accepted : OrderStatus.Closed, callTaskWorker);
		}

		#region Работа с документами

		public virtual OrderDocumentType[] GetRequirementDocTypes()
		{
			//создаём объект-ключ на основе текущего заказа. Этот ключ содержит набор свойств,
			//по которым будет происходить подбор правила для создания набора документов
			var key = new OrderStateKey(this);

			//обращение к хранилищу правил для получения массива типов документов по ключу
			return OrderDocumentRulesRepository.GetSetOfDocumets(key);
		}

		public virtual void UpdateDocuments()
		{
			CheckAndCreateDocuments(GetRequirementDocTypes());
		}

		public virtual void UpdateCertificates(out List<Nomenclature> nomenclaturesNeedUpdate)
		{
			nomenclaturesNeedUpdate = new List<Nomenclature>();
			if(AddCertificates && DeliveryDate.HasValue) {
				IList<Certificate> newList = new List<Certificate>();
				foreach(var item in _nomenclatureRepository.GetDictionaryWithCertificatesForNomenclatures(UoW, OrderItems.Select(i => i.Nomenclature).ToArray())) {
					if(item.Value.All(c => c.IsArchive || c.ExpirationDate.HasValue && c.ExpirationDate.Value < DeliveryDate))
						nomenclaturesNeedUpdate.Add(item.Key);
					else
						newList.Add(item.Value.FirstOrDefault(c => c.ExpirationDate == item.Value.Max(cert => cert.ExpirationDate)));
				}

				newList = newList.Distinct().ToList();
				var oldList = new List<Certificate>();

				foreach(var cer in OrderDocuments.Where(d => d.Type == OrderDocumentType.ProductCertificate)
												 .Cast<NomenclatureCertificateDocument>()
												 .Select(c => c.Certificate))
					oldList.Add(cer);

				foreach(var cer in oldList) {
					if(!newList.Any(c => c == cer)) {
						var removingDoc = OrderDocuments.Where(d => d.Type == OrderDocumentType.ProductCertificate)
														.Cast<NomenclatureCertificateDocument>()
														.FirstOrDefault(d => d.Certificate == cer);
						ObservableOrderDocuments.Remove(removingDoc);
					}
				}

				foreach(var cer in newList) {
					if(!oldList.Any(c => c == cer))
						ObservableOrderDocuments.Add(
							new NomenclatureCertificateDocument {
								Order = this,
								AttachedToOrder = this,
								Certificate = cer
							}
						);
				}
			}

			if(!AddCertificates)
			{
				ObservableOrderDocuments.Where(od => od.Type == OrderDocumentType.ProductCertificate).ToList()
					.ForEach(od => ObservableOrderDocuments.Remove(od));
			}
		}

		public virtual void CheckDocumentExportPermissions()
		{
			var updDoc = OrderDocuments.OfType<UPDDocument>().FirstOrDefault();
			if(updDoc != null && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_export_UPD_to_excel")) {
				updDoc.RestrictedOutputPresentationTypes = new[] { OutputPresentationType.ExcelTableOnly, OutputPresentationType.Excel2007 };
			}

			var specialUpdDoc = OrderDocuments.OfType<SpecialUPDDocument>().FirstOrDefault();
			if(specialUpdDoc != null && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_export_UPD_to_excel")) {
				specialUpdDoc.RestrictedOutputPresentationTypes = new[] { OutputPresentationType.ExcelTableOnly, OutputPresentationType.Excel2007 };
			}
		}

		private void CheckAndCreateDocuments(params OrderDocumentType[] needed)
		{
			var docsOfOrder = typeof(OrderDocumentType).GetFields()
													   .Where(x => x.GetCustomAttributes(typeof(DocumentOfOrderAttribute), false).Any())
													   .Select(x => (OrderDocumentType)x.GetValue(null))
													   .ToArray();

			if(needed.Any(x => !docsOfOrder.Contains(x)))
				throw new ArgumentException($@"В метод можно передавать только типы документов помеченные атрибутом {nameof(DocumentOfOrderAttribute)}", nameof(needed));

			var needCreate = needed.ToList();
			foreach(var doc in OrderDocuments.Where(d => d.Order?.Id == Id && docsOfOrder.Contains(d.Type)).ToList()) {
				if(needed.Contains(doc.Type))
					needCreate.Remove(doc.Type);
				else
					ObservableOrderDocuments.Remove(doc);
				if(OrderDocuments.Any(x => x.Order?.Id == Id && x.Id != doc.Id && x.Type == doc.Type)) {
					ObservableOrderDocuments.Remove(doc);
				}
			}
			//Создаем отсутствующие
			foreach(var type in needCreate) {
				if(ObservableOrderDocuments.Any(x => x.Order?.Id == Id && x.Type == type))
					continue;
				ObservableOrderDocuments.Add(CreateDocumentOfOrder(type));
			}
			CheckDocumentCount(this);
		}

		private void CheckDocumentCount(Order order)
		{
			var torg12document = order.ObservableOrderDocuments.FirstOrDefault(x => x is Torg12Document && x.Type == OrderDocumentType.Torg12);
			if(torg12document != null && IsCashlessPaymentTypeAndOrganizationWithoutVAT) {
				((Torg12Document)torg12document).CopiesToPrint = 2;
			}
		}

		private OrderDocument CreateDocumentOfOrder(OrderDocumentType type)
		{
			OrderDocument newDoc;
			switch(type) {
				case OrderDocumentType.Bill:
					newDoc = new BillDocument();
					break;
				case OrderDocumentType.SpecialBill:
					newDoc = new SpecialBillDocument();
					break;
				case OrderDocumentType.UPD:
					var updDocument = new UPDDocument();
					if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_export_UPD_to_excel")) {
						updDocument.RestrictedOutputPresentationTypes = new[] { OutputPresentationType.ExcelTableOnly, OutputPresentationType.Excel2007 };
					}
					newDoc = updDocument;
					break;
				case OrderDocumentType.SpecialUPD:
					var specialUpdDocument = new SpecialUPDDocument();
					if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_export_UPD_to_excel")) {
						specialUpdDocument.RestrictedOutputPresentationTypes = new[] { OutputPresentationType.ExcelTableOnly, OutputPresentationType.Excel2007 };
					}
					newDoc = specialUpdDocument;
					break;
				case OrderDocumentType.Invoice:
					newDoc = new InvoiceDocument();
					break;
				case OrderDocumentType.InvoiceBarter:
					newDoc = new InvoiceBarterDocument();
					break;
				case OrderDocumentType.InvoiceContractDoc:
					newDoc = new InvoiceContractDoc();
					break;
				case OrderDocumentType.Torg12:
					newDoc = new Torg12Document();
					break;
				case OrderDocumentType.ShetFactura:
					newDoc = new ShetFacturaDocument();
					break;
				case OrderDocumentType.DriverTicket:
					newDoc = new DriverTicketDocument();
					break;
				case OrderDocumentType.DoneWorkReport:
					newDoc = new DoneWorkDocument();
					break;
				case OrderDocumentType.EquipmentTransfer:
					newDoc = new EquipmentTransferDocument();
					break;
				case OrderDocumentType.EquipmentReturn:
					newDoc = new EquipmentReturnDocument();
					break;
				case OrderDocumentType.TransportInvoice:
					newDoc = new TransportInvoiceDocument();
					break;
				case OrderDocumentType.Torg2:
					newDoc = new Torg2Document();
					break;
				case OrderDocumentType.AssemblyList:
					newDoc = new AssemblyListDocument();
					break;
				default:
					throw new NotSupportedException("Не поддерживаемый тип документа");
			}
			newDoc.Order = newDoc.AttachedToOrder = this;
			return newDoc;
		}

		#endregion

		/// <summary>
		/// Возврат первого попавшегося контакта из цепочки:<br/>
		/// 0. Почта для чеков в контрагенте<br/>
		/// 1. Почта для счетов в контрагенте<br/>
		/// 2. Телефон для чеков в точке доставки<br/>
		/// 3. Телефон для чеков в контрагенте<br/>
		/// 4. Телефон личный в ТД<br/>
		/// 5. Телефон личный в контрагенте<br/>
		/// 6. Иная почта в контрагенте<br/>
		/// 7. Городской телефон в ТД<br/>
		/// 8. Городской телефон в контрагенте<br/>
		/// </summary>
		/// <returns>Контакт с минимальным весом.<br/>Телефоны возвращает в формате +7</returns>
		public virtual string GetContact()
		{
			if(Client == null)
			{
				return null;
			}

			//Dictionary<вес контакта, контакт>
			Dictionary<int, string> contacts = new Dictionary<int, string>();

			try
			{
				if(!SelfDelivery && DeliveryPoint != null && DeliveryPoint.Phones.Any())
				{
					var deliveryPointReceiptPhone = DeliveryPoint.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.PhoneType?.PhonePurpose == PhonePurpose.ForReceipts
							&& !p.IsArchive);
					
					if(deliveryPointReceiptPhone != null)
					{
						contacts[2] = "+7" + deliveryPointReceiptPhone.DigitsNumber;
					}

					var phone = DeliveryPoint.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.DigitsNumber.Substring(0, 1) == "9"
							&& !p.IsArchive);
					
					if(phone != null)
					{
						contacts[4] = "+7" + phone.DigitsNumber;
					}
					else if(DeliveryPoint.Phones.Any(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& !p.IsArchive))
					{
						contacts[7] = "+7" + DeliveryPoint.Phones.FirstOrDefault(
							p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
								&& !p.IsArchive).DigitsNumber;
					}
				}
			}
			catch(GenericADOException ex)
			{
				logger.Error(ex.Message);
			}

			try
			{
				if(Client.Phones.Any())
				{
					var clientReceiptPhone = Client.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.PhoneType?.PhonePurpose == PhonePurpose.ForReceipts
							&& !p.IsArchive);
					
					if(clientReceiptPhone != null)
					{
						contacts[3] = "+7" + clientReceiptPhone.DigitsNumber;
					}

					var phone = Client.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.DigitsNumber.Substring(0, 1) == "9"
							&& !p.IsArchive);
					
					if(phone != null)
					{
						contacts[5] = "+7" + phone.DigitsNumber;
					}
					else if(Client.Phones.Any(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& !p.IsArchive))
					{
						contacts[8] = "+7" + Client.Phones.FirstOrDefault(
							p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
								&& !p.IsArchive).DigitsNumber;
					}
				}
			}
			catch(GenericADOException ex)
			{
				logger.Error(ex.Message);
			}
			try
			{
				if(Client.Emails.Any())
				{
					var receiptEmail = Client.Emails.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Address)
						 && e.EmailType?.EmailPurpose == EmailPurpose.ForReceipts)?.Address;

					if(receiptEmail != null)
					{
						contacts[0] = receiptEmail;
					}

					var billsEmail = Client.Emails.FirstOrDefault(
						e => !string.IsNullOrWhiteSpace(e.Address)
							&& e.EmailType?.EmailPurpose == EmailPurpose.ForBills)?.Address;
					
					if(billsEmail != null)
					{
						contacts[1] = billsEmail;
					}

					var email = Client.Emails.FirstOrDefault(e =>
						!string.IsNullOrWhiteSpace(e.Address)
						&& e.EmailType?.EmailPurpose != EmailPurpose.ForBills
						&& e.EmailType?.EmailPurpose != EmailPurpose.ForReceipts)
						?.Address;
					
					if(email != null)
					{
						contacts[6] = email;
					}
				}
			}
			catch(GenericADOException ex)
			{
				logger.Error(ex.Message);
			}

			if(!contacts.Any())
			{
				return null;
			}

			var onlyWithValidPhones = contacts.Where(x =>
				(x.Value.StartsWith("+7")
					&& x.Value.Length == 12)
				|| !x.Value.StartsWith("+7"));

			if(!onlyWithValidPhones.Any())
			{
				throw new InvalidOperationException($"Не удалось подобрать контакт для заказа {Id}");
			}

			int minWeight = onlyWithValidPhones.Min(c => c.Key);
			var contact = contacts[minWeight];

			if(string.IsNullOrWhiteSpace(contact))
			{
				throw new InvalidOperationException($"Не удалось подобрать контакт для заказа {Id}");
			}

			return contact;
		}

		public virtual void SaveOrderComment()
		{
			if(Id == 0) return;

			using(var uow = UnitOfWorkFactory.CreateForRoot<Order>(Id, "Кнопка сохранить только комментарий к заказу")) {
				uow.Root.Comment = Comment;
				uow.Save();
				uow.Commit();
			}
			UoW.Session.Refresh(this);
		}

		public virtual void SaveEntity(
			IUnitOfWork uow,
			Employee currentEmployee,
			IOrderDailyNumberController orderDailyNumberController,
			IPaymentFromBankClientController paymentFromBankClientController)
		{
			SetFirstOrder();

			if(!IsLoadedFrom1C)
			{
				UpdateContract();
			}

			LastEditor = currentEmployee;
			LastEditedTime = DateTime.Now;
			ParseTareReason();
			ClearPromotionSets();
			orderDailyNumberController.UpdateDailyNumber(this);
			paymentFromBankClientController.UpdateAllocatedSum(UoW, this);
			paymentFromBankClientController.ReturnAllocatedSumToClientBalanceIfChangedPaymentTypeFromCashless(UoW, this);

			var hasPromotionalSetForNewClient = PromotionalSets.Any(x => !x.CanBeReorderedWithoutRestriction);

			if(hasPromotionalSetForNewClient
			   && ContactlessDelivery
			   && !SelfDelivery
			   && !Comment.Contains(_generalSettingsParameters.OrderAutoComment))
			{
				Comment = $"{_generalSettingsParameters.OrderAutoComment}{Environment.NewLine}{Comment}";
			}

			uow.Save();
		}

		public virtual void RemoveReturnTareReason()
		{
			if (ReturnTareReason != null)
				ReturnTareReason = null;

			if(ReturnTareReasonCategory != null)
				ReturnTareReasonCategory = null;
		}

		public virtual void SetActualCountsToZeroOnCanceled()
		{
			foreach(var item in OrderItems)
			{
				if(!item.OriginalDiscountMoney.HasValue || !item.OriginalDiscount.HasValue)
				{
					item.OriginalDiscountMoney = item.DiscountMoney > 0 ? (decimal?)item.DiscountMoney : null;
					item.OriginalDiscount = item.Discount > 0 ? (decimal?)item.Discount : null;
					item.OriginalDiscountReason = (item.DiscountMoney > 0 || item.Discount > 0) ? item.DiscountReason : null;
				}
				item.ActualCount = 0m;
			}

			foreach(var equip in OrderEquipments)
			{
				equip.ActualCount = 0;
			}

			foreach(var deposit in OrderDepositItems)
			{
				deposit.ActualCount = 0;
			}
		}

		public virtual void RestoreOrder()
		{
			foreach(var item in OrderItems)
			{
				if(item.OriginalDiscountMoney.HasValue || item.OriginalDiscount.HasValue)
				{
					item.DiscountMoney = item.OriginalDiscountMoney ?? 0;
					item.DiscountReason = item.OriginalDiscountReason;
					item.Discount = item.OriginalDiscount ?? 0;
					item.OriginalDiscountMoney = null;
					item.OriginalDiscountReason = null;
					item.OriginalDiscount = null;
				}
				item.ActualCount = null;
			}
			foreach(var equip in OrderEquipments)
			{
				equip.ActualCount = null;
			}

			foreach(var deposit in OrderDepositItems)
			{
				deposit.ActualCount = null;
			}
		}


		/// <summary>
		/// Возвращает список со всеми товарами, которые нужно доставить клиенту
		/// </summary>
		/// <returns></returns>
		public virtual IList<NomenclatureAmountNode> GetAllGoodsToDeliver(bool isActualCount = false)
		{
			var result = new List<NomenclatureAmountNode>();

			foreach(var orderItem in OrderItems.Where(x => Nomenclature.GetCategoriesForShipment().Contains(x.Nomenclature.Category)))
			{
				var found = result.FirstOrDefault(x => x.NomenclatureId == orderItem.Nomenclature.Id);
				if(found != null)
				{
					found.Amount += isActualCount && orderItem.ActualCount.HasValue ? orderItem.ActualCount.Value : orderItem.Count;
				}
				else
				{
					result.Add(new NomenclatureAmountNode
					{
						NomenclatureId = orderItem.Nomenclature.Id,
						Nomenclature = orderItem.Nomenclature,
						Amount = isActualCount && orderItem.ActualCount.HasValue ? orderItem.ActualCount.Value : orderItem.Count
					});
				}
			}

			foreach(var equipment in OrderEquipments.Where(x => x.Direction == Direction.Deliver
						&& Nomenclature.GetCategoriesForShipment().Contains(x.Nomenclature.Category)))
			{
				var found = result.FirstOrDefault(x => x.NomenclatureId == equipment.Nomenclature.Id);
				if(found != null)
				{
					found.Amount += isActualCount && equipment.ActualCount.HasValue ? equipment.ActualCount.Value : equipment.Count;
				}
				else
				{
					result.Add(new NomenclatureAmountNode
					{
						NomenclatureId = equipment.Nomenclature.Id,
						Nomenclature = equipment.Nomenclature,
						Amount = isActualCount && equipment.ActualCount.HasValue ? equipment.ActualCount.Value : equipment.Count
					});
				}
			}
			return result;
		}

		public virtual void SetNeedToRecendEdoUpd()
		{
			var userCanResendUpd = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_resend_upd_documents");
			if(!userCanResendUpd)
			{
				InteractiveService.ShowMessage(ImportanceLevel.Warning, "Текущий пользователь не имеет права повторной отправки УПД");
				return;
			}

			if(OrderPaymentStatus == OrderPaymentStatus.Paid)
			{
				if(!ServicesConfig.InteractiveService.Question(
					$"Счет по заказу №{Id} оплачен.\r\nПроверьте, пожалуйста, статус УПД в ЭДО перед повторной отправкой на предмет аннулирован/не аннулирован, подписан/не подписан.\r\n\r\n" +
					$"Вы уверены, что хотите отправить повторно?"))
				{
					return;
				}
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var edoDocumentsActions = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
					.Where(x => x.Order.Id == Id)
					.FirstOrDefault();

				if(edoDocumentsActions == null)
				{
					edoDocumentsActions = new OrderEdoTrueMarkDocumentsActions();
					edoDocumentsActions.Order = this;
				}

				edoDocumentsActions.IsNeedToResendEdoUpd = true;

				var orderLastTrueMarkDocument = uow.GetAll<TrueMarkApiDocument>()
					.Where(x => x.Order.Id == Id)
					.OrderByDescending(x => x.CreationDate)
					.FirstOrDefault();

				if(orderLastTrueMarkDocument != null 
					&& orderLastTrueMarkDocument.Type != TrueMarkApiDocument.TrueMarkApiDocumentType.WithdrawalCancellation)
				{
					edoDocumentsActions.IsNeedToCancelTrueMarkDocument = true;
				}

				uow.Save(edoDocumentsActions);
				uow.Commit();
			}				
		}

		#endregion

		#region Аренда

		#region NonFreeRent

		public virtual void AddNonFreeRent(PaidRentPackage paidRentPackage, Nomenclature equipmentNomenclature)
		{
			OrderItem orderRentDepositItem = GetExistingNonFreeRentDepositItem(paidRentPackage);
			if(orderRentDepositItem == null) {
				orderRentDepositItem = CreateNewNonFreeRentDepositItem(paidRentPackage);
				AddOrderItem(orderRentDepositItem);
			}

			OrderItem orderRentServiceItem = GetExistingNonFreeRentServiceItem(paidRentPackage);
			if(orderRentServiceItem == null) {
				orderRentServiceItem = CreateNewNonFreeRentServiceItem(paidRentPackage);
				AddOrderItem(orderRentServiceItem);
			}

			OrderEquipment orderRentEquipment = GetExistingRentEquipmentItem(equipmentNomenclature, orderRentDepositItem, orderRentServiceItem);
			if (orderRentEquipment == null) {
				orderRentEquipment = CreateNewRentEquipmentItem(equipmentNomenclature, orderRentDepositItem, orderRentServiceItem);
				ObservableOrderEquipments.Add(orderRentEquipment);
			} else {
				orderRentEquipment.Count++;
			}

			UpdateRentsCount();

			OnPropertyChanged(nameof(OrderSum));
			OnPropertyChanged(nameof(OrderCashSum));
		}

		private OrderItem GetExistingNonFreeRentDepositItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentDepositItem = OrderItems
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == OrderRentType.NonFreeRent)
				.Where(x => x.OrderItemRentSubType == OrderItemRentSubType.RentDepositItem)
				.FirstOrDefault();
			return orderRentDepositItem;
		}

		private OrderItem CreateNewNonFreeRentDepositItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentDepositItem = new OrderItem {
				Order = this,
				Count = 1,
				RentType = OrderRentType.NonFreeRent,
				OrderItemRentSubType = OrderItemRentSubType.RentDepositItem,
				PaidRentPackage = paidRentPackage,
				Price = paidRentPackage.Deposit,
				Nomenclature = paidRentPackage.DepositService
			};
			return orderRentDepositItem;
		}

		private OrderItem GetExistingNonFreeRentServiceItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentServiceItem = OrderItems
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == OrderRentType.NonFreeRent)
				.Where(x => x.OrderItemRentSubType == OrderItemRentSubType.RentServiceItem)
				.FirstOrDefault();
			return orderRentServiceItem;
		}

		private OrderItem CreateNewNonFreeRentServiceItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentServiceItem = new OrderItem {
				Order = this,
				Count = 1,
				RentCount = 1,
				RentType = OrderRentType.NonFreeRent,
				OrderItemRentSubType = OrderItemRentSubType.RentServiceItem,
				PaidRentPackage = paidRentPackage,
				Price = paidRentPackage.PriceMonthly,
				Nomenclature = paidRentPackage.RentServiceMonthly
			};
			return orderRentServiceItem;
		}

		#endregion NonFreeRent

		#region DailyRent

		public virtual void AddDailyRent(PaidRentPackage paidRentPackage, Nomenclature equipmentNomenclature)
		{
			OrderItem orderRentDepositItem = GetExistingDailyRentDepositItem(paidRentPackage);
			if(orderRentDepositItem == null) {
				orderRentDepositItem = CreateNewDailyRentDepositItem(paidRentPackage);
				AddOrderItem(orderRentDepositItem);
			}

			OrderItem orderRentServiceItem = GetExistingDailyRentServiceItem(paidRentPackage);
			if(orderRentServiceItem == null) {
				orderRentServiceItem = CreateNewDailyRentServiceItem(paidRentPackage);
				AddOrderItem(orderRentServiceItem);
			}

			OrderEquipment orderRentEquipment = GetExistingRentEquipmentItem(equipmentNomenclature, orderRentDepositItem, orderRentServiceItem);
			if (orderRentEquipment == null) {
				orderRentEquipment = CreateNewRentEquipmentItem(equipmentNomenclature, orderRentDepositItem, orderRentServiceItem);
				ObservableOrderEquipments.Add(orderRentEquipment);
			} else {
				orderRentEquipment.Count++;
			}

			UpdateRentsCount();

			OnPropertyChanged(nameof(OrderSum));
			OnPropertyChanged(nameof(OrderCashSum));
		}

		private OrderItem GetExistingDailyRentDepositItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentDepositItem = OrderItems
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == OrderRentType.DailyRent)
				.Where(x => x.OrderItemRentSubType == OrderItemRentSubType.RentDepositItem)
				.FirstOrDefault();
			return orderRentDepositItem;
		}

		private OrderItem CreateNewDailyRentDepositItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentDepositItem = new OrderItem {
				Order = this,
				Count = 1,
				RentType = OrderRentType.DailyRent,
				OrderItemRentSubType = OrderItemRentSubType.RentDepositItem,
				PaidRentPackage = paidRentPackage,
				Price = paidRentPackage.Deposit,
				Nomenclature = paidRentPackage.DepositService
			};
			return orderRentDepositItem;
		}

		private OrderItem GetExistingDailyRentServiceItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentServiceItem = OrderItems
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == OrderRentType.DailyRent)
				.Where(x => x.OrderItemRentSubType == OrderItemRentSubType.RentServiceItem)
				.FirstOrDefault();
			return orderRentServiceItem;
		}

		private OrderItem CreateNewDailyRentServiceItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentServiceItem = new OrderItem {
				Order = this,
				Count = 1,
				RentCount = 1,
				RentType = OrderRentType.DailyRent,
				OrderItemRentSubType = OrderItemRentSubType.RentServiceItem,
				PaidRentPackage = paidRentPackage,
				Price = paidRentPackage.PriceDaily,
				Nomenclature = paidRentPackage.RentServiceDaily
			};
			return orderRentServiceItem;
		}

		#endregion DailyRent

		#region FreeRent

		public virtual void AddFreeRent(FreeRentPackage freeRentPackage, Nomenclature equipmentNomenclature)
		{
			OrderItem orderRentDepositItem = GetExistingFreeRentDepositItem(freeRentPackage);
			if(orderRentDepositItem == null) {
				orderRentDepositItem = CreateNewFreeRentDepositItem(freeRentPackage);
				AddOrderItem(orderRentDepositItem);
			}

			OrderEquipment orderRentEquipment = GetExistingRentEquipmentItem(equipmentNomenclature, orderRentDepositItem);
			if (orderRentEquipment == null) {
				orderRentEquipment = CreateNewRentEquipmentItem(equipmentNomenclature, orderRentDepositItem);
				ObservableOrderEquipments.Add(orderRentEquipment);
			} else {
				orderRentEquipment.Count++;
			}

			UpdateRentsCount();

			OnPropertyChanged(nameof(OrderSum));
			OnPropertyChanged(nameof(OrderCashSum));
		}

		private OrderItem GetExistingFreeRentDepositItem(FreeRentPackage freeRentPackage)
		{
			OrderItem orderRentDepositItem = OrderItems
				.Where(x => x.FreeRentPackage != null && x.FreeRentPackage.Id == freeRentPackage.Id)
				.Where(x => x.RentType == OrderRentType.FreeRent)
				.Where(x => x.OrderItemRentSubType == OrderItemRentSubType.RentDepositItem)
				.FirstOrDefault();
			return orderRentDepositItem;
		}

		private OrderItem CreateNewFreeRentDepositItem(FreeRentPackage freeRentPackage)
		{
			OrderItem orderRentDepositItem = new OrderItem {
				Order = this,
				Count = 1,
				RentType = OrderRentType.FreeRent,
				OrderItemRentSubType = OrderItemRentSubType.RentDepositItem,
				FreeRentPackage = freeRentPackage,
				Price = freeRentPackage.Deposit,
				Nomenclature = freeRentPackage.DepositService
			};
			return orderRentDepositItem;
		}

		#endregion FreeRent

		private OrderEquipment GetExistingRentEquipmentItem(Nomenclature nomenclature, OrderItem rentDepositItem, OrderItem rentServiceItem = null)
		{
			OrderEquipment rentEquipment = OrderEquipments
				.Where(x => x.Reason == Reason.Rent)
				.Where(x => x.Nomenclature == nomenclature)
				.Where(x => x.OrderRentDepositItem == rentDepositItem)
				.Where(x => x.OrderRentServiceItem == rentServiceItem)
				.FirstOrDefault();
			return rentEquipment;
		}

		private OrderEquipment CreateNewRentEquipmentItem(Nomenclature nomenclature, OrderItem rentDepositItem, OrderItem rentServiceItem = null)
		{
			OrderEquipment rentEquipment = new OrderEquipment {
				Order = this,
				Count = 1,
				Direction = Direction.Deliver,
				Nomenclature = nomenclature,
				Reason = Reason.Rent,
				DirectionReason = DirectionReason.Rent,
				OwnType = OwnTypes.Rent,
				OrderRentDepositItem = rentDepositItem,
				OrderRentServiceItem = rentServiceItem
			};
			return rentEquipment;
		}

		public virtual void UpdateRentsCount()
		{
			foreach (var orderItem in OrderItems.Where(x => x.OrderItemRentSubType != OrderItemRentSubType.None)) {
				switch(orderItem.OrderItemRentSubType) {
					case OrderItemRentSubType.RentServiceItem:
						var totalEquipmentCountForService = GetRentEquipmentTotalCountForServiceItem(orderItem);
						orderItem.SetRentEquipmentCount(totalEquipmentCountForService);
						break;
					case OrderItemRentSubType.RentDepositItem:
						var totalEquipmentCountForDeposit = GetRentEquipmentTotalCountForDepositItem(orderItem);
						orderItem.SetRentEquipmentCount(totalEquipmentCountForDeposit);
						break;
				}
			}
		}

		private int GetRentEquipmentTotalCountForDepositItem(OrderItem orderRentDepositItem)
		{
			var totalCount = orderEquipments.Where(x => x.OrderRentDepositItem == orderRentDepositItem)
				.Sum(x => x.Count);
			return totalCount;
		}

		private int GetRentEquipmentTotalCountForServiceItem(OrderItem orderRentServiceItem)
		{
			var totalCount = orderEquipments.Where(x => x.OrderRentServiceItem == orderRentServiceItem)
				.Sum(x => x.Count);
			return totalCount;
		}

		#endregion Аренда

		#region Акции

		/// <summary>
		/// Можем применить акцию "Бутыль"?
		/// </summary>
		public virtual bool CanAddStockBottle(IOrderRepository orderRepository)
		{
			bool result = Client != null && orderRepository.GetFirstRealOrderForClientForActionBottle(UoW, this,Client) == null;
			if(result) {
				BottlesReturn = 0;
			}
			return result;
		}

		#endregion

		#region	Внутренние функции

		private bool HasMarkedAndUnmarkedOrderItems()
		{
			var hasMarkedOrderItem = false;
			var hasUnmarkedOrderItem = false;

			foreach(var orderItem in ObservableOrderItems)
			{
				if(!hasMarkedOrderItem)
				{
					hasMarkedOrderItem = !string.IsNullOrWhiteSpace(orderItem.Nomenclature.Gtin);
				}

				if(!hasUnmarkedOrderItem)
				{
					hasUnmarkedOrderItem = string.IsNullOrWhiteSpace(orderItem.Nomenclature.Gtin);
				}
			}

			return hasMarkedOrderItem && hasUnmarkedOrderItem;
		}

		private decimal GetFixedPrice(OrderItem item) => item.GetWaterFixedPrice() ?? default(decimal);

		private decimal GetNomenclaturePrice(OrderItem item, bool useAlternativePrice)
		{
			decimal nomenclaturePrice = 0M;
			if(item.Nomenclature.IsWater19L) {
				nomenclaturePrice = item.Nomenclature.GetPrice(GetTotalWater19LCount(), useAlternativePrice);
			} else {
				nomenclaturePrice = item.Nomenclature.GetPrice(item.Count, useAlternativePrice);
			}
			return nomenclaturePrice;
		}

		private void ObservableOrderDepositItems_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(OrderSum));
		}

		protected internal virtual void ObservableOrderItems_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(OrderSum));
			UpdateDocuments();
		}

		public virtual void UpdateCommentManagerInfo(Employee editor)
		{
			CommentOPManagerUpdatedAt = DateTime.Now;
			CommentOPManagerChangedBy = editor;
		}

		#endregion

		#region Для расчетов в логистике

		/// <summary>
		/// Время разгрузки в секундах.
		/// </summary>
		public virtual int CalculateTimeOnPoint(bool hasForwarder)
		{
			int byFormula = 3 * 60; //На подпись документво 3 мин.
			int bottels = Total19LBottlesToDeliver;
			if(!hasForwarder)
				byFormula += CalculateGoDoorCount(bottels, 2) * 100; //100 секун(5/3 минуты) на 2 бутыли без экспедитора.
			else
				byFormula += CalculateGoDoorCount(bottels, 4) * 1 * 60; //1 минута на 4 бутыли c экспедитором.

			return byFormula < 5 * 60 ? 5 * 60 : byFormula;
		}

		private int CalculateGoDoorCount(int bottles, int atTime) => bottles / atTime + (bottles % atTime > 0 ? 1 : 0);

		/// <summary>
		/// Расчёт веса товаров и оборудования к клиенту для этого заказа
		/// </summary>
		/// <returns>Вес</returns>
		/// <param name="includeGoods">Если <c>true</c>, то в расчёт веса будут включены товары.</param>
		/// <param name="includeEquipment">Если <c>true</c>, то в расчёт веса будет включено оборудование.</param>
		public virtual decimal FullWeight(bool includeGoods = true, bool includeEquipment = true)
		{
			decimal weight = 0;
			if(includeGoods)
				weight += OrderItems.Sum(x => x.Nomenclature.Weight * x.Count);
			if(includeEquipment)
				weight += OrderEquipments.Where(x => x.Direction == Direction.Deliver)
										 .Sum(x => x.Nomenclature.Weight * x.Count);
			return weight;
		}

		public virtual decimal GetSalesItemsWeight(bool includeGoods = true, bool includeEquipment = true)
		{
			decimal weight = OrderItems.Sum(x => x.Nomenclature.Weight * (x.ActualCount ?? x.Count));
			return weight;
		}

		/// <summary>
		/// Расчёт объёма товаров и оборудования к клиенту для этого заказа
		/// </summary>
		/// <returns>Объём</returns>
		/// <param name="includeGoods">Если <c>true</c>, то в расчёт веса будут включены товары.</param>
		/// <param name="includeEquipment">Если <c>true</c>, то в расчёт веса будет включено оборудование.</param>
		public virtual decimal FullVolume(bool includeGoods = true, bool includeEquipment = true)
		{
			decimal volume = 0;
			if(includeGoods)
				volume += OrderItems.Sum(x => x.Nomenclature.Volume * x.Count);
			if(includeEquipment)
				volume += OrderEquipments.Where(x => x.Direction == Direction.Deliver)
										 .Sum(x => x.Nomenclature.Volume * x.Count);
			return volume;
		}

		/// <summary>
		/// Расчёт объёма ВОЗВРАЩАЕМОГО оборудования (из тары включается только 19-литровая), имеющие наравление от клиента для этого заказа
		/// </summary>
		/// <param name="includeBottlesReturn">Если <c>true</c>, то в расчёт объема будут включены возвращаемые бутыли, количество которых указано в свойстве BottlesReturn.</param>
		/// <param name="includeEquipment">Если <c>true</c>, то в расчёт объема будет включено оборудование, имеющее направление "От клиента"</param>
		/// <param name="one19LitersBottleVolume">Расчетное значение объема одного 19-литрового бутыля</param>
		/// <returns>Объём</returns>
		public virtual decimal FullReverseVolume(bool includeBottlesReturn = true, bool includeEquipment = true, decimal one19LitersBottleVolume = 0.03645m)
		{
			decimal volume = 0;
			if (includeBottlesReturn)
			{
				volume += (BottlesReturn ?? 0) * one19LitersBottleVolume;
			}
			if (includeEquipment)
			{
				volume += OrderEquipments
					.Where(
						x => x.Direction == Direction.PickUp
						&& (x.Nomenclature.Category == NomenclatureCategory.equipment
							|| (x.Nomenclature.Category == NomenclatureCategory.bottle && x.Nomenclature.TareVolume == TareVolume.Vol19L)))
					.Sum(x => x.Nomenclature.Volume * x.Count);
			}
			return volume;
		}

		#endregion

		#region Статические

		public static OrderStatus[] StatusesToExport1c => new[] {
			OrderStatus.Accepted,
			OrderStatus.Closed,
			OrderStatus.InTravelList,
			OrderStatus.OnLoading,
			OrderStatus.OnTheWay,
			OrderStatus.Shipped,
			OrderStatus.UnloadingOnStock
		};

		public static PaymentType[] PaymentTypesFastDeliveryAvailableFor => new[]
		{
			PaymentType.Cash,
			PaymentType.PaidOnline,
			PaymentType.Terminal,
			PaymentType.DriverApplicationQR,
			PaymentType.SmsQR,
			PaymentType.Cashless
		};

		#endregion

		#region Операции

		public virtual List<DepositOperation> UpdateDepositOperations(IUnitOfWork uow)
		{
			var bottleRefundDeposit = ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Bottles).Sum(x => x.ActualSum);
			var equipmentRefundDeposit = ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Equipment).Sum(x => x.ActualSum);
			var operations = UpdateDepositOperations(uow, equipmentRefundDeposit, bottleRefundDeposit);
			return operations;
		}

		public virtual List<DepositOperation> UpdateDepositOperations(IUnitOfWork uow, decimal equipmentRefundDeposit, decimal bottleRefundDeposit)
		{
			if(IsContractCloser == true) {
				DepositOperations?.Clear();
				return null;
			}

			var result = new List<DepositOperation>();
			DepositOperation bottlesOperation = null;
			DepositOperation equipmentOperation = null;
			bottlesOperation = DepositOperations?.FirstOrDefault(x => x.DepositType == DepositType.Bottles);
			equipmentOperation = DepositOperations?.FirstOrDefault(x => x.DepositType == DepositType.Equipment);

			//Залоги
			var bottleReceivedDeposit = OrderItems.Where(x => x.Nomenclature.TypeOfDepositCategory == TypeOfDepositCategory.BottleDeposit)
												  .Sum(x => x.ActualSum);
			var equipmentReceivedDeposit = OrderItems.Where(x => x.Nomenclature.TypeOfDepositCategory == TypeOfDepositCategory.EquipmentDeposit)
													 .Sum(x => x.ActualSum);

			//ЗАЛОГИ ЗА БУТЫЛИ
			if(bottleReceivedDeposit != 0m || bottleRefundDeposit != 0m) {
				if(bottlesOperation == null) {
					bottlesOperation = new DepositOperation {
						Order = this,
						OperationTime = DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
						DepositType = DepositType.Bottles,
						Counterparty = Client
					};
				}
				bottlesOperation.DeliveryPoint = DeliveryPoint;
				bottlesOperation.ReceivedDeposit = bottleReceivedDeposit;
				bottlesOperation.RefundDeposit = bottleRefundDeposit;
				result.Add(bottlesOperation);
			} else {
				if(bottlesOperation != null) {
					DepositOperations?.Remove(bottlesOperation);
					UoW.Delete(bottlesOperation);
				}
			}

			//ЗАЛОГИ ЗА ОБОРУДОВАНИЕ
			if(equipmentReceivedDeposit != 0m || equipmentRefundDeposit != 0m) {
				if(equipmentOperation == null) {
					equipmentOperation = new DepositOperation {
						Order = this,
						OperationTime = DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
						DepositType = DepositType.Equipment,
						Counterparty = Client
					};
				}
				equipmentOperation.DeliveryPoint = DeliveryPoint;
				equipmentOperation.ReceivedDeposit = equipmentReceivedDeposit;
				equipmentOperation.RefundDeposit = equipmentRefundDeposit;
				result.Add(equipmentOperation);
			} else {
				if(equipmentOperation != null) {
					DepositOperations.Remove(equipmentOperation);
					UoW.Delete(equipmentOperation);
				}
			}

			result.ForEach(x => uow.Save(x));

			return result;
		}

		#endregion

		#region Доставка за час

		public virtual bool CanChangeFastDelivery => OrderStatus == OrderStatus.NewOrder;

		private Nomenclature _fastDeliveryNomenclature;
		private Nomenclature FastDeliveryNomenclature
		{
			get
			{
				if(_fastDeliveryNomenclature == null)
				{
					var nomenclatureParametersProvider = new NomenclatureParametersProvider(new ParametersProvider());
					_fastDeliveryNomenclature = nomenclatureParametersProvider.GetFastDeliveryNomenclature(UoW);
				}

				return _fastDeliveryNomenclature;
			}
		}

		public virtual void AddFastDeliveryNomenclatureIfNeeded()
		{
			if(IsFastDelivery && orderItems.All(x => x.Nomenclature.Id != FastDeliveryNomenclature.Id))
			{
				var canApplyAlternativePrice = HasPermissionsForAlternativePrice
				                               && FastDeliveryNomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= 1);
				var fastDeliveryItemToAdd = new OrderItem
				{
					Order = this,
					Nomenclature = FastDeliveryNomenclature,
					Count = 1,
					Price = FastDeliveryNomenclature.GetPrice(1, canApplyAlternativePrice)
				};

				AddOrderItem(fastDeliveryItemToAdd);
			}
		}

		public virtual void RemoveFastDeliveryNomenclature()
		{
			var fastDeliveryItemToRemove =
					ObservableOrderItems.SingleOrDefault(x => x.Nomenclature.Id == FastDeliveryNomenclature.Id);

			RemoveOrderItem(fastDeliveryItemToRemove);
		}

		#endregion

		#region Точка доставки

		private int _educationalInstitutionDeliveryPointCategoryId;

		private int EducationalInstitutionDeliveryPointCategoryId
		{
			get
			{
				if(_educationalInstitutionDeliveryPointCategoryId == default(int))
				{
					var deliveryPointParametersProvider = new DeliveryPointParametersProvider(new ParametersProvider());
					_educationalInstitutionDeliveryPointCategoryId = deliveryPointParametersProvider.EducationalInstitutionDeliveryPointCategoryId;
				}

				return _educationalInstitutionDeliveryPointCategoryId;
			}
		}

		#endregion
	}
}

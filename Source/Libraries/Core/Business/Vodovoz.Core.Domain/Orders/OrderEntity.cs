using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Attributes;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Orders.Documents;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "заказы",
		Nominative = "заказ",
		Prepositional = "заказе",
		PrepositionalPlural = "заказах"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class OrderEntity : PropertyChangedBase, IDomainObject, IBusinessObject
	{
		public const string Table = "orders";
		private int _id;
		private DateTime _version;
		private DateTime? _createDate;
		private bool _isFirstOrder;
		private bool _isSecondOrder;
		private DateTime? _timeDelivered;
		private DateTime? _firstDeliveryDate;
		private DateTime _billDate = DateTime.Now;
		private string _deliverySchedule1c;
		private bool _payAfterShipment;
		private string _oDZComment;
		private string _oPComment;
		private DateTime? _commentOPManagerUpdatedAt;
		private int? _callBeforeArrivalMinutes;
		private bool? _isDoNotMakeCallBeforeArrival;
		private int? _bottlesReturn;
		private string _comment;
		private string _commentLogist;
		private string _clientPhone;
		private string _sumDifferenceReason;
		private bool _shipped;
		private bool _collectBottles;
		private string _code1c;
		private string _address1c;
		private string _address1cCode;
		private string _toClientText;
		private string _fromClientText;
		private int? _dailyNumber;
		private DateTime _lastEditedTime;
		private string _commentManager;
		private string _driverMobileAppComment;
		private DateTime? _driverMobileAppCommentTime;
		private int? _returnedTare;
		private string _informationOnTara;
		private bool _isBottleStock;
		private bool _isBottleStockDiscrepancy;
		private bool _isSelfDeliveryPaid;
		private int _bottlesByStockCount;
		private int _bottlesByStockActualCount;
		private int? _driverCallId;
		private int? _trifle;
		private int? _onlinePaymentNumber;
		private int? _eShopOrder;
		private string _counterpartyExternalOrderId;
		private bool _isContractCloser;
		private bool _isTareNonReturnReasonChangedByUser;
		private bool _hasCommentForDriver;
		private bool _addCertificates;
		private bool _contactlessDelivery;
		private bool _isCopiedFromUndelivery;
		private TimeSpan? _waitUntilTime;
		private bool _dontArriveBeforeInterval;
		private bool _isFastDelivery;
		private bool _selfDelivery;
		private OrderStatus _orderStatus;
		private OrderPaymentStatus _orderPaymentStatus;
		private OrderAddressType _orderAddressType;
		private OrderSignatureType? _signatureType;
		private PaymentByTerminalSource? _paymentByTerminalSource;
		private DriverCallType _driverCallType;
		private OrderSource _orderSource = OrderSource.VodovozApp;
		private DefaultDocumentType? _documentType;
		private DateTime? _deliveryDate;
		private PaymentFromEntity _paymentByCardFrom;
		private PaymentType _paymentType;
		private CounterpartyEntity _client;
		private DeliveryPointEntity _deliveryPoint;
		private CounterpartyContractEntity _contract;
		private DeliveryScheduleEntity _deliverySchedule;
		private string _orderPartsIds;
		
		private IObservableList<OrderItemEntity> _orderItems = new ObservableList<OrderItemEntity>();
		private IList<OrderDocumentEntity> _orderDocuments = new List<OrderDocumentEntity>();
		private IObservableList<OrderDepositItemEntity> _orderDepositItems = new ObservableList<OrderDepositItemEntity>();

		public virtual IUnitOfWork UoW { set; get; }

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Версия")]
		public virtual DateTime Version
		{
			get => _version;
			set => SetField(ref _version, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime? CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		[Display(Name = "Первый заказ")]
		public virtual bool IsFirstOrder
		{
			get => _isFirstOrder;
			set => SetField(ref _isFirstOrder, value);
		}

		[Display(Name = "Второй заказ клиента")]
		public virtual bool IsSecondOrder
		{
			get => _isSecondOrder;
			set => SetField(ref _isSecondOrder, value);
		}

		[Display(Name = "Время доставки")]
		public virtual DateTime? TimeDelivered
		{
			get => _timeDelivered;
			set => SetField(ref _timeDelivered, value);
		}

		[Display(Name = "Первичная дата доставки")]
		[HistoryDateOnly]
		public virtual DateTime? FirstDeliveryDate
		{
			get => _firstDeliveryDate;
			set => SetField(ref _firstDeliveryDate, value);
		}

		[Display(Name = "Дата счета")]
		[HistoryDateOnly]
		public virtual DateTime BillDate
		{
			get => _billDate;
			set => SetField(ref _billDate, value);
		}

		[Display(Name = "Время доставки из 1С")]
		public virtual string DeliverySchedule1c
		{
			get => string.IsNullOrWhiteSpace(_deliverySchedule1c)
				  ? "Время доставки из 1С не загружено"
				  : _deliverySchedule1c;
			set => SetField(ref _deliverySchedule1c, value);
		}

		[Display(Name = "Оплата после отгрузки")]
		public virtual bool PayAfterShipment
		{
			get => _payAfterShipment;
			set => SetField(ref _payAfterShipment, value);
		}

		[Display(Name = "Комментарий ОДЗ")]
		public virtual string ODZComment
		{
			get => _oDZComment;
			set => SetField(ref _oDZComment, value);
		}

		#region OPComment

		[Display(Name = "Комментарий ОП")]
		public virtual string OPComment
		{
			get => _oPComment;
			set => SetField(ref _oPComment, value);
		}

		[Display(Name = "Последнее изменение комментария менеджера")]
		public virtual DateTime? CommentOPManagerUpdatedAt
		{
			get => _commentOPManagerUpdatedAt;
			set => SetField(ref _commentOPManagerUpdatedAt, value);
		}

		#endregion

		[Display(Name = "Бутылей на возврат")]
		public virtual int? BottlesReturn
		{
			get => _bottlesReturn;
			set => SetField(ref _bottlesReturn, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Отзвон за")]
		public virtual int? CallBeforeArrivalMinutes
		{
			get => _callBeforeArrivalMinutes;
			set => SetField(ref _callBeforeArrivalMinutes, value);
		}

		[Display(Name = "Отзвон не нужен")]
		public virtual bool? IsDoNotMakeCallBeforeArrival
		{
			get => _isDoNotMakeCallBeforeArrival;
			set => SetField(ref _isDoNotMakeCallBeforeArrival, value);
		}

		[Display(Name = "Комментарий логиста")]
		public virtual string CommentLogist
		{
			get => _commentLogist;
			set => SetField(ref _commentLogist, value);
		}

		[Display(Name = "Номер телефона")]
		public virtual string ClientPhone
		{
			get => _clientPhone;
			set => SetField(ref _clientPhone, value);
		}

		[Display(Name = "Причина переплаты/недоплаты")]
		public virtual string SumDifferenceReason
		{
			get => _sumDifferenceReason;
			set => SetField(ref _sumDifferenceReason, value);
		}

		[Display(Name = "Отгружено по платежке")]
		public virtual bool Shipped
		{
			get => _shipped;
			set => SetField(ref _shipped, value);
		}

		public virtual bool CollectBottles
		{
			get => _collectBottles;
			set => SetField(ref _collectBottles, value);
		}

		[Display(Name = "Код 1С")]
		public virtual string Code1c
		{
			get => _code1c;
			set => SetField(ref _code1c, value);
		}

		[Display(Name = "Адрес 1С")]
		public virtual string Address1c
		{
			get => _address1c;
			set => SetField(ref _address1c, value);
		}

		[Display(Name = "Код адреса 1С")]
		public virtual string Address1cCode
		{
			get => _address1cCode;
			set => SetField(ref _address1cCode, value);
		}

		[Display(Name = "Оборудование к клиенту")]
		public virtual string ToClientText
		{
			get => _toClientText;
			set => SetField(ref _toClientText, value);
		}

		[Display(Name = "Оборудование от клиента")]
		public virtual string FromClientText
		{
			get => _fromClientText;
			set => SetField(ref _fromClientText, value);
		}

		/// <summary>
		/// Уникальный номер в пределах одного дня
		/// </summary>
		[Display(Name = "Ежедневный номер")]
		public virtual int? DailyNumber
		{
			get => _dailyNumber;
			set => SetField(ref _dailyNumber, value);
		}

		[Display(Name = "Последние изменения")]
		[IgnoreHistoryTrace]
		public virtual DateTime LastEditedTime
		{
			get => _lastEditedTime;
			set => SetField(ref _lastEditedTime, value);
		}

		/// <summary>
		/// Комментарий менеджера ответственного за водительский телефон
		/// </summary>
		[Display(Name = "Комментарий менеджера")]
		public virtual string CommentManager
		{
			get => _commentManager;
			set => SetField(ref _commentManager, value);
		}

		[Display(Name = "Комментарий водителя из приложения")]
		public virtual string DriverMobileAppComment
		{
			get => _driverMobileAppComment;
			set => SetField(ref _driverMobileAppComment, value);
		}

		[Display(Name = "Время установки комментария водителя из приложения")]
		public virtual DateTime? DriverMobileAppCommentTime
		{
			get => _driverMobileAppCommentTime;
			set => SetField(ref _driverMobileAppCommentTime, value);
		}

		[Display(Name = "Возвратная тара")]
		public virtual int? ReturnedTare
		{
			get => _returnedTare;
			set => SetField(ref _returnedTare, value);
		}

		[Display(Name = "Информация о таре")]
		public virtual string InformationOnTara
		{
			get => _informationOnTara;
			set => SetField(ref _informationOnTara, value);
		}

		[Display(Name = "Акция \"Бутыль\" ")]
		public virtual bool IsBottleStock
		{
			get => _isBottleStock;
			set => SetField(ref _isBottleStock, value);
		}

		[Display(Name = "Расхождение между кол-вом фактически сданных и ожидаемых бутылей по акции \"Бутыль\"")]
		public virtual bool IsBottleStockDiscrepancy
		{
			get => _isBottleStockDiscrepancy;
			set => SetField(ref _isBottleStockDiscrepancy, value);
		}

		[Display(Name = "Самовывоз оплачен")]
		public virtual bool IsSelfDeliveryPaid
		{
			get => _isSelfDeliveryPaid;
			set => SetField(ref _isSelfDeliveryPaid, value);
		}

		[Display(Name = "Количество бутылей по акции")]
		public virtual int BottlesByStockCount
		{
			get => _bottlesByStockCount;
			set => SetField(ref _bottlesByStockCount, value);
		}

		[Display(Name = "Фактическое количество бутылей по акции")]
		public virtual int BottlesByStockActualCount
		{
			get => _bottlesByStockActualCount;
			set => SetField(ref _bottlesByStockActualCount, value);
		}

		[Display(Name = "Номер звонка водителя")]
		public virtual int? DriverCallId
		{
			get => _driverCallId;
			set => SetField(ref _driverCallId, value);
		}

		[Display(Name = "Сдача с")]
		public virtual int? Trifle
		{
			get => _trifle;
			set => SetField(ref _trifle, value);
		}

		[Display(Name = "Номер онлайн оплаты")]
		public virtual int? OnlinePaymentNumber
		{
			get => _onlinePaymentNumber;
			set => SetField(ref _onlinePaymentNumber, value);
		}

		[Display(Name = "Заказ из интернет магазина")]
		public virtual int? EShopOrder
		{
			get => _eShopOrder;
			set => SetField(ref _eShopOrder, value);
		}

		[Display(Name = "Идентификатор заказа в ИС контрагента")]
		public virtual string CounterpartyExternalOrderId
		{
			get => _counterpartyExternalOrderId;
			set => SetField(ref _counterpartyExternalOrderId, value);
		}

		[Display(Name = "Заказ - закрывашка по контракту?")]
		public virtual bool IsContractCloser
		{
			get => _isContractCloser;
			set => SetField(ref _isContractCloser, value);
		}

		[Display(Name = "Причина невозврата тары указана пользователем")]
		[IgnoreHistoryTrace]
		public virtual bool IsTareNonReturnReasonChangedByUser
		{
			get => _isTareNonReturnReasonChangedByUser;
			set => SetField(ref _isTareNonReturnReasonChangedByUser, value);
		}

		[Display(Name = "Есть комментарий для водителя?")]
		[IgnoreHistoryTrace]
		public virtual bool HasCommentForDriver
		{
			get => _hasCommentForDriver;
			set => SetField(ref _hasCommentForDriver, value);
		}

		[Display(Name = "Добавить сертификаты продукции")]
		public virtual bool AddCertificates
		{
			get => _addCertificates;
			set => SetField(ref _addCertificates, value);
		}

		[Display(Name = "Бесконтактная доставка")]
		public virtual bool ContactlessDelivery
		{
			get => _contactlessDelivery;
			set => SetField(ref _contactlessDelivery, value);
		}

		[Display(Name = "Перенос из недовоза")]
		public virtual bool IsCopiedFromUndelivery
		{
			get => _isCopiedFromUndelivery;
			set => SetField(ref _isCopiedFromUndelivery, value);
		}

		[Display(Name = "Ожидает до")]
		public virtual TimeSpan? WaitUntilTime
		{
			get => _waitUntilTime;
			set => SetField(ref _waitUntilTime, value);
		}

		[Display(Name = "Не приезжать раньше интервала")]
		public virtual bool DontArriveBeforeInterval
		{
			get => _dontArriveBeforeInterval;
			set => SetField(ref _dontArriveBeforeInterval, value);
		}

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

		[Display(Name = "Самовывоз")]
		public virtual bool SelfDelivery
		{
			get => _selfDelivery;
			set
			{
				if(SetField(ref _selfDelivery, value) && value)
				{
					IsContractCloser = false;
					CallBeforeArrivalMinutes = null;
				}
			}
		}

		[Display(Name = "Статус заказа")]
		[OrderTracker1c]
		public virtual OrderStatus OrderStatus
		{
			get => _orderStatus;
			set => SetField(ref _orderStatus, value);
		}

		[Display(Name = "Статус оплаты заказа")]
		public virtual OrderPaymentStatus OrderPaymentStatus
		{
			get => _orderPaymentStatus;
			set => SetField(ref _orderPaymentStatus, value);
		}

		[Display(Name = "Тип доставки заказа")]
		public virtual OrderAddressType OrderAddressType
		{
			get => _orderAddressType;
			set => SetField(ref _orderAddressType, value);
		}

		[Display(Name = "Подписание документов")]
		public virtual OrderSignatureType? SignatureType
		{
			get => _signatureType;
			set => SetField(ref _signatureType, value);
		}

		[Display(Name = "Подтип оплаты по терминалу")]
		public virtual PaymentByTerminalSource? PaymentByTerminalSource
		{
			get => _paymentByTerminalSource;
			set => SetField(ref _paymentByTerminalSource, value);
		}

		[Display(Name = "Водитель отзвонился")]
		public virtual DriverCallType DriverCallType
		{
			get => _driverCallType;
			set => SetField(ref _driverCallType, value);
		}

		[Display(Name = "Источник заказа")]
		public virtual OrderSource OrderSource
		{
			get => _orderSource;
			set => SetField(ref _orderSource, value);
		}

		[Display(Name = "Тип безналичных документов")]
		public virtual DefaultDocumentType? DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}

		[Display(Name = "Дата доставки")]
		[HistoryDateOnly]
		[OrderTracker1c]
		public virtual DateTime? DeliveryDate
		{
			get => _deliveryDate;
			//Нельзя устанавливать, см. логику в Order.cs
			protected set => SetField(ref _deliveryDate, value);
		}

		[Display(Name = "Место, откуда проведена оплата")]
		public virtual PaymentFromEntity PaymentByCardFrom
		{
			get => _paymentByCardFrom;
			//Нельзя устанавливать, см. логику в Order.cs
			protected set => SetField(ref _paymentByCardFrom, value);
		}

		[Display(Name = "Форма оплаты")]
		public virtual PaymentType PaymentType
		{
			get => _paymentType;
			//Нельзя устанавливать, см. логику в Order.cs
			protected set => SetField(ref _paymentType, value);
		}

		[Display(Name = "Строки заказа")]
		[OrderTracker1c]
		public virtual IObservableList<OrderItemEntity> OrderItems
		{
			get => _orderItems;
			set => SetField(ref _orderItems, value);
		}

		[Display(Name = "Документы заказа")]
		public virtual IList<OrderDocumentEntity> OrderDocuments
		{
			get => _orderDocuments;
			set => SetField(ref _orderDocuments, value);
		}

		[Display(Name = "Залоги заказа")]
		public virtual IObservableList<OrderDepositItemEntity> OrderDepositItems
		{
			get => _orderDepositItems;
			set => SetField(ref _orderDepositItems, value);
		}
		
		[Display(Name = "Клиент")]
		[OrderTracker1c]
		public virtual CounterpartyEntity Client
		{
			get => _client;
			//Нельзя устанавливать, см. логику в Order.cs
			protected set => SetField(ref _client, value);
		}

		[Display(Name = "Точка доставки")]
		public virtual DeliveryPointEntity DeliveryPoint
		{
			get => _deliveryPoint;
			//Нельзя устанавливать, см. логику в Order.cs
			protected set => SetField(ref _deliveryPoint, value);
		}

		[Display(Name = "Договор")]
		[OrderTracker1c]
		public virtual CounterpartyContractEntity Contract
		{
			get => _contract;
			set => SetField(ref _contract, value);
		}

		/// <summary>
		/// Время доставки
		/// </summary>
		[Display(Name = "Время доставки")]
		public virtual DeliveryScheduleEntity DeliverySchedule
		{
			get => _deliverySchedule;
			//Нельзя устанавливать, см. логику в Order.cs
			protected set => SetField(ref _deliverySchedule, value);
		}
		
		/// <summary>
		/// Id частей заказа
		/// </summary>
		[Display(Name = "Id частей заказа")]
		public virtual string OrderPartsIds
		{
			get => _orderPartsIds;
			set => SetField(ref _orderPartsIds, value);
		}

		#region Вычисляемые свойства

		public virtual bool IsUndeliveredStatus =>
			OrderStatus == OrderStatus.Canceled
			|| OrderStatus == OrderStatus.DeliveryCanceled
			|| OrderStatus == OrderStatus.NotDelivered;

		public virtual bool IsLoadedFrom1C => !string.IsNullOrEmpty(Code1c);
		
		/// <summary>
		/// Проверка, является ли целью покупки заказа - для перепродажи
		/// </summary>
		public virtual bool IsOrderForResale =>
			Client?.ReasonForLeaving == ReasonForLeaving.Resale;
		
		/// <summary>
		/// Проверка, является ли целью покупки заказа - госзакупки
		/// </summary>
		public virtual bool IsOrderForTender =>
			Client?.ReasonForLeaving == ReasonForLeaving.Tender;

		/// <summary>
		/// Проверка на госзаказ
		/// и нужно ли собирать данный заказ отдельно при отгрузке со склада
		/// (сканировать марки на складе для отправки документов в статусе заказа "В Пути")
		/// </summary>
		public virtual bool IsNeedIndividualSetOnLoadForTender =>
			IsOrderForTender
			&& Client?.OrderStatusForSendingUpd == OrderStatusForSendingUpd.EnRoute
			&& PaymentType == PaymentType.Cashless;

		/// <summary>
		/// Документооборот по ЭДО с клиентом по заказу осуществляется по новой схеме
		/// </summary>
		public virtual bool IsClientWorksWithNewEdoProcessing =>
			Client?.IsNewEdoProcessing ?? false;
			
		/// <summary>
		/// Полная сумма заказа
		/// </summary>
		public virtual decimal OrderSum => OrderPositiveSum - OrderNegativeSum;

		/// <summary>
		/// Вся положительная сумма заказа
		/// </summary>
		public virtual decimal OrderPositiveSum
		{
			get
			{
				decimal sum = 0;
				foreach(OrderItemEntity item in OrderItems)
				{
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
				foreach(OrderItemEntity item in OrderItems)
				{
					sum += item.Sum;
				}
				return sum;
			}
		}

		/// <summary>
		/// Вся отрицательная сумма заказа
		/// </summary>
		public virtual decimal OrderNegativeSum
		{
			get
			{
				decimal sum = 0;
				foreach(OrderDepositItemEntity dep in OrderDepositItems)
				{
					sum += dep.ActualSum;
				}
				return sum;
			}
		}

		#endregion Вычисляемые свойства
		
		/// <summary>
		/// Проверка, является ли клиент по заказу сетевым покупателем
		/// и нужно ли собирать данный заказ отдельно при отгрузке со склада
		/// </summary>
		public virtual bool IsNeedIndividualSetOnLoad(ICounterpartyEdoAccountEntityController edoAccountController)
		{
			if(Client is null)
			{
				return false;
			}
			
			var edoAccount = edoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(Client, Contract?.Organization?.Id);
			
			return PaymentType == PaymentType.Cashless
				&& Client.OrderStatusForSendingUpd == OrderStatusForSendingUpd.EnRoute
				&& edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree;
		}

		/// <summary>
		/// Является ли заказ безналичным и организация по договору без НДС
		/// </summary>
		public virtual bool IsCashlessPaymentTypeAndOrganizationWithoutVAT => PaymentType == PaymentType.Cashless
			&& Contract?.Organization?.GetActualVatRateVersion(BillDate)?.VatRate.VatRateValue == 0;

		public override string ToString()
		{
			if(IsLoadedFrom1C)
			{
				return string.Format("Заказ №{0}({1})", Id, Code1c);
			}
			else
			{
				return string.Format("Заказ №{0}", Id);
			} 
		}
	}
}

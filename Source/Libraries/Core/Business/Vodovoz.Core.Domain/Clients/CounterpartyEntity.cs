using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Domain.Client;
using VodovozInfrastructure.Attributes;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Контрагент
	/// </summary>
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "контрагенты",
			Nominative = "контрагент",
			Accusative = "контрагента",
			Genitive = "контрагента",
			GenitivePlural = "контрагентов"
		)
	]
	[HistoryTrace]
	[EntityPermission]
	public class CounterpartyEntity : AccountOwnerBase, IDomainObject, IHasAttachedFilesInformations<CounterpartyFileInformation>
	{
		private int _id;
		private OrderStatusForSendingUpd _orderStatusForSendingUpd;
		private bool _isNewEdoProcessing = true;

		private bool _roboatsExclude;
		private bool _isForSalesDepartment;
		private ReasonForLeaving _reasonForLeaving;
		private bool _isPaperlessWorkflow;
		private bool _isNotSendDocumentsByEdo;
		private bool _isNotSendEquipmentTransferByEdo;
		private bool _canSendUpdInAdvance;
		private RegistrationInChestnyZnakStatus _registrationInChestnyZnakStatus;
		private string _specialContractName;
		private string _specialContractNumber;
		private DateTime? _specialContractDate;
		private bool _doNotMixMarkedAndUnmarkedGoodsInOrder;
		private string _patronymic;
		private string _firstName;
		private string _surname;
		private bool _needSendBillByEdo;
		private bool _isDeliveriesClosed;
		private string _closeDeliveryComment;
		private DateTime? _closeDeliveryDate;
		private DebtType? _closeDeliveryDebtType;
		private decimal _maxCredit;
		private string _name;
		private string _typeOfOwnership;
		private string _fullName;
		private int _vodovozInternalId;
		private string _code1c;
		private string _comment;
		private string _iNN;
		private string _kPP;
		private string _oGRN;
		private DateTime? _ogrnDate;
		private string _jurAddress;
		private string _address;
		private PaymentType _paymentMethod;
		private PersonType _personType;
		private bool _isArchive;
		private string _ringUpPhone;
		private DefaultDocumentType? _defaultDocumentType;
		private bool _newBottlesNeeded;
		private string _signatoryFIO;
		private string _signatoryPost;
		private string _signatoryBaseOf;
		private string _phoneFrom1c;
		private TaxType _taxType;
		private DateTime? _createDate = DateTime.Now;
		private bool _useSpecialDocFields;
		private bool _alwaysPrintInvoice;
		private bool _specialExpireDatePercentCheck;
		private decimal _specialExpireDatePercent;
		private string _payerSpecialKPP;
		private string _cargoReceiver;
		private string _specialCustomer;
		private string _govContract;
		private string _specialDeliveryAddress;
		private int? _tTNCount;
		private int? _torg2Count;
		private int? _uPDCount;
		private int? _allUPDCount;
		private int? _torg12Count;
		private int? _shetFacturaCount;
		private int? _carProxyCount;
		private string _oKPO;
		private string _oKDP;
		private CargoReceiverSource _cargoReceiverSource;
		private int _delayDaysForProviders;
		private int _delayDaysForBuyers;
		private bool _isChainStore;
		private bool _isForRetail;
		private bool _noPhoneCall;
		private int _technicalProcessingDelay;
		private CounterpartyType _counterpartyType;
		private bool _alwaysSendReceipts;
		private bool _sendBillByEdo;
		private bool _excludeFromAutoCalls;
		private bool _hideDeliveryPointForBill;
		private RevenueStatus? _revenueStatus;
		private DateTime? _revenueStatusDate;

		private OrganizationEntity _worksThroughOrganization;
		private IList<NomenclatureFixedPriceEntity> _nomenclatureFixedPrices = new List<NomenclatureFixedPriceEntity>();
		private IObservableList<CounterpartyFileInformation> _attachedFileInformations = new ObservableList<CounterpartyFileInformation>();
		private IObservableList<EmailEntity> _emails = new ObservableList<EmailEntity>();
		private IObservableList<PhoneEntity> _phones = new ObservableList<PhoneEntity>();
		private IObservableList<SpecialNomenclature> _specialNomenclatures = new ObservableList<SpecialNomenclature>();
		private IObservableList<CounterpartyEdoAccountEntity> _counterpartyEdoAccounts = new ObservableList<CounterpartyEdoAccountEntity>();

		/// <summary>
		/// Идентификатор<br/>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set
			{
				if(SetField(ref _id, value))
				{
					UpdateFileInformations();
				}
			}
		}

		/// <summary>
		/// Состояние заказа для отправки УПД
		/// </summary>
		[Display(Name = "Статус заказа для отправки УПД")]
		public virtual OrderStatusForSendingUpd OrderStatusForSendingUpd
		{
			get => _orderStatusForSendingUpd;
			set => SetField(ref _orderStatusForSendingUpd, value);
		}

		/// <summary>
		/// Документооборот по ЭДО с клиентом осуществляется по новой схеме<br/>
		/// Работа с ЭДО по новой схеме
		/// </summary>
		[Display(Name = "Работа с ЭДО по новой схеме")]
		public virtual bool IsNewEdoProcessing
		{
			get => _isNewEdoProcessing;
			set => SetField(ref _isNewEdoProcessing, value);
		}

		/// <summary>
		/// Фиксированные цены
		/// </summary>
		[Display(Name = "Фиксированные цены")]
		public virtual IList<NomenclatureFixedPriceEntity> NomenclatureFixedPrices
		{
			get => _nomenclatureFixedPrices;
			set => SetField(ref _nomenclatureFixedPrices, value);
		}

		/// <summary>
		/// Работает через организацию
		/// </summary>
		[Display(Name = "Работает через организацию")]
		public virtual OrganizationEntity WorksThroughOrganization
		{
			get => _worksThroughOrganization;
			set => SetField(ref _worksThroughOrganization, value);
		}

		#region CloseDelivery

		/// <summary>
		/// Поставки закрыты
		/// </summary>
		[Display(Name = "Поставки закрыты?")]
		public virtual bool IsDeliveriesClosed
		{
			get => _isDeliveriesClosed;
			protected set => SetField(ref _isDeliveriesClosed, value);
		}

		/// <summary>
		/// Комментарий по закрытию поставок
		/// </summary>
		[Display(Name = "Комментарий по закрытию поставок")]
		public virtual string CloseDeliveryComment
		{
			get => _closeDeliveryComment;
			set => SetField(ref _closeDeliveryComment, value);
		}

		/// <summary>
		/// Дата закрытия поставок
		/// </summary>
		[Display(Name = "Дата закрытия поставок")]
		public virtual DateTime? CloseDeliveryDate
		{
			get => _closeDeliveryDate;
			protected set => SetField(ref _closeDeliveryDate, value);
		}

		public virtual DebtType? CloseDeliveryDebtType
		{
			get => _closeDeliveryDebtType;
			set => SetField(ref _closeDeliveryDebtType, value);
		}

		#endregion CloseDelivery

		/// <summary>
		/// Максимальный кредит
		/// </summary>
		[Display(Name = "Максимальный кредит")]
		public virtual decimal MaxCredit
		{
			get => _maxCredit;
			set => SetField(ref _maxCredit, value);
		}

		/// <summary>
		/// Форма собственности
		/// </summary>
		[Display(Name = "Форма собственности")]
		[StringLength(20)]
		public virtual string TypeOfOwnership
		{
			get => _typeOfOwnership;
			set => SetField(ref _typeOfOwnership, value);
		}

		/// <summary>
		/// Название
		/// </summary>
		[Required(ErrorMessage = "Название контрагента должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set
			{
				if(SetField(ref _name, value) && PersonType == PersonType.natural)
				{
					FullName = Name;
				}
			}
		}

		/// <summary>
		/// Полное название
		/// </summary>
		[Display(Name = "Полное название")]
		public virtual string FullName
		{
			get => _fullName;
			set => SetField(ref _fullName, value);
		}

		public virtual string Code1c
		{
			get => _code1c;
			set => SetField(ref _code1c, value);
		}

		/// <summary>
		/// Комментарий
		/// </summary>
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// ИНН
		/// </summary>
		[Display(Name = "ИНН")]
		public virtual string INN
		{
			get => _iNN;
			set => SetField(ref _iNN, value);
		}

		/// <summary>
		/// КПП
		/// </summary>
		[Display(Name = "КПП")]
		public virtual string KPP
		{
			get => _kPP;
			set => SetField(ref _kPP, value);
		}
		
		/// <summary>
		/// Статус в налоговой
		/// </summary>
		[Display(Name = "Статус в налоговой")]
		public virtual RevenueStatus? RevenueStatus
		{
			get => _revenueStatus;
			set => SetField(ref _revenueStatus, value);
		}
		
		/// <summary>
		/// Дата статуса в налоговой
		/// </summary>
		[Display(Name = "Дата статуса в налоговой")]
		public virtual DateTime? RevenueStatusDate
		{
			get => _revenueStatusDate;
			set => SetField(ref _revenueStatusDate, value);
		}

		/// <summary>
		/// ОГРН/ОГРНИП
		/// </summary>
		[Display(Name = "ОГРН/ОГРНИП")]
		public virtual string OGRN
		{
			get => _oGRN;
			set => SetField(ref _oGRN, value);
		}
		
		/// <summary>
		/// Дата ОГРН/ОГРНИП
		/// </summary>
		[Display(Name = "Дата внесения ОГРН/ОГРНИП")]
		public virtual DateTime? OGRNDate
		{
			get => _ogrnDate;
			set => SetField(ref _ogrnDate, value);
		}

		/// <summary>
		/// Юридический адрес
		/// </summary>
		[Display(Name = "Юридический адрес")]
		public virtual string JurAddress
		{
			get => _jurAddress;
			set => SetField(ref _jurAddress, value);
		}

		/// <summary>
		/// Фактический адрес
		/// </summary>
		[Display(Name = "Фактический адрес")]
		public virtual string Address
		{
			get => _address;
			set => SetField(ref _address, value);
		}

		/// <summary>
		/// Форма контрагента
		/// </summary>
		[Display(Name = "Форма контрагента")]
		public virtual PersonType PersonType
		{
			get => _personType;
			set
			{
				SetField(ref _personType, value);

				if(value == PersonType.natural)
				{
					PaymentMethod = PaymentType.Cash;
				}
			}
		}

		/// <summary>
		/// Вид оплаты
		/// </summary>
		[Display(Name = "Вид оплаты")]
		public virtual PaymentType PaymentMethod
		{
			get => _paymentMethod;
			set => SetField(ref _paymentMethod, value);
		}

		/// <summary>
		/// Архивный
		/// </summary>
		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		/// <summary>
		/// Телефон для обзвона
		/// </summary>
		[Display(Name = "Телефон для обзвона")]
		public virtual string RingUpPhone
		{
			get => _ringUpPhone;
			set => SetField(ref _ringUpPhone, value);
		}

		/// <summary>
		/// Тип безналичных документов по-умолчанию
		/// </summary>
		[Display(Name = "Тип безналичных документов по-умолчанию")]
		public virtual DefaultDocumentType? DefaultDocumentType
		{
			get => _defaultDocumentType;
			set => SetField(ref _defaultDocumentType, value);
		}

		/// <summary>
		/// Новая необоротная тара
		/// </summary>
		[Display(Name = "Новая необоротная тара")]
		public virtual bool NewBottlesNeeded
		{
			get => _newBottlesNeeded;
			set => SetField(ref _newBottlesNeeded, value);
		}

		/// <summary>
		/// ФИО подписанта
		/// </summary>
		[Display(Name = "ФИО подписанта")]
		public virtual string SignatoryFIO
		{
			get => _signatoryFIO;
			set => SetField(ref _signatoryFIO, value);
		}

		/// <summary>
		/// Должность подписанта
		/// </summary>
		[Display(Name = "Должность подписанта")]
		public virtual string SignatoryPost
		{
			get => _signatoryPost;
			set => SetField(ref _signatoryPost, value);
		}

		/// <summary>
		/// На основании
		/// </summary>
		[Display(Name = "На основании")]
		public virtual string SignatoryBaseOf
		{
			get => _signatoryBaseOf;
			set => SetField(ref _signatoryBaseOf, value);
		}

		/// <summary>
		/// Телефон
		/// </summary>
		[Display(Name = "Телефон")]
		public virtual string PhoneFrom1c
		{
			get => _phoneFrom1c;
			set => SetField(ref _phoneFrom1c, value);
		}

		/// <summary>
		/// Налогообложение
		/// </summary>
		[Display(Name = "Налогобложение")]
		public virtual TaxType TaxType
		{
			get => _taxType;
			set => SetField(ref _taxType, value);
		}

		/// <summary>
		/// Дата создания
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		#region ОсобаяПечать

		/// <summary>
		/// Особая печать документов
		/// </summary>
		[Display(Name = "Особая печать документов")]
		public virtual bool UseSpecialDocFields
		{
			get => _useSpecialDocFields;
			set => SetField(ref _useSpecialDocFields, value);
		}

		/// <summary>
		/// Всегда печатать накладную
		/// </summary>
		[Display(Name = "Всегда печатать накладную")]
		public virtual bool AlwaysPrintInvoice
		{
			get => _alwaysPrintInvoice;
			set => SetField(ref _alwaysPrintInvoice, value);
		}

		#region Особое требование срок годности

		/// <summary>
		/// Особое требование: требуется срок годности
		/// </summary>
		[Display(Name = "Особое требование: требуется срок годности")]
		public virtual bool SpecialExpireDatePercentCheck
		{
			get => _specialExpireDatePercentCheck;
			set => SetField(ref _specialExpireDatePercentCheck, value);
		}

		/// <summary>
		/// Особое требование: срок годности %
		/// </summary>
		[Display(Name = "Особое требование: срок годности %")]
		public virtual decimal SpecialExpireDatePercent
		{
			get => _specialExpireDatePercent;
			set => SetField(ref _specialExpireDatePercent, value);
		}

		#endregion Особое требование срок годности

		/// <summary>
		/// Название особого договора
		/// </summary>
		[Display(Name = "Название особого договора")]
		public virtual string SpecialContractName
		{
			get => _specialContractName;
			set => SetField(ref _specialContractName, value);
		}

		/// <summary>
		/// Номер особого договора
		/// </summary>
		[Display(Name = "Номер особого договора")]
		public virtual string SpecialContractNumber
		{
			get => _specialContractNumber;
			set => SetField(ref _specialContractNumber, value);
		}

		/// <summary>
		/// Дата особого договора
		/// </summary>
		[Display(Name = "Дата особого договора")]
		public virtual DateTime? SpecialContractDate
		{
			get => _specialContractDate;
			set => SetField(ref _specialContractDate, value);
		}

		/// <summary>
		/// Особый КПП плательщика
		/// </summary>
		[Display(Name = "Особый КПП плательщика")]
		public virtual string PayerSpecialKPP
		{
			get => _payerSpecialKPP;
			set => SetField(ref _payerSpecialKPP, value);
		}

		/// <summary>
		/// Грузополучатель
		/// </summary>
		[Display(Name = "Грузополучатель")]
		public virtual string CargoReceiver
		{
			get => _cargoReceiver;
			set => SetField(ref _cargoReceiver, value);
		}

		/// <summary>
		/// Особый покупатель
		/// </summary>
		[Display(Name = "Особый покупатель")]
		public virtual string SpecialCustomer
		{
			get => _specialCustomer;
			set => SetField(ref _specialCustomer, value);
		}

		/// <summary>
		/// Идентификатор государственного контракта
		/// </summary>
		[Display(Name = "Идентификатор государственного контракта")]
		public virtual string GovContract
		{
			get => _govContract;
			set => SetField(ref _govContract, value);
		}

		/// <summary>
		/// Особый адрес доставки
		/// </summary>
		[Display(Name = "Особый адрес доставки")]
		public virtual string SpecialDeliveryAddress
		{
			get => _specialDeliveryAddress;
			set => SetField(ref _specialDeliveryAddress, value);
		}

		/// <summary>
		/// Кол-во ТТН
		/// </summary>
		[Display(Name = "Кол-во ТТН")]
		public virtual int? TTNCount
		{
			get => _tTNCount;
			set => SetField(ref _tTNCount, value);
		}

		/// <summary>
		/// Кол-во Торг-2
		/// </summary>
		[Display(Name = "Кол-во Торг-2")]
		public virtual int? Torg2Count
		{
			get => _torg2Count;
			set => SetField(ref _torg2Count, value);
		}

		/// <summary>
		/// Кол-во УПД(не для безнала)
		/// </summary>
		[Display(Name = "Кол-во УПД(не для безнала)")]
		public virtual int? UPDCount
		{
			get => _uPDCount;
			set => SetField(ref _uPDCount, value);
		}

		/// <summary>
		/// Кол-во УПД
		/// </summary>
		[Display(Name = "Кол-во УПД")]
		public virtual int? AllUPDCount
		{
			get => _allUPDCount;
			set => SetField(ref _allUPDCount, value);
		}

		/// <summary>
		/// Кол-во Торг-12
		/// </summary>
		[Display(Name = "Кол-во Торг-12")]
		public virtual int? Torg12Count
		{
			get => _torg12Count;
			set => SetField(ref _torg12Count, value);
		}

		/// <summary>
		/// Кол-во отчет-фактур
		/// </summary>
		[Display(Name = "Кол-во отчет-фактур")]
		public virtual int? ShetFacturaCount
		{
			get => _shetFacturaCount;
			set => SetField(ref _shetFacturaCount, value);
		}

		/// <summary>
		/// Кол-во доверенностей вод-ль
		/// </summary>
		[Display(Name = "Кол-во доверенностей вод-ль")]
		public virtual int? CarProxyCount
		{
			get => _carProxyCount;
			set => SetField(ref _carProxyCount, value);
		}

		/// <summary>
		/// ОКПО
		/// </summary>
		[Display(Name = "ОКПО")]
		public virtual string OKPO
		{
			get => _oKPO;
			set => SetField(ref _oKPO, value);
		}

		/// <summary>
		/// ОКПД
		/// </summary>
		[Display(Name = "ОКДП")]
		public virtual string OKDP
		{
			get => _oKDP;
			set => SetField(ref _oKDP, value);
		}

		/// <summary>
		/// Источник грузополучателя
		/// </summary>
		[Display(Name = "Источник грузополучателя")]
		public virtual CargoReceiverSource CargoReceiverSource
		{
			get => _cargoReceiverSource;
			set => SetField(ref _cargoReceiverSource, value);
		}

		#endregion ОсобаяПечать

		#region ЭДО и Честный знак

		/// <summary>
		/// Причина выбытия
		/// </summary>
		[Display(Name = "Причина выбытия")]
		public virtual ReasonForLeaving ReasonForLeaving
		{
			get => _reasonForLeaving;
			set => SetField(ref _reasonForLeaving, value);
		}

		/// <summary>
		/// Статус регистрации в Честном Знаке
		/// </summary>
		[Display(Name = "Статус регистрации в Честном Знаке")]
		public virtual RegistrationInChestnyZnakStatus RegistrationInChestnyZnakStatus
		{
			get => _registrationInChestnyZnakStatus;
			set => SetField(ref _registrationInChestnyZnakStatus, value);
		}

		/// <summary>
		/// Отказ от печатных документов
		/// </summary>
		[Display(Name = "Отказ от печатных документов")]
		public virtual bool IsPaperlessWorkflow
		{
			get => _isPaperlessWorkflow;
			set => SetField(ref _isPaperlessWorkflow, value);
		}

		/// <summary>
		/// Не отправлять документы по ЭДО
		/// </summary>
		[Display(Name = "Не отправлять документы по EDO")]
		public virtual bool IsNotSendDocumentsByEdo
		{
			get => _isNotSendDocumentsByEdo;
			set => SetField(ref _isNotSendDocumentsByEdo, value);
		}

		/// <summary>
		/// Не отправлять акт приёма-передачи по ЭДО
		/// </summary>
		[Display(Name = "Не отправлять акт приёма-передачи по ЭДО")]
		public virtual bool IsNotSendEquipmentTransferByEdo
		{
			get => _isNotSendEquipmentTransferByEdo;
			set => SetField(ref _isNotSendEquipmentTransferByEdo, value);
		}

		/// <summary>
		/// Отправлять УПД заранее
		/// </summary>
		[Display(Name = "Отправлять УПД заранее")]
		public virtual bool CanSendUpdInAdvance
		{
			get => _canSendUpdInAdvance;
			set => SetField(ref _canSendUpdInAdvance, value);
		}

		/// <summary>
		/// Фамилия
		/// </summary>
		[Display(Name = "Фамилия")]
		public virtual string Surname
		{
			get => _surname;
			set => SetField(ref _surname, value);
		}

		/// <summary>
		/// Имя
		/// </summary>
		[Display(Name = "Имя")]
		public virtual string FirstName
		{
			get => _firstName;
			set => SetField(ref _firstName, value);
		}

		/// <summary>
		/// Отчество
		/// </summary>
		[Display(Name = "Отчество")]
		public virtual string Patronymic
		{
			get => _patronymic;
			set => SetField(ref _patronymic, value);
		}

		/// <summary>
		/// Не смешивать в одном заказе маркированные и немаркированные товары
		/// </summary>
		[Display(Name = "Не смешивать в одном заказе маркированные и немаркированные товары")]
		public virtual bool DoNotMixMarkedAndUnmarkedGoodsInOrder
		{
			get => _doNotMixMarkedAndUnmarkedGoodsInOrder;
			set => SetField(ref _doNotMixMarkedAndUnmarkedGoodsInOrder, value);
		}

		/// <summary>
		/// Отправлять счета по ЭДО
		/// </summary>
		[Display(Name = "Отправлять счета по ЭДО")]
		public virtual bool NeedSendBillByEdo
		{
			get => _needSendBillByEdo;
			set => SetField(ref _needSendBillByEdo, value);
		}
		
		[Display(Name = "ЭДО аккаунты контрагента")]
		public virtual IObservableList<CounterpartyEdoAccountEntity> CounterpartyEdoAccounts
		{
			get => _counterpartyEdoAccounts;
			set => SetField(ref _counterpartyEdoAccounts, value);
		}

		#endregion ЭДО и Честный знак

		/// <summary>
		/// Отсрочка дней
		/// </summary>
		[Display(Name = "Отсрочка дней")]
		public virtual int DelayDaysForProviders
		{
			get => _delayDaysForProviders;
			set => SetField(ref _delayDaysForProviders, value);
		}

		/// <summary>
		/// Отсрочка дней покупателям
		/// </summary>
		[Display(Name = "Отсрочка дней покупателям")]
		public virtual int DelayDaysForBuyers
		{
			get => _delayDaysForBuyers;
			set => SetField(ref _delayDaysForBuyers, value);
		}

		/// <summary>
		/// Тип контрагента
		/// </summary>
		[Display(Name = "Тип контрагента")]
		public virtual CounterpartyType CounterpartyType
		{
			get => _counterpartyType;
			set => SetField(ref _counterpartyType, value);
		}

		/// <summary>
		/// Сетевой магазин
		/// </summary>
		[Display(Name = "Сетевой магазин")]
		public virtual bool IsChainStore
		{
			get => _isChainStore;
			set => SetField(ref _isChainStore, value);
		}

		/// <summary>
		/// Для розницы
		/// </summary>
		[Display(Name = "Для розницы")]
		public virtual bool IsForRetail
		{
			get => _isForRetail;
			set => SetField(ref _isForRetail, value);
		}

		/// <summary>
		/// Для отдела продаж
		/// </summary>
		[Display(Name = "Для отдела продаж")]
		public virtual bool IsForSalesDepartment
		{
			get => _isForSalesDepartment;
			set => SetField(ref _isForSalesDepartment, value);
		}

		/// <summary>
		/// Без прозвона
		/// </summary>
		[Display(Name = "Без прозвона")]
		public virtual bool NoPhoneCall
		{
			get => _noPhoneCall;
			set => SetField(ref _noPhoneCall, value);
		}

		/// <summary>
		/// Исключение из Roboats звонков
		/// </summary>
		[Display(Name = "Исключение из Roboats звонков")]
		public virtual bool RoboatsExclude
		{
			get => _roboatsExclude;
			set => SetField(ref _roboatsExclude, value);
		}

		/// <summary>
		/// Отказ от автообзвонов
		/// </summary>
		[Display(Name = "Отказ от автообзвонов")]
		public virtual bool ExcludeFromAutoCalls
		{
			get => _excludeFromAutoCalls;
			set => SetField(ref _excludeFromAutoCalls, value);
		}

		/// <summary>
		/// Отсрочка технической обработки
		/// </summary>
		[Display(Name = "Отсрочка технической обработки")]
		public virtual int TechnicalProcessingDelay
		{
			get => _technicalProcessingDelay;
			set => SetField(ref _technicalProcessingDelay, value);
		}

		/// <summary>
		/// Всегда отправлять чеки
		/// </summary>
		[RestrictedHistoryProperty]
		[IgnoreHistoryTrace]
		[Display(Name = "Всегда отправлять чеки")]
		public virtual bool AlwaysSendReceipts
		{
			get => _alwaysSendReceipts;
			set => SetField(ref _alwaysSendReceipts, value);
		}

		/// <summary>
		/// Скрывать ТД в счетах
		/// </summary>
		[Display(Name = "Скрывать ТД в счетах")]
		public virtual bool HideDeliveryPointForBill
		{
			get => _hideDeliveryPointForBill;
			set => SetField(ref _hideDeliveryPointForBill, value);
		}

		/// <summary>
		/// Информация о прикрепленных файлах
		/// </summary>
		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<CounterpartyFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

		/// <summary>
		/// E-mail адреса
		/// </summary>
		[Display(Name = "E-mail адреса")]
		public virtual IObservableList<EmailEntity> Emails
		{
			get => _emails;
			set => SetField(ref _emails, value);
		}

		/// <summary>
		/// Телефоны
		/// </summary>
		[Display(Name = "Телефоны")]
		public virtual IObservableList<PhoneEntity> Phones
		{
			get => _phones;
			set => SetField(ref _phones, value);
		}

		/// <summary>
		/// Особенные номера ТМЦ
		/// </summary>
		[Display(Name = "Особенные номера ТМЦ")]
		public virtual IObservableList<SpecialNomenclature> SpecialNomenclatures
		{
			get => _specialNomenclatures;
			set => SetField(ref _specialNomenclatures, value);
		}

		/// <summary>
		/// Добавление информации о прикрепленном файле по имени файла
		/// </summary>
		/// <param name="fileName"></param>
		public virtual void AddFileInformation(string fileName)
		{
			if(AttachedFileInformations.Any(afi => afi.FileName == fileName))
			{
				return;
			}

			AttachedFileInformations.Add(new CounterpartyFileInformation
			{
				FileName = fileName,
				CounterpartyId = Id
			});
		}

		/// <summary>
		/// Удаление информации о прикрепленном файле по имени файла
		/// </summary>
		/// <param name="fileName"></param>
		public virtual void RemoveFileInformation(string fileName)
		{
			AttachedFileInformations.Remove(AttachedFileInformations.FirstOrDefault(afi => afi.FileName == fileName));
		}
		
		public virtual CounterpartyEdoAccountEntity DefaultEdoAccount(int organizationId)
		{
			return CounterpartyEdoAccounts
				.FirstOrDefault(x => x.OrganizationId == organizationId && x.IsDefault);
		}

		public virtual bool LegalAndHasAnyDefaultAccountAgreedForEdo =>
			PersonType == PersonType.legal
			&& CounterpartyEdoAccounts.Any(
				x => x.IsDefault && x.ConsentForEdoStatus == ConsentForEdoStatus.Agree);
		
		/// <summary>
		/// Является ли клиент ИП с незаполненными ОГРНИП или датой ОГРНИП
		/// </summary>
		/// <returns></returns>
		public virtual bool IsPrivateBusinessmanWithoutOgrnOrOgrnDate() =>
			_iNN != null
			&& _iNN.Length == CompanyConstants.PrivateBusinessmanInnLength
			&& (!string.IsNullOrWhiteSpace(_oGRN) || !_ogrnDate.HasValue);

		/// <summary>
		/// Обновление информации о прикрепленных файлах
		/// </summary>
		private void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.CounterpartyId = Id;
			}
		}
	}
}

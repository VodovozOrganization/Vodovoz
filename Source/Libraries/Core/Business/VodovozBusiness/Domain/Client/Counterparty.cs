using Gamma.Utilities;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Services;
using QS.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Retail;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using VodovozInfrastructure.Attributes;

namespace Vodovoz.Domain.Client
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "контрагенты",
			Nominative = "контрагент",
			Accusative = "контрагента",
			Genitive = "контрагента"
		)
	]
	[HistoryTrace]
	[EntityPermission]
	public class Counterparty : AccountOwnerBase, IDomainObject, IValidatableObject, INamed, IArchivable
	{
		//Используется для валидации, не получается истолльзовать бизнес объект так как наследуемся от AccountOwnerBase
		private const int _specialContractNameLimit = 800;
		private const int _cargoReceiverLimitSymbols = 500;

		private bool _roboatsExclude;
		private bool _isForSalesDepartment;
		private ReasonForLeaving _reasonForLeaving;
		private bool _isPaperlessWorkflow;
		private bool _isNotSendDocumentsByEdo;
		private bool _canSendUpdInAdvance;
		private RegistrationInChestnyZnakStatus _registrationInChestnyZnakStatus;
		private OrderStatusForSendingUpd _orderStatusForSendingUpd;
		private ConsentForEdoStatus _consentForEdoStatus;
		private string _personalAccountIdInEdo;
		private EdoOperator _edoOperator;
		private string _specialContractName;
		private string _specialContractNumber;
		private DateTime? _specialContractDate;
		private bool _doNotMixMarkedAndUnmarkedGoodsInOrder;
		private string _patronymic;
		private string _firstName;
		private string _surname;
		private bool _needSendBillByEdo;
		private IList<CounterpartyEdoOperator> _counterpartyEdoOperators = new List<CounterpartyEdoOperator>();
		private GenericObservableList<CounterpartyEdoOperator> _observableCounterpartyEdoOperators;
		private IList<CounterpartyContract> _counterpartyContracts;
		private IList<DeliveryPoint> _deliveryPoints = new List<DeliveryPoint>();
		private GenericObservableList<DeliveryPoint> _observableDeliveryPoints;
		private IList<Tag> _tags = new List<Tag>();
		private GenericObservableList<Tag> _observableTags;
		private IList<Contact> _contact = new List<Contact>();
		private bool _isDeliveriesClosed;
		private string _closeDeliveryComment;
		private DateTime? _closeDeliveryDate;
		private Employee _closeDeliveryPerson;
		private IList<Proxy> _proxies;
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
		private string _jurAddress;
		private string _address;
		private PaymentType _paymentMethod;
		private PersonType _personType;
		private int? _defaultExpenseCategoryId;
		private Counterparty _mainCounterparty;
		private Counterparty _previousCounterparty;
		private bool _isArchive;
		private IList<Phone> _phones = new List<Phone>();
		private GenericObservableList<Phone> _observablePhones;
		private string _ringUpPhone;
		private IList<Email> _emails = new List<Email>();
		private Employee _accountant;
		private Employee _salesManager;
		private Employee _bottlesManager;
		private Contact _mainContact;
		private Contact _financialContact;
		private DefaultDocumentType? _defaultDocumentType;
		private bool _newBottlesNeeded;
		private string _signatoryFIO;
		private string _signatoryPost;
		private string _signatoryBaseOf;
		private string _phoneFrom1c;
		private ClientCameFrom _cameFrom;
		private Order _firstOrder;
		private TaxType _taxType;
		private DateTime? _createDate = DateTime.Now;
		private LogisticsRequirements _logisticsRequirements;
		private bool _useSpecialDocFields;
		private bool _alwaysPrintInvoice;
		private bool _specialExpireDatePercentCheck;
		private decimal _specialExpireDatePercent;
		private string _payerSpecialKPP;
		private string _cargoReceiver;
		private string _customer;
		private string _govContract;
		private string _deliveryAddress;
		private int? _ttnCount;
		private int? _torg2Count;
		private int? _updCount;
		private int? _updAllCount;
		private int? _torg12Count;
		private int? _shetFacturaCount;
		private int? _carProxyCount;
		private string _okpo;
		private string _okdp;
		private CargoReceiverSource _cargoReceiverSource;
		private IList<SpecialNomenclature> _specialNomenclatures = new List<SpecialNomenclature>();
		private GenericObservableList<SpecialNomenclature> _observableSpecialNomenclatures;
		private int _delayDaysForProviders;
		private int _delayDaysForBuyers;
		private CounterpartyType _counterpartyType;
		private bool _isChainStore;
		private bool _isForRetail;
		private bool _noPhoneCall;
		private IList<SalesChannel> _salesChannels = new List<SalesChannel>();
		private GenericObservableList<SalesChannel> _observableSalesChannels;
		private int _technicalProcessingDelay;
		private IList<SupplierPriceItem> _suplierPriceItems = new List<SupplierPriceItem>();
		private GenericObservableList<SupplierPriceItem> _observableSuplierPriceItems;
		private bool _alwaysSendReceipts;
		private IList<NomenclatureFixedPrice> _nomenclatureFixedPrices = new List<NomenclatureFixedPrice>();
		private GenericObservableList<NomenclatureFixedPrice> _observableNomenclatureFixedPrices;
		private IList<CounterpartyFile> _files = new List<CounterpartyFile>();
		private GenericObservableList<CounterpartyFile> _observableFiles;
		private Organization _worksThroughOrganization;
		private IList<ISupplierPriceNode> _priceNodes = new List<ISupplierPriceNode>();
		private GenericObservableList<ISupplierPriceNode> _observablePriceNodes;
		private CounterpartySubtype _counterpartySubtype;
		private bool _isLiquidating;

		#region Свойства

		public virtual IUnitOfWork UoW { get; set; }

		[Display(Name = "Договоры")]
		public virtual IList<CounterpartyContract> CounterpartyContracts
		{
			get => _counterpartyContracts;
			set => SetField(ref _counterpartyContracts, value);
		}

		[Display(Name = "Точки доставки")]
		public virtual IList<DeliveryPoint> DeliveryPoints
		{
			get => _deliveryPoints;
			set => SetField(ref _deliveryPoints, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryPoint> ObservableDeliveryPoints
		{
			get
			{
				if(_observableDeliveryPoints == null)
				{
					_observableDeliveryPoints = new GenericObservableList<DeliveryPoint>(DeliveryPoints);
				}

				return _observableDeliveryPoints;
			}
		}

		[Display(Name = "Теги")]
		public virtual IList<Tag> Tags
		{
			get => _tags;
			set => SetField(ref _tags, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Tag> ObservableTags
		{
			get
			{
				if(_observableTags == null)
				{
					_observableTags = new GenericObservableList<Tag>(Tags);
				}

				return _observableTags;
			}
		}

		[Display(Name = "Контактные лица")]
		public virtual IList<Contact> Contacts
		{
			get => _contact;
			set => SetField(ref _contact, value);
		}

		#region CloseDelivery

		[Display(Name = "Поставки закрыты?")]
		public virtual bool IsDeliveriesClosed
		{
			get => _isDeliveriesClosed;
			protected set => SetField(ref _isDeliveriesClosed, value);
		}

		[Display(Name = "Комментарий по закрытию поставок")]
		public virtual string CloseDeliveryComment
		{
			get => _closeDeliveryComment;
			set => SetField(ref _closeDeliveryComment, value);
		}

		[Display(Name = "Дата закрытия поставок")]
		public virtual DateTime? CloseDeliveryDate
		{
			get => _closeDeliveryDate;
			protected set => SetField(ref _closeDeliveryDate, value);
		}

		[Display(Name = "Сотрудник закрывший поставки")]
		public virtual Employee CloseDeliveryPerson
		{
			get => _closeDeliveryPerson;
			protected set => SetField(ref _closeDeliveryPerson, value);
		}

		#endregion CloseDelivery

		[Display(Name = "Доверенности")]
		public virtual IList<Proxy> Proxies
		{
			get => _proxies;
			set => SetField(ref _proxies, value);
		}

		public virtual int Id { get; set; }

		[Display(Name = "Максимальный кредит")]
		public virtual decimal MaxCredit
		{
			get => _maxCredit;
			set => SetField(ref _maxCredit, value);
		}

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

		[Display(Name = "Форма собственности")]
		[StringLength(20)]
		public virtual string TypeOfOwnership
		{
			get => _typeOfOwnership;
			set => SetField(ref _typeOfOwnership, value);
		}

		[Display(Name = "Полное название")]
		public virtual string FullName
		{
			get => _fullName;
			set => SetField(ref _fullName, value);
		}

		/// <summary>
		/// Генерируется триггером на строне БД.
		/// </summary>
		[Display(Name = "Внутренний номер контрагента")]
		public virtual int VodovozInternalId
		{
			get => _vodovozInternalId;
			set => SetField(ref _vodovozInternalId, value);
		}

		public virtual string Code1c
		{
			get => _code1c;
			set => SetField(ref _code1c, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "ИНН")]
		public virtual string INN
		{
			get => _iNN;
			set => SetField(ref _iNN, value);
		}

		[Display(Name = "КПП")]
		public virtual string KPP
		{
			get => _kPP;
			set => SetField(ref _kPP, value);
		}

		[Display(Name = "Контрагент в статусе ликвидации")]
		public virtual bool IsLiquidating
		{
			get => _isLiquidating;
			set => SetField(ref _isLiquidating, value);
		}

		[Display(Name = "ОГРН")]
		public virtual string OGRN
		{
			get => _oGRN;
			set => SetField(ref _oGRN, value);
		}

		[Display(Name = "Юридический адрес")]
		public virtual string JurAddress
		{
			get => _jurAddress;
			set => SetField(ref _jurAddress, value);
		}

		[Display(Name = "Фактический адрес")]
		public virtual string Address
		{
			get => _address;
			set => SetField(ref _address, value);
		}

		[Display(Name = "Вид оплаты")]
		public virtual PaymentType PaymentMethod
		{
			get => _paymentMethod;
			set => SetField(ref _paymentMethod, value);
		}

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

		[Display(Name = "Расход по-умолчанию")]
		[HistoryIdentifier(TargetType = typeof(FinancialExpenseCategory))]
		public virtual int? DefaultExpenseCategoryId
		{
			get => _defaultExpenseCategoryId;
			set => SetField(ref _defaultExpenseCategoryId, value);
		}

		[Display(Name = "Головная организация")]
		public virtual Counterparty MainCounterparty
		{
			get => _mainCounterparty;
			set => SetField(ref _mainCounterparty, value);
		}

		[Display(Name = "Предыдущий контрагент")]
		public virtual Counterparty PreviousCounterparty
		{
			get => _previousCounterparty;
			set => SetField(ref _previousCounterparty, value);
		}

		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Телефоны")]
		public virtual IList<Phone> Phones
		{
			get => _phones;
			set => SetField(ref _phones, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Phone> ObservablePhones
		{
			get
			{
				if(_observablePhones == null)
				{
					_observablePhones = new GenericObservableList<Phone>(Phones);
				}

				return _observablePhones;
			}
		}

		[Display(Name = "Телефон для обзвона")]
		public virtual string RingUpPhone
		{
			get => _ringUpPhone;
			set => SetField(ref _ringUpPhone, value);
		}

		[Display(Name = "E-mail адреса")]
		public virtual IList<Email> Emails
		{
			get => _emails;
			set => SetField(ref _emails, value);
		}

		[Display(Name = "Все операторы ЭДО контрагента")]
		public virtual IList<CounterpartyEdoOperator> CounterpartyEdoOperators
		{
			get => _counterpartyEdoOperators;
			set => SetField(ref _counterpartyEdoOperators, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CounterpartyEdoOperator> ObservableCounterpartyEdoOperators =>
				_observableCounterpartyEdoOperators ?? (_observableCounterpartyEdoOperators = new GenericObservableList<CounterpartyEdoOperator>(CounterpartyEdoOperators));

		[Display(Name = "Бухгалтер")]
		public virtual Employee Accountant
		{
			get => _accountant;
			set => SetField(ref _accountant, value);
		}

		[Display(Name = "Менеджер по продажам")]
		public virtual Employee SalesManager
		{
			get => _salesManager;
			set => SetField(ref _salesManager, value);
		}

		[Display(Name = "Менеджер по бутылям")]
		public virtual Employee BottlesManager
		{
			get => _bottlesManager;
			set => SetField(ref _bottlesManager, value);
		}

		[Display(Name = "Главное контактное лицо")]
		public virtual Contact MainContact
		{
			get => _mainContact;
			set => SetField(ref _mainContact, value);
		}

		[Display(Name = "Контакт по финансовым вопросам")]
		public virtual Contact FinancialContact
		{
			get => _financialContact;
			set => SetField(ref _financialContact, value);
		}

		[Display(Name = "Тип безналичных документов по-умолчанию")]
		public virtual DefaultDocumentType? DefaultDocumentType
		{
			get => _defaultDocumentType;
			set => SetField(ref _defaultDocumentType, value);
		}

		[Display(Name = "Новая необоротная тара")]
		public virtual bool NewBottlesNeeded
		{
			get => _newBottlesNeeded;
			set => SetField(ref _newBottlesNeeded, value);
		}

		[Display(Name = "ФИО подписанта")]
		public virtual string SignatoryFIO
		{
			get => _signatoryFIO;
			set => SetField(ref _signatoryFIO, value);
		}

		[Display(Name = "Должность подписанта")]
		public virtual string SignatoryPost
		{
			get => _signatoryPost;
			set => SetField(ref _signatoryPost, value);
		}

		[Display(Name = "На основании")]
		public virtual string SignatoryBaseOf
		{
			get => _signatoryBaseOf;
			set => SetField(ref _signatoryBaseOf, value);
		}

		[Display(Name = "Телефон")]
		public virtual string PhoneFrom1c
		{
			get => _phoneFrom1c;
			set => SetField(ref _phoneFrom1c, value);
		}

		[Display(Name = "Откуда клиент")]
		public virtual ClientCameFrom CameFrom
		{
			get => _cameFrom;
			set => SetField(ref _cameFrom, value);
		}
		
		[Display(Name = "Первый заказ")]
		public virtual Order FirstOrder
		{
			get => _firstOrder;
			set => SetField(ref _firstOrder, value);
		}
		
		[Display(Name = "Налогобложение")]
		public virtual TaxType TaxType
		{
			get => _taxType;
			set => SetField(ref _taxType, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime? CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		[Display(Name = "Требования к логистике")]
		public virtual LogisticsRequirements LogisticsRequirements
		{
			get => _logisticsRequirements;
			set => SetField(ref _logisticsRequirements, value);
		}

		#region ОсобаяПечать
		
		[Display(Name = "Особая печать документов")]
		public virtual bool UseSpecialDocFields
		{
			get => _useSpecialDocFields;
			set => SetField(ref _useSpecialDocFields, value);
		}

		[Display(Name = "Всегда печатать накладную")]
		public virtual bool AlwaysPrintInvoice
		{
			get => _alwaysPrintInvoice;
			set => SetField(ref _alwaysPrintInvoice, value);
		}

		#region Особое требование срок годности

		[Display(Name = "Особое требование: требуется срок годности")]
		public virtual bool SpecialExpireDatePercentCheck
		{
			get => _specialExpireDatePercentCheck;
			set => SetField(ref _specialExpireDatePercentCheck, value);
		}

		[Display(Name = "Особое требование: срок годности %")]
		public virtual decimal SpecialExpireDatePercent
		{
			get => _specialExpireDatePercent;
			set => SetField(ref _specialExpireDatePercent, value);
		}

		#endregion Особое требование срок годности

		[Display(Name = "Название особого договора")]
		public virtual string SpecialContractName
		{
			get => _specialContractName;
			set => SetField(ref _specialContractName, value);
		}

		[Display(Name = "Номер особого договора")]
		public virtual string SpecialContractNumber
		{
			get => _specialContractNumber;
			set => SetField(ref _specialContractNumber, value);
		}

		[Display(Name = "Дата особого договора")]
		public virtual DateTime? SpecialContractDate
		{
			get => _specialContractDate;
			set => SetField(ref _specialContractDate, value);
		}

		[Display(Name = "Особый КПП плательщика")]
		public virtual string PayerSpecialKPP
		{
			get => _payerSpecialKPP;
			set => SetField(ref _payerSpecialKPP, value);
		}

		[Display(Name = "Грузополучатель")]
		public virtual string CargoReceiver
		{
			get => _cargoReceiver;
			set => SetField(ref _cargoReceiver, value);
		}

		[Display(Name = "Особый покупатель")]
		public virtual string SpecialCustomer
		{
			get => _customer;
			set => SetField(ref _customer, value);
		}

		[Display(Name = "Идентификатор государственного контракта")]
		public virtual string GovContract
		{
			get => _govContract;
			set => SetField(ref _govContract, value);
		}

		[Display(Name = "Особый адрес доставки")]
		public virtual string SpecialDeliveryAddress
		{
			get => _deliveryAddress;
			set => SetField(ref _deliveryAddress, value);
		}

		[Display(Name = "Кол-во ТТН")]
		public virtual int? TTNCount
		{
			get => _ttnCount;
			set => SetField(ref _ttnCount, value);
		}

		[Display(Name = "Кол-во Торг-2")]
		public virtual int? Torg2Count
		{
			get => _torg2Count;
			set => SetField(ref _torg2Count, value);
		}

		[Display(Name = "Кол-во УПД(не для безнала)")]
		public virtual int? UPDCount
		{
			get => _updCount;
			set => SetField(ref _updCount, value);
		}

		[Display(Name = "Кол-во УПД")]
		public virtual int? AllUPDCount
		{
			get => _updAllCount;
			set => SetField(ref _updAllCount, value);
		}

		[Display(Name = "Кол-во Торг-12")]
		public virtual int? Torg12Count
		{
			get => _torg12Count;
			set => SetField(ref _torg12Count, value);
		}

		[Display(Name = "Кол-во отчет-фактур")]
		public virtual int? ShetFacturaCount
		{
			get => _shetFacturaCount;
			set => SetField(ref _shetFacturaCount, value);
		}

		[Display(Name = "Кол-во доверенностей вод-ль")]
		public virtual int? CarProxyCount
		{
			get => _carProxyCount;
			set => SetField(ref _carProxyCount, value);
		}

		[Display(Name = "ОКПО")]
		public virtual string OKPO
		{
			get => _okpo;
			set => SetField(ref _okpo, value);
		}

		[Display(Name = "ОКДП")]
		public virtual string OKDP
		{
			get => _okdp;
			set => SetField(ref _okdp, value);
		}

		[Display(Name = "Источник грузополучателя")]
		public virtual CargoReceiverSource CargoReceiverSource
		{
			get => _cargoReceiverSource;
			set => SetField(ref _cargoReceiverSource, value);
		}

		[Display(Name = "Особенный номер ТМЦ")]
		public virtual IList<SpecialNomenclature> SpecialNomenclatures
		{
			get => _specialNomenclatures;
			set => SetField(ref _specialNomenclatures, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SpecialNomenclature> ObservableSpecialNomenclatures
		{
			get
			{
				if(_observableSpecialNomenclatures == null)
				{
					_observableSpecialNomenclatures = new GenericObservableList<SpecialNomenclature>(SpecialNomenclatures);
				}

				return _observableSpecialNomenclatures;
			}
		}

		#endregion ОсобаяПечать

		#region ЭДО и Честный знак

		[Display(Name = "Причина выбытия")]
		public virtual ReasonForLeaving ReasonForLeaving
		{
			get => _reasonForLeaving;
			set => SetField(ref _reasonForLeaving, value);
		}

		[Display(Name = "Статус регистрации в Честном Знаке")]
		public virtual RegistrationInChestnyZnakStatus RegistrationInChestnyZnakStatus
		{
			get => _registrationInChestnyZnakStatus;
			set => SetField(ref _registrationInChestnyZnakStatus, value);
		}

		[Display(Name = "Согласие клиента на ЭДО")]
		public virtual ConsentForEdoStatus ConsentForEdoStatus
		{
			get => _consentForEdoStatus;
			set => SetField(ref _consentForEdoStatus, value);
		}

		[Display(Name = "Статус заказа для отправки УПД")]
		public virtual OrderStatusForSendingUpd OrderStatusForSendingUpd
		{
			get => _orderStatusForSendingUpd;
			set => SetField(ref _orderStatusForSendingUpd, value);
		}

		[Display(Name = "Отказ от печатных документов")]
		public virtual bool IsPaperlessWorkflow
		{
			get => _isPaperlessWorkflow;
			set => SetField(ref _isPaperlessWorkflow, value);
		}

		[Display(Name = "Не отправлять документы по EDO")]
		public virtual bool IsNotSendDocumentsByEdo
		{
			get => _isNotSendDocumentsByEdo;
			set => SetField(ref _isNotSendDocumentsByEdo, value);
		}

		[Display(Name = "Отправлять УПД заранее")]
		public virtual bool CanSendUpdInAdvance
		{
			get => _canSendUpdInAdvance;
			set => SetField(ref _canSendUpdInAdvance, value);
		}

		[Display(Name = "Код личного кабинета в ЭДО")]
		public virtual string PersonalAccountIdInEdo
		{
			get => _personalAccountIdInEdo;
			set
			{
				var cleanedId = value == null
					? null
					: Regex.Replace(value, @"\s+", string.Empty);

				SetField(ref _personalAccountIdInEdo, cleanedId?.ToUpper());
			}
		}

		[Display(Name = "Оператор ЭДО")]
		public virtual EdoOperator EdoOperator
		{
			get => _edoOperator;
			set => SetField(ref _edoOperator, value);
		}

		[Display(Name = "Фамилия")]
		public virtual string Surname
		{
			get => _surname;
			set => SetField(ref _surname, value);
		}

		[Display(Name = "Имя")]
		public virtual string FirstName
		{
			get => _firstName;
			set => SetField(ref _firstName, value);
		}

		[Display(Name = "Отчество")]
		public virtual string Patronymic
		{
			get => _patronymic;
			set => SetField(ref _patronymic, value);
		}

		[Display(Name = "Не смешивать в одном заказе маркированные и немаркированные товары")]
		public virtual bool DoNotMixMarkedAndUnmarkedGoodsInOrder
		{
			get => _doNotMixMarkedAndUnmarkedGoodsInOrder;
			set => SetField(ref _doNotMixMarkedAndUnmarkedGoodsInOrder, value);
		}

		[Display(Name = "Отправлять счета по ЭДО")]
		public virtual bool NeedSendBillByEdo
		{
			get => _needSendBillByEdo;
			set => SetField(ref _needSendBillByEdo, value);
		}

		#endregion

		[Display(Name = "Отсрочка дней")]
		public virtual int DelayDaysForProviders
		{
			get => _delayDaysForProviders;
			set => SetField(ref _delayDaysForProviders, value);
		}

		[Display(Name = "Отсрочка дней покупателям")]
		public virtual int DelayDaysForBuyers
		{
			get => _delayDaysForBuyers;
			set => SetField(ref _delayDaysForBuyers, value);
		}

		[Display(Name = "Тип контрагента")]
		public virtual CounterpartyType CounterpartyType
		{
			get => _counterpartyType;
			set => SetField(ref _counterpartyType, value);
		}

		[Display(Name = "Подтип контрагента")]
		public virtual CounterpartySubtype CounterpartySubtype
		{
			get => _counterpartySubtype;
			set => SetField(ref _counterpartySubtype, value);
		}

		[Display(Name = "Сетевой магазин")]
		public virtual bool IsChainStore
		{
			get => _isChainStore;
			set => SetField(ref _isChainStore, value);
		}

		[Display(Name = "Для розницы")]
		public virtual bool IsForRetail
		{
			get => _isForRetail;
			set => SetField(ref _isForRetail, value);
		}

		[Display(Name = "Для отдела продаж")]
		public virtual bool IsForSalesDepartment
		{
			get => _isForSalesDepartment;
			set => SetField(ref _isForSalesDepartment, value);
		}

		[Display(Name = "Без прозвона")]
		public virtual bool NoPhoneCall
		{
			get => _noPhoneCall;
			set => SetField(ref _noPhoneCall, value);
		}

		[Display(Name = "Исключение из Roboats звонков")]
		public virtual bool RoboatsExclude
		{
			get => _roboatsExclude;
			set => SetField(ref _roboatsExclude, value);
		}

		[PropertyChangedAlso(nameof(ObservableSalesChannels))]
		[Display(Name = "Каналы сбыта")]
		public virtual IList<SalesChannel> SalesChannels
		{
			get => _salesChannels;
			set => SetField(ref _salesChannels, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SalesChannel> ObservableSalesChannels
		{
			get
			{
				if(_observableSalesChannels == null)
				{
					_observableSalesChannels = new GenericObservableList<SalesChannel>(SalesChannels);
				}

				return _observableSalesChannels;
			}
		}

		[Display(Name = "Отсрочка технической обработки")]
		public virtual int TechnicalProcessingDelay
		{
			get => _technicalProcessingDelay;
			set => SetField(ref _technicalProcessingDelay, value);
		}

		[PropertyChangedAlso(nameof(ObservablePriceNodes))]
		[Display(Name = "Цены на ТМЦ")]
		public virtual IList<SupplierPriceItem> SuplierPriceItems
		{
			get => _suplierPriceItems;
			set => SetField(ref _suplierPriceItems, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SupplierPriceItem> ObservableSuplierPriceItems
		{
			get
			{
				if(_observableSuplierPriceItems == null)
				{
					_observableSuplierPriceItems = new GenericObservableList<SupplierPriceItem>(SuplierPriceItems);
				}

				return _observableSuplierPriceItems;
			}
		}

		[RestrictedHistoryProperty]
		[IgnoreHistoryTrace]
		[Display(Name = "Всегда отправлять чеки")]
		public virtual bool AlwaysSendReceipts
		{
			get => _alwaysSendReceipts;
			set => SetField(ref _alwaysSendReceipts, value);
		}

		[Display(Name = "Фиксированные цены")]
		public virtual IList<NomenclatureFixedPrice> NomenclatureFixedPrices
		{
			get => _nomenclatureFixedPrices;
			set => SetField(ref _nomenclatureFixedPrices, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<NomenclatureFixedPrice> ObservableNomenclatureFixedPrices
		{
			get => _observableNomenclatureFixedPrices ?? (_observableNomenclatureFixedPrices =
				new GenericObservableList<NomenclatureFixedPrice>(NomenclatureFixedPrices));
		}

		[Display(Name = "Документы")]
		public virtual IList<CounterpartyFile> Files
		{
			get => _files;
			set => SetField(ref _files, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CounterpartyFile> ObservableFiles
		{
			get
			{
				if(_observableFiles == null)
				{
					_observableFiles = new GenericObservableList<CounterpartyFile>(Files);
				}

				return _observableFiles;
			}
		}

		[Display(Name = "Работает через организацию")]
		public virtual Organization WorksThroughOrganization
		{
			get => _worksThroughOrganization;
			set => SetField(ref _worksThroughOrganization, value);
		}

		#endregion Свойства

		#region Calculated Properties

		public virtual string RawJurAddress
		{
			get => JurAddress;
			set
			{
				StringBuilder sb = new StringBuilder(value);
				sb.Replace("\n", "");
				JurAddress = sb.ToString();
				OnPropertyChanged(nameof(RawJurAddress));
			}
		}

		public virtual bool IsNotEmpty
		{
			get
			{
				bool result = false;
				CheckSpecialField(ref result, SpecialContractName);
				CheckSpecialField(ref result, PayerSpecialKPP);
				CheckSpecialField(ref result, CargoReceiver);
				CheckSpecialField(ref result, SpecialCustomer);
				CheckSpecialField(ref result, GovContract);
				CheckSpecialField(ref result, SpecialDeliveryAddress);
				return result;
			}
		}

		public virtual IList<ISupplierPriceNode> PriceNodes
		{
			get => _priceNodes;
			set => SetField(ref _priceNodes, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ISupplierPriceNode> ObservablePriceNodes
		{
			get
			{
				if(_observablePriceNodes == null)
				{
					_observablePriceNodes = new GenericObservableList<ISupplierPriceNode>(PriceNodes);
				}

				return _observablePriceNodes;
			}
		}

		#endregion

		#region CloseDelivery

		public virtual void AddCloseDeliveryComment(string newComment, Employee currentEmployee)
		{
			CloseDeliveryComment = currentEmployee.ShortName + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + ": " + newComment;
		}

		protected virtual bool CloseDelivery(Employee currentEmployee)
		{
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_deliveries_for_counterparty"))
			{
				return false;
			}

			IsDeliveriesClosed = true;
			CloseDeliveryDate = DateTime.Now;
			CloseDeliveryPerson = currentEmployee;
			return true;
		}


		protected virtual bool OpenDelivery()
		{
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_deliveries_for_counterparty"))
			{
				return false;
			}

			IsDeliveriesClosed = false;
			CloseDeliveryDate = null;
			CloseDeliveryPerson = null;
			CloseDeliveryComment = null;

			return true;
		}

		public virtual bool ToggleDeliveryOption(Employee currentEmployee)
		{
			return IsDeliveriesClosed ? OpenDelivery() : CloseDelivery(currentEmployee);
		}

		public virtual string GetCloseDeliveryInfo()
		{
			return CloseDeliveryPerson?.ShortName + " " + CloseDeliveryDate?.ToString("dd/MM/yyyy HH:mm");
		}

		#endregion CloseDelivery

		#region цены поставщика

		public virtual void SupplierPriceListRefresh(string[] searchValues = null)
		{
			bool ShortOrFullNameContainsSearchValues(Nomenclature nom)
			{
				if(searchValues == null)
				{
					return true;
				}

				var shortOrFullName = nom.ShortOrFullName.ToLower();

				foreach(var val in searchValues)
				{
					if(!shortOrFullName.Contains(val.ToLower()))
					{
						return false;
					}
				}

				return true;
			}

			int cnt = 0;
			ObservablePriceNodes.Clear();
			var pItems = SuplierPriceItems.Select(i => i.NomenclatureToBuy)
										  .Distinct()
										  .Where(i => ShortOrFullNameContainsSearchValues(i)
												|| searchValues.Contains(i.Id.ToString()));

			foreach(var nom in pItems)
			{
				var sNom = new SellingNomenclature
				{
					NomenclatureToBuy = nom,
					Parent = null,
					PosNr = (++cnt).ToString()
				};

				var children = SuplierPriceItems.Cast<ISupplierPriceNode>().Where(i => i.NomenclatureToBuy == nom).ToList();

				foreach(var i in children)
				{
					i.Parent = sNom;
				}

				sNom.Children = children;
				ObservablePriceNodes.Add(sNom);
			}
		}

		public virtual void AddSupplierPriceItems(Nomenclature nomenclatureFromSupplier)
		{
			foreach(SupplierPaymentType type in Enum.GetValues(typeof(SupplierPaymentType)))
			{
				ObservableSuplierPriceItems.Add(
					new SupplierPriceItem
					{
						Supplier = this,
						NomenclatureToBuy = nomenclatureFromSupplier,
						PaymentType = type
					}
				);
			}
		}

		public virtual void AddFile(CounterpartyFile file)
		{
			if(ObservableFiles.Contains(file))
			{
				return;
			}

			file.Counterparty = this;
			ObservableFiles.Add(file);
		}

		public virtual void RemoveFile(CounterpartyFile file)
		{
			if(ObservableFiles.Contains(file))
			{
				ObservableFiles.Remove(file);
			}
		}

		public virtual void RemoveNomenclatureWithPrices(int nomenclatureId)
		{
			var removableItems = new List<SupplierPriceItem>(
				ObservableSuplierPriceItems.Where(i => i.NomenclatureToBuy.Id == nomenclatureId).ToList());

			foreach(var item in removableItems)
			{
				ObservableSuplierPriceItems.Remove(item);
			}
		}

		#endregion цены поставщика

		private void CheckSpecialField(ref bool result, string fieldValue)
		{
			if(!string.IsNullOrWhiteSpace(fieldValue))
			{
				result = true;
			}
		}

		public virtual string GetSpecialContractString()
		{
			if(!string.IsNullOrWhiteSpace(SpecialContractName)
				&& string.IsNullOrWhiteSpace(SpecialContractNumber)
				&& !SpecialContractDate.HasValue)
			{
				return SpecialContractName;
			}

			var contractNumber = !string.IsNullOrWhiteSpace(SpecialContractNumber)
				? $"№ {SpecialContractNumber}"
				: string.Empty;

			var contractDate = SpecialContractDate.HasValue
				? $"от {SpecialContractDate.Value.ToShortDateString()}"
				: string.Empty;

			return $"{SpecialContractName} {contractNumber} {contractDate}";
		}

		public Counterparty()
		{
			Name = string.Empty;
			FullName = string.Empty;
			Comment = string.Empty;
			INN = string.Empty;
			OGRN = string.Empty;
			JurAddress = string.Empty;
			PhoneFrom1c = string.Empty;
		}

		#region IValidatableObject implementation

		public virtual bool CheckForINNDuplicate(ICounterpartyRepository counterpartyRepository, IUnitOfWork uow)
		{
			IList<Counterparty> counterarties = counterpartyRepository.GetCounterpartiesByINN(uow, INN);

			if(counterarties == null)
			{
				return false;
			}

			if(counterarties.Any(x => x.Id != Id))
			{
				return true;
			}

			return false;
		}

		public virtual IList<Counterparty> CheckForPhoneNumberDuplicate(ICounterpartyRepository counterpartyRepository, IUnitOfWork uow, string phoneNumber)
		{
			IList<Counterparty> counterarties = counterpartyRepository.GetNotArchivedCounterpartiesByPhoneNumber(uow, phoneNumber);

			return counterarties.Where(x => x?.Id != Id).ToList();
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.ServiceContainer.GetService(typeof(IBottlesRepository)) is IBottlesRepository bottlesRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(bottlesRepository)}");
			}

			if(!(validationContext.ServiceContainer.GetService(typeof(IDepositRepository)) is IDepositRepository depositRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(depositRepository)}");
			}

			if(!(validationContext.ServiceContainer.GetService(typeof(IMoneyRepository)) is IMoneyRepository moneyRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(moneyRepository)}");
			}

			if(!(validationContext.ServiceContainer.GetService(
				typeof(ICounterpartyRepository)) is ICounterpartyRepository counterpartyRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(counterpartyRepository)}");
			}

			if(!(validationContext.ServiceContainer.GetService(typeof(IOrderRepository)) is IOrderRepository orderRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(orderRepository)}");
			}

			if(CargoReceiverSource == CargoReceiverSource.Special && string.IsNullOrWhiteSpace(CargoReceiver))
			{
				yield return new ValidationResult("Если выбран особый грузополучатель, необходимо ввести данные о нем");
			}

			if(CargoReceiver != null && CargoReceiver.Length > _cargoReceiverLimitSymbols)
			{
				yield return new ValidationResult(
					$"Длина строки \"Грузополучатель\" не должна превышать {_cargoReceiverLimitSymbols} символов");
			}

			if(CheckForINNDuplicate(counterpartyRepository, UoW))
			{
				yield return new ValidationResult(
					"Контрагент с данным ИНН уже существует.",
					new[] { this.GetPropertyName(o => o.INN) });
			}

			if(UseSpecialDocFields && PayerSpecialKPP != null && PayerSpecialKPP.Length != 9)
			{
				yield return new ValidationResult("Длина КПП для документов должна равнятся 9-ти.",
					new[] { this.GetPropertyName(o => o.KPP) });
			}

			if(PersonType == PersonType.legal)
			{
				if(TypeOfOwnership == null || TypeOfOwnership.Length == 0)
				{
					yield return new ValidationResult("Не заполнена Форма собственности.",
						new[] { nameof(TypeOfOwnership) });
				}

				if(KPP?.Length != 9 && KPP?.Length != 0 && TypeOfOwnership != "ИП")
				{
					yield return new ValidationResult("Длина КПП должна равнятся 9-ти.",
						new[] { this.GetPropertyName(o => o.KPP) });
				}

				if(INN.Length != 10 && INN.Length != 0 && TypeOfOwnership != "ИП")
				{
					yield return new ValidationResult("Длина ИНН должна равнятся 10-ти.",
						new[] { this.GetPropertyName(o => o.INN) });
				}

				if(INN.Length != 12 && INN.Length != 0 && TypeOfOwnership == "ИП")
				{
					yield return new ValidationResult("Длина ИНН для ИП должна равнятся 12-ти.",
						new[] { this.GetPropertyName(o => o.INN) });
				}

				if(string.IsNullOrWhiteSpace(KPP) && TypeOfOwnership != "ИП")
				{
					yield return new ValidationResult("Для организации необходимо заполнить КПП.",
						new[] { this.GetPropertyName(o => o.KPP) });
				}

				if(string.IsNullOrWhiteSpace(INN))
				{
					yield return new ValidationResult("Для организации необходимо заполнить ИНН.",
						new[] { this.GetPropertyName(o => o.INN) });
				}

				if(KPP != null && !Regex.IsMatch(KPP, "^[0-9]*$") && TypeOfOwnership != "ИП")
				{
					yield return new ValidationResult("КПП может содержать только цифры.",
						new[] { this.GetPropertyName(o => o.KPP) });
				}

				if(!Regex.IsMatch(INN, "^[0-9]*$"))
				{
					yield return new ValidationResult("ИНН может содержать только цифры.",
						new[] { this.GetPropertyName(o => o.INN) });
				}
			}

			if(IsDeliveriesClosed && string.IsNullOrWhiteSpace(CloseDeliveryComment))
			{
				yield return new ValidationResult("Необходимо заполнить комментарий по закрытию поставок",
					new[] { this.GetPropertyName(o => o.CloseDeliveryComment) });
			}

			if(IsArchive)
			{
				var unclosedContracts = CounterpartyContracts.Where(c => !c.IsArchive)
					.Select(c => c.Id.ToString()).ToList();

				if(unclosedContracts.Count > 0)
				{
					yield return new ValidationResult(
						string.Format("Вы не можете сдать контрагента в архив с открытыми договорами: {0}", string.Join(", ", unclosedContracts)),
						new[] { this.GetPropertyName(o => o.CounterpartyContracts) });
				}

				var balance = moneyRepository.GetCounterpartyDebt(UoW, this);

				if(balance != 0)
				{
					yield return new ValidationResult(
						string.Format("Вы не можете сдать контрагента в архив так как у него имеется долг: {0}", CurrencyWorks.GetShortCurrencyString(balance)));
				}

				var activeOrders = orderRepository.GetCurrentOrders(UoW, this);

				if(activeOrders.Count > 0)
				{
					yield return new ValidationResult(
						string.Format("Вы не можете сдать контрагента в архив с незакрытыми заказами: {0}", string.Join(", ", activeOrders.Select(o => o.Id.ToString()))),
						new[] { this.GetPropertyName(o => o.CounterpartyContracts) });
				}

				var deposit = depositRepository.GetDepositsAtCounterparty(UoW, this, null);

				if(deposit != 0)
				{
					yield return new ValidationResult(
						string.Format("Вы не можете сдать контрагента в архив так как у него есть невозвращенные залоги: {0}", CurrencyWorks.GetShortCurrencyString(deposit)));
				}

				var bottles = bottlesRepository.GetBottlesDebtAtCounterparty(UoW, this);
				
				if(bottles != 0)
				{
					yield return new ValidationResult(
						string.Format("Вы не можете сдать контрагента в архив так как он не вернул {0} бутылей", bottles));
				}
			}

			if(Id == 0 && CameFrom == null)
			{
				yield return new ValidationResult("Для новых клиентов необходимо заполнить поле \"Откуда клиент\"");
			}

			if(CounterpartyType == CounterpartyType.Dealer && string.IsNullOrEmpty(OGRN))
			{
				yield return new ValidationResult("Для дилеров необходимо заполнить поле \"ОГРН\"");
			}

			if(Id == 0 && PersonType == PersonType.legal && TaxType == TaxType.None)
			{
				yield return new ValidationResult("Для новых клиентов необходимо заполнить поле \"Налогообложение\"");
			}

			var everyAddedMinCountValueCount = NomenclatureFixedPrices
				.GroupBy(p => new { p.Nomenclature, p.MinCount })
				.Select(p => new { NomenclatureName = p.Key.Nomenclature?.Name, MinCountValue = p.Key.MinCount, Count = p.Count() });

			foreach(var p in everyAddedMinCountValueCount)
			{
				if(p.Count > 1)
				{
					yield return new ValidationResult(
							$"\"{p.NomenclatureName}\": фиксированная цена для количества \"{p.MinCountValue}\" указана {p.Count} раз(а)",
							new[] { this.GetPropertyName(o => o.NomenclatureFixedPrices) });
				}
			}

			foreach(var fixedPrice in NomenclatureFixedPrices)
			{
				var fixedPriceValidationResults = fixedPrice.Validate(validationContext);
				foreach(var fixedPriceValidationResult in fixedPriceValidationResults)
				{
					yield return fixedPriceValidationResult;
				}
			}

			if(Id == 0 && UseSpecialDocFields)
			{
				if(!string.IsNullOrWhiteSpace(SpecialContractName)
					&& (string.IsNullOrWhiteSpace(SpecialContractNumber) || !SpecialContractDate.HasValue))
				{
					yield return new ValidationResult("Помимо специального названия договора надо заполнить его номер и дату");
				}

				if(!string.IsNullOrWhiteSpace(SpecialContractNumber)
					&& (string.IsNullOrWhiteSpace(SpecialContractName) || !SpecialContractDate.HasValue))
				{
					yield return new ValidationResult("Помимо специального номера договора надо заполнить его название и дату");
				}

				if(SpecialContractDate.HasValue
					&& (string.IsNullOrWhiteSpace(SpecialContractNumber) || string.IsNullOrWhiteSpace(SpecialContractName)))
				{
					yield return new ValidationResult("Помимо специальной даты договора надо заполнить его название и номер");
				}
			}

			if(UseSpecialDocFields)
			{
				if(!string.IsNullOrWhiteSpace(SpecialContractName) && SpecialContractName.Length > _specialContractNameLimit)
				{
					yield return new ValidationResult(
						$"Длина наименования особого договора превышена на {SpecialContractName.Length - _specialContractNameLimit}");
				}
			}

			if(TechnicalProcessingDelay > 0 && Files.Count == 0)
			{
				yield return new ValidationResult("Для установки дней отсрочки тех обработки необходимо загрузить документ");
			}

			var phonesValidationStringBuilder = new StringBuilder();
			var phoneNumberDuplicatesIsChecked = new List<string>();

			var phonesDuplicates = counterpartyRepository.GetNotArchivedCounterpartiesAndDeliveryPointsDescriptionsByPhoneNumber(UoW, Phones.ToList(), Id);

			foreach(var phone in phonesDuplicates)
			{
				phonesValidationStringBuilder.AppendLine($"Телефон {phone.Key} уже указан у контрагентов:");

				foreach(var message in phone.Value)
				{
					phonesValidationStringBuilder.AppendLine($"\t{message}");
				}
			}

			foreach(var phone in Phones)
			{
				if(phone.RoboAtsCounterpartyName == null)
				{
					phonesValidationStringBuilder.AppendLine($"Для телефона {phone.Number} не указано имя контрагента.");
				}

				if(phone.RoboAtsCounterpartyPatronymic == null)
				{
					phonesValidationStringBuilder.AppendLine($"Для телефона {phone.Number} не указано отчество контрагента.");
				}

				if(!phone.IsValidPhoneNumber)
				{
					phonesValidationStringBuilder.AppendLine($"Номер {phone.Number} имеет неправильный формат.");
				}

				#region Проверка дубликатов номера телефона

				if(!phoneNumberDuplicatesIsChecked.Contains(phone.Number))
				{
					if(Phones.Where(p => p.Number == phone.Number).Count() > 1)
					{
						phonesValidationStringBuilder.AppendLine($"Телефон {phone.Number} в карточке контрагента указан несколько раз.");
					}

					phoneNumberDuplicatesIsChecked.Add(phone.Number);
				}

				#endregion
			}

			var phonesValidationMessage = phonesValidationStringBuilder.ToString();

			if(!string.IsNullOrEmpty(phonesValidationMessage))
			{
				yield return new ValidationResult(phonesValidationMessage);
			}

			if(ReasonForLeaving == ReasonForLeaving.Resale && string.IsNullOrWhiteSpace(INN))
			{
				yield return new ValidationResult("Для перепродажи должен быть заполнен ИНН");
			}

			if(IsNotSendDocumentsByEdo && IsPaperlessWorkflow)
			{
				yield return new ValidationResult("При выборе \"Не отправлять документы по EDO\" должен быть отключен \"Отказ от печатных документов\"");
			}

			foreach(var email in Emails)
			{
				if(!email.IsValidEmail)
				{
					yield return new ValidationResult($"Адрес электронной почты {email.Address} имеет неправильный формат.");
				}
			}
		}

		#endregion
	}
}

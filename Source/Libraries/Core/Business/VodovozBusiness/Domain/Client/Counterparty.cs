using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Gamma.Utilities;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Services;
using QS.Utilities;
using Vodovoz.Domain.Cash;
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
	public class Counterparty : AccountOwnerBase, IDomainObject, IValidatableObject
	{
		//Используется для валидации, не получается истолльзовать бизнес объект так как наследуемся от AccountOwnerBase
		public virtual IUnitOfWork UoW { get; set; }
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
		GenericObservableList<CounterpartyEdoOperator> _observableCounterpartyEdoOperators;

		#region Свойства

		private IList<CounterpartyContract> counterpartyContracts;

		[Display(Name = "Договоры")]
		public virtual IList<CounterpartyContract> CounterpartyContracts {
			get => counterpartyContracts;
			set => SetField(ref counterpartyContracts, value, () => CounterpartyContracts);
		}

		private IList<DeliveryPoint> deliveryPoints = new List<DeliveryPoint>();

		[Display(Name = "Точки доставки")]
		public virtual IList<DeliveryPoint> DeliveryPoints {
			get => deliveryPoints;
			set => SetField(ref deliveryPoints, value, () => DeliveryPoints);
		}

		GenericObservableList<DeliveryPoint> observableDeliveryPoints;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryPoint> ObservableDeliveryPoints {
			get {
				if(observableDeliveryPoints == null)
					observableDeliveryPoints = new GenericObservableList<DeliveryPoint>(DeliveryPoints);
				return observableDeliveryPoints;
			}
		}

		private IList<Tag> tags = new List<Tag>();

		[Display(Name = "Теги")]
		public virtual IList<Tag> Tags {
			get => tags;
			set => SetField(ref tags, value, () => Tags);
		}

		GenericObservableList<Tag> observableTags;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Tag> ObservableTags {
			get {
				if(observableTags == null)
					observableTags = new GenericObservableList<Tag>(Tags);
				return observableTags;
			}
		}

		private IList<Contact> contact = new List<Contact>();

		[Display(Name = "Контактные лица")]
		public virtual IList<Contact> Contacts {
			get => contact;
			set => SetField(ref contact, value, () => Contacts);
		}

		#region CloseDelivery

		private bool isDeliveriesClosed;

		[Display(Name = "Поставки закрыты?")]
		public virtual bool IsDeliveriesClosed {
			get => isDeliveriesClosed;
			protected set => SetField(ref isDeliveriesClosed, value, () => IsDeliveriesClosed);
		}

		private string closeDeliveryComment;

		[Display(Name = "Комментарий по закрытию поставок")]
		public virtual string CloseDeliveryComment {
			get => closeDeliveryComment;
			set => SetField(ref closeDeliveryComment, value, () => CloseDeliveryComment);
		}

		private DateTime? closeDeliveryDate;

		[Display(Name = "Дата закрытия поставок")]
		public virtual DateTime? CloseDeliveryDate {
			get => closeDeliveryDate;
			protected set => SetField(ref closeDeliveryDate, value, () => CloseDeliveryDate);
		}

		private Employee closeDeliveryPerson;

		[Display(Name = "Сотрудник закрывший поставки")]
		public virtual Employee CloseDeliveryPerson {
			get => closeDeliveryPerson;
			protected set => SetField(ref closeDeliveryPerson, value, () => CloseDeliveryPerson);
		}

		#endregion CloseDelivery

		private IList<Proxy> proxies;

		[Display(Name = "Доверенности")]
		public virtual IList<Proxy> Proxies {
			get => proxies;
			set => SetField(ref proxies, value, () => Proxies);
		}

		public virtual int Id { get; set; }

		decimal maxCredit;

		[Display(Name = "Максимальный кредит")]
		public virtual decimal MaxCredit {
			get => maxCredit;
			set => SetField(ref maxCredit, value, () => MaxCredit);
		}

		string name;

		[Required(ErrorMessage = "Название контрагента должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set {
				if(SetField(ref name, value, () => Name) && PersonType == PersonType.natural)
					FullName = Name;
			}
		}

		string typeOfOwnership;

		[Display(Name = "Форма собственности")]
		[StringLength(10)]
		public virtual string TypeOfOwnership {
			get => typeOfOwnership;
			set => SetField(ref typeOfOwnership, value);
		}

		string fullName;

		[Display(Name = "Полное название")]
		public virtual string FullName {
			get => fullName;
			set => SetField(ref fullName, value, () => FullName);
		}

		/// <summary>
		/// Генерируется триггером на строне БД.
		/// </summary>
		int vodovozInternalId;
		[Display(Name = "Внутренний номер контрагента")]
		public virtual int VodovozInternalId {
			get => vodovozInternalId;
			set => SetField(ref vodovozInternalId, value, () => VodovozInternalId);
		}

		string code1c;

		public virtual string Code1c {
			get => code1c;
			set => SetField(ref code1c, value, () => Code1c);
		}

		string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		string iNN;

		[Display(Name = "ИНН")]
		public virtual string INN {
			get => iNN;
			set => SetField(ref iNN, value, () => INN);
		}

		string kPP;

		[Display(Name = "КПП")]
		public virtual string KPP {
			get => kPP;
			set => SetField(ref kPP, value, () => KPP);
		}

		string oGRN;

		[Display(Name = "ОГРН")]
		public virtual string OGRN {
			get => oGRN;
			set => SetField(ref oGRN, value, () => OGRN);
		}

		string jurAddress;

		[Display(Name = "Юридический адрес")]
		public virtual string JurAddress {
			get => jurAddress;
			set => SetField(ref jurAddress, value, () => JurAddress);
		}

		string address;

		[Display(Name = "Фактический адрес")]
		public virtual string Address {
			get => address;
			set => SetField(ref address, value, () => Address);
		}

		PaymentType paymentMethod;

		[Display(Name = "Вид оплаты")]
		public virtual PaymentType PaymentMethod {
			get => paymentMethod;
			set => SetField(ref paymentMethod, value);
		}

		PersonType personType;

		[Display(Name = "Форма контрагента")]
		public virtual PersonType PersonType {
			get => personType;
			set {
				SetField(ref personType, value, () => PersonType);

				if(value == PersonType.natural)
				{
					PaymentMethod = PaymentType.Cash;
				}
			}
		}

		ExpenseCategory defaultExpenseCategory;

		[Display(Name = "Расход по-умолчанию")]
		public virtual ExpenseCategory DefaultExpenseCategory {
			get => defaultExpenseCategory;
			set => SetField(ref defaultExpenseCategory, value, () => DefaultExpenseCategory);
		}

		Counterparty mainCounterparty;

		[Display(Name = "Головная организация")]
		public virtual Counterparty MainCounterparty {
			get => mainCounterparty;
			set => SetField(ref mainCounterparty, value, () => MainCounterparty);
		}

		Counterparty previousCounterparty;

		[Display(Name = "Предыдущий контрагент")]
		public virtual Counterparty PreviousCounterparty {
			get => previousCounterparty;
			set => SetField(ref previousCounterparty, value, () => PreviousCounterparty);
		}

		bool isArchive;

		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value, () => IsArchive);
		}

		IList<Phone> phones = new List<Phone>();

		[Display(Name = "Телефоны")]
		public virtual IList<Phone> Phones {
			get => phones;
			set => SetField(ref phones, value, () => Phones);
		}

		GenericObservableList<Phone> observablePhones;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Phone> ObservablePhones {
			get {
				if(observablePhones == null)
					observablePhones = new GenericObservableList<Phone>(Phones);
				return observablePhones;
			}
		}

		string ringUpPhone;

		[Display(Name = "Телефон для обзвона")]
		public virtual string RingUpPhone {
			get => ringUpPhone;
			set => SetField(ref ringUpPhone, value, () => RingUpPhone);
		}


		IList<Email> emails;

		[Display(Name = "E-mail адреса")]
		public virtual IList<Email> Emails {
			get => emails;
			set => SetField(ref emails, value, () => Emails);
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
		

		Employee accountant;

		[Display(Name = "Бухгалтер")]
		public virtual Employee Accountant {
			get => accountant;
			set => SetField(ref accountant, value, () => Accountant);
		}

		Employee salesManager;

		[Display(Name = "Менеджер по продажам")]
		public virtual Employee SalesManager {
			get => salesManager;
			set => SetField(ref salesManager, value, () => SalesManager);
		}

		Employee bottlesManager;

		[Display(Name = "Менеджер по бутылям")]
		public virtual Employee BottlesManager {
			get => bottlesManager;
			set => SetField(ref bottlesManager, value, () => BottlesManager);
		}

		Contact mainContact;

		[Display(Name = "Главное контактное лицо")]
		public virtual Contact MainContact {
			get => mainContact;
			set => SetField(ref mainContact, value, () => MainContact);
		}

		Contact financialContact;

		[Display(Name = "Контакт по финансовым вопросам")]
		public virtual Contact FinancialContact {
			get => financialContact;
			set => SetField(ref financialContact, value, () => FinancialContact);
		}

		DefaultDocumentType? defaultDocumentType;

		[Display(Name = "Тип безналичных документов по-умолчанию")]
		public virtual DefaultDocumentType? DefaultDocumentType {
			get => defaultDocumentType;
			set => SetField(ref defaultDocumentType, value, () => DefaultDocumentType);
		}

		private bool newBottlesNeeded;

		[Display(Name = "Новая необоротная тара")]
		public virtual bool NewBottlesNeeded {
			get => newBottlesNeeded;
			set => SetField(ref newBottlesNeeded, value, () => NewBottlesNeeded);
		}

		string signatoryFIO;

		[Display(Name = "ФИО подписанта")]
		public virtual string SignatoryFIO {
			get => signatoryFIO;
			set => SetField(ref signatoryFIO, value, () => SignatoryFIO);
		}

		string signatoryPost;

		[Display(Name = "Должность подписанта")]
		public virtual string SignatoryPost {
			get => signatoryPost;
			set => SetField(ref signatoryPost, value, () => SignatoryPost);
		}

		string signatoryBaseOf;

		[Display(Name = "На основании")]
		public virtual string SignatoryBaseOf {
			get => signatoryBaseOf;
			set => SetField(ref signatoryBaseOf, value, () => SignatoryBaseOf);
		}

		string phoneFrom1c;

		[Display(Name = "Телефон")]
		public virtual string PhoneFrom1c {
			get => phoneFrom1c;
			set => SetField(ref phoneFrom1c, value, () => PhoneFrom1c);
		}

		ClientCameFrom cameFrom;

		[Display(Name = "Откуда клиент")]
		public virtual ClientCameFrom CameFrom {
			get => cameFrom;
			set => SetField(ref cameFrom, value, () => CameFrom);
		}

		Order firstOrder;
		[Display(Name = "Первый заказ")]
		public virtual Order FirstOrder {
			get => firstOrder;
			set => SetField(ref firstOrder, value, () => FirstOrder);
		}
		
		TaxType taxType;
		[Display(Name = "Налогобложение")]
		public virtual TaxType TaxType {
			get => taxType;
			set => SetField(ref taxType, value);
		}
		
		private DateTime? createDate = DateTime.Now;
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreateDate {
			get => createDate;
			set => SetField(ref createDate, value);
		}

		private LogisticsRequirements _logisticsRequirements;
		[Display(Name = "Требования к логистике")]
		public virtual LogisticsRequirements LogisticsRequirements
		{
			get => _logisticsRequirements;
			set => SetField(ref _logisticsRequirements, value);
		}

		#region ОсобаяПечать
		bool useSpecialDocFields;
		[Display(Name = "Особая печать документов")]
		public virtual bool UseSpecialDocFields {
			get => useSpecialDocFields;
			set => SetField(ref useSpecialDocFields, value, () => UseSpecialDocFields);
		}

		bool alwaysPrintInvoice;
		[Display(Name = "Всегда печатать накладную")]
		public virtual bool AlwaysPrintInvoice
		{
			get => alwaysPrintInvoice;
			set => SetField(ref alwaysPrintInvoice, value);
		}
		#region Особое требование срок годности
		[Display(Name = "Особое требование: требуется срок годности")]
		bool specialExpireDatePercentCheck;
		public virtual bool SpecialExpireDatePercentCheck
		{
			get => specialExpireDatePercentCheck;
			set => SetField(ref specialExpireDatePercentCheck, value, () => SpecialExpireDatePercentCheck);
		}

		decimal specialExpireDatePercent;
		[Display(Name = "Особое требование: срок годности %")]
		public virtual decimal SpecialExpireDatePercent {
			get => specialExpireDatePercent;
			set => SetField(ref specialExpireDatePercent, value, () => SpecialExpireDatePercent); 
		}

		#endregion Особое требование срок годности
		
		[Display(Name = "Название особого договора")]
		public virtual string SpecialContractName {
			get => _specialContractName;
			set => SetField(ref _specialContractName, value);
		}
		
		[Display(Name = "Номер особого договора")]
		public virtual string SpecialContractNumber {
			get => _specialContractNumber;
			set => SetField(ref _specialContractNumber, value);
		}
		
		[Display(Name = "Дата особого договора")]
		public virtual DateTime? SpecialContractDate {
			get => _specialContractDate;
			set => SetField(ref _specialContractDate, value);
		}

		string payerSpecialKPP;
		[Display(Name = "Особый КПП плательщика")]
		public virtual string PayerSpecialKPP {
			get => payerSpecialKPP;
			set => SetField(ref payerSpecialKPP, value, () => PayerSpecialKPP);
		}

		string cargoReceiver;
		[Display(Name = "Грузополучатель")]
		public virtual string CargoReceiver {
			get => cargoReceiver;
			set => SetField(ref cargoReceiver, value, () => CargoReceiver);
		}

		string customer;
		[Display(Name = "Особый покупатель")]
		public virtual string SpecialCustomer {
			get => customer;
			set => SetField(ref customer, value, () => SpecialCustomer);
		}

		string govContract;
		[Display(Name = "Идентификатор государственного контракта")]
		public virtual string GovContract {
			get => govContract;
			set => SetField(ref govContract, value, () => GovContract);
		}

		string deliveryAddress;
		[Display(Name = "Особый адрес доставки")]
		public virtual string SpecialDeliveryAddress {
			get => deliveryAddress;
			set => SetField(ref deliveryAddress, value, () => SpecialDeliveryAddress);
		}

		int? ttnCount;
		[Display(Name = "Кол-во ТТН")]
		public virtual int? TTNCount {
			get => ttnCount;
			set => SetField(ref ttnCount, value, () => TTNCount);
		}

		int? torg2Count;
		[Display(Name = "Кол-во Торг-2")]
		public virtual int? Torg2Count {
			get => torg2Count;
			set => SetField(ref torg2Count, value, () => Torg2Count);
		}

		int? updCount;
		[Display(Name = "Кол-во УПД(не для безнала)")]
		public virtual int? UPDCount {
			get => updCount;
			set => SetField(ref updCount, value);
		}

		int? updAllCount;
		[Display(Name = "Кол-во УПД")]
		public virtual int? AllUPDCount
		{
			get => updAllCount;
			set => SetField(ref updAllCount, value);
		}

		int? torg12Count;
		[Display(Name = "Кол-во Торг-12")]
		public virtual int? Torg12Count
		{
			get => torg12Count;
			set => SetField(ref torg12Count, value);
		}

		int? shetFacturaCount;
		[Display(Name = "Кол-во отчет-фактур")]
		public virtual int? ShetFacturaCount
		{
			get => shetFacturaCount;
			set => SetField(ref shetFacturaCount, value);
		}

		int? carProxyCount;
		[Display(Name = "Кол-во доверенностей вод-ль")]
		public virtual int? CarProxyCount
		{
			get => carProxyCount;
			set => SetField(ref carProxyCount, value);
		}

		string okpo;
		[Display(Name = "ОКПО")]
		public virtual string OKPO {
			get => okpo;
			set => SetField(ref okpo, value, () => OKPO);
		}

		string okdp;
		[Display(Name = "ОКДП")]
		public virtual string OKDP {
			get => okdp;
			set => SetField(ref okdp, value, () => OKDP);
		}

		CargoReceiverSource cargoReceiverSource;
		[Display(Name = "Источник грузополучателя")]
		public virtual CargoReceiverSource CargoReceiverSource {
			get => cargoReceiverSource;
			set => SetField(ref cargoReceiverSource, value, () => CargoReceiverSource);
		}


		IList<SpecialNomenclature> specialNomenclatures = new List<SpecialNomenclature>();
		[Display(Name = "Особенный номер ТМЦ")]
		public virtual IList<SpecialNomenclature> SpecialNomenclatures {
			get => specialNomenclatures;
			set => SetField(ref specialNomenclatures, value);
		}

		GenericObservableList<SpecialNomenclature> observableSpecialNomenclatures;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SpecialNomenclature> ObservableSpecialNomenclatures {
			get {
				if(observableSpecialNomenclatures == null)
					observableSpecialNomenclatures = new GenericObservableList<SpecialNomenclature>(SpecialNomenclatures);
				return observableSpecialNomenclatures;
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

		#endregion

		int delayDaysForProviders;
		[Display(Name = "Отсрочка дней")]
		public virtual int DelayDaysForProviders {
			get => delayDaysForProviders;
			set => SetField(ref delayDaysForProviders, value);
		}
		
		int delayDaysForBuyers;
		[Display(Name = "Отсрочка дней покупателям")]
		public virtual int DelayDaysForBuyers {
			get => delayDaysForBuyers;
			set => SetField(ref delayDaysForBuyers, value);
		}

		CounterpartyType counterpartyType;
		[Display(Name = "Тип контрагента")]
		public virtual CounterpartyType CounterpartyType {
			get => counterpartyType;
			set => SetField(ref counterpartyType, value);
		}

		private bool isChainStore;
		[Display(Name = "Сетевой магазин")]
		public virtual bool IsChainStore {
			get => isChainStore;
			set => SetField(ref isChainStore, value);
		}

		private bool isForRetail;
		[Display(Name = "Для розницы")]
		public virtual bool IsForRetail
		{
			get => isForRetail;
			set => SetField(ref isForRetail, value);
		}

		[Display(Name = "Для отдела продаж")]
		public virtual bool IsForSalesDepartment
		{
			get => _isForSalesDepartment;
			set => SetField(ref _isForSalesDepartment, value);
		}

		private bool noPhoneCall;
		[Display(Name = "Без прозвона")]
		public virtual bool NoPhoneCall
		{
			get => noPhoneCall;
			set => SetField(ref noPhoneCall, value);
		}

		[Display(Name = "Исключение из Roboats звонков")]
		public virtual bool RoboatsExclude
		{
			get => _roboatsExclude;
			set => SetField(ref _roboatsExclude, value);
		}

		IList<SalesChannel> salesChannels = new List<SalesChannel>();
		[PropertyChangedAlso(nameof(ObservableSalesChannels))]
		[Display(Name = "Каналы сбыта")]
		public virtual IList<SalesChannel> SalesChannels
		{
			get => salesChannels;
			set => SetField(ref salesChannels, value);
		}

		GenericObservableList<SalesChannel> observableSalesChannels;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SalesChannel> ObservableSalesChannels
		{
			get
			{
				if (observableSalesChannels == null)
					observableSalesChannels = new GenericObservableList<SalesChannel>(SalesChannels);
				return observableSalesChannels;
			}
		}

		private int technicalProcessingDelay;
		[Display(Name = "Отсрочка технической обработки")]
		public virtual int TechnicalProcessingDelay
		{
			get => technicalProcessingDelay;
			set => SetField(ref technicalProcessingDelay, value);
		}

		IList<SupplierPriceItem> suplierPriceItems = new List<SupplierPriceItem>();
		[PropertyChangedAlso(nameof(ObservablePriceNodes))]
		[Display(Name = "Цены на ТМЦ")]
		public virtual IList<SupplierPriceItem> SuplierPriceItems {
			get => suplierPriceItems;
			set => SetField(ref suplierPriceItems, value);
		}

		GenericObservableList<SupplierPriceItem> observableSuplierPriceItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SupplierPriceItem> ObservableSuplierPriceItems {
			get {
				if(observableSuplierPriceItems == null)
					observableSuplierPriceItems = new GenericObservableList<SupplierPriceItem>(SuplierPriceItems);
				return observableSuplierPriceItems;
			}
		}

		private bool _alwaysSendReceipts;
		[RestrictedHistoryProperty]
		[IgnoreHistoryTrace]
		[Display(Name = "Всегда отправлять чеки")]
		public virtual bool AlwaysSendReceipts {
			get => _alwaysSendReceipts;
			set => SetField(ref _alwaysSendReceipts, value);
		}

		private IList<NomenclatureFixedPrice> nomenclatureFixedPrices = new List<NomenclatureFixedPrice>();
		[Display(Name = "Фиксированные цены")]
		public virtual IList<NomenclatureFixedPrice> NomenclatureFixedPrices {
			get => nomenclatureFixedPrices;
			set => SetField(ref nomenclatureFixedPrices, value);
		}

		private GenericObservableList<NomenclatureFixedPrice> observableNomenclatureFixedPrices;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<NomenclatureFixedPrice> ObservableNomenclatureFixedPrices {
			get => observableNomenclatureFixedPrices ?? (observableNomenclatureFixedPrices =
				new GenericObservableList<NomenclatureFixedPrice>(NomenclatureFixedPrices));
		}

		IList<CounterpartyFile> files = new List<CounterpartyFile>();
		[Display(Name = "Документы")]
		public virtual IList<CounterpartyFile> Files
		{
			get => files;
			set => SetField(ref files, value);
		}

		GenericObservableList<CounterpartyFile> observableFiles;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CounterpartyFile> ObservableFiles
		{
			get
			{
				if (observableFiles == null)
					observableFiles = new GenericObservableList<CounterpartyFile>(Files);
				return observableFiles;
			}
		}

		private Organization _worksThroughOrganization;
		[Display(Name = "Работает через организацию")]
		public virtual Organization WorksThroughOrganization
		{
			get => _worksThroughOrganization;
			set => SetField(ref _worksThroughOrganization, value);
		}

		#region Calculated Properties

		public virtual string RawJurAddress {
			get => JurAddress;
			set {
				StringBuilder sb = new StringBuilder(value);
				sb.Replace("\n", "");
				JurAddress = sb.ToString();
				OnPropertyChanged(nameof(RawJurAddress));
			}
		}

		private void CheckSpecialField(ref bool result, string fieldValue)
		{
			if(!string.IsNullOrWhiteSpace(fieldValue))
				result = true;
		}

		public virtual bool IsNotEmpty {
			get {
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

		IList<ISupplierPriceNode> priceNodes = new List<ISupplierPriceNode>();
		public virtual IList<ISupplierPriceNode> PriceNodes {
			get => priceNodes;
			set => SetField(ref priceNodes, value);
		}

		GenericObservableList<ISupplierPriceNode> observablePriceNodes;

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ISupplierPriceNode> ObservablePriceNodes {
			get {
				if(observablePriceNodes == null)
					observablePriceNodes = new GenericObservableList<ISupplierPriceNode>(PriceNodes);
				return observablePriceNodes;
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
					return true;

				var shortOrFullName = nom.ShortOrFullName.ToLower();
				foreach(var val in searchValues) {
					if(!shortOrFullName.Contains(val.ToLower()))
						return false;
				}
				return true;
			}

			int cnt = 0;
			ObservablePriceNodes.Clear();
			var pItems = SuplierPriceItems.Select(i => i.NomenclatureToBuy)
										  .Distinct()
										  .Where(i => ShortOrFullNameContainsSearchValues(i) 
												   || searchValues.Contains(i.Id.ToString()))
										  ;
			foreach(var nom in pItems) {
				var sNom = new SellingNomenclature {
					NomenclatureToBuy = nom,
					Parent = null,
					PosNr = (++cnt).ToString()
				};

				var children = SuplierPriceItems.Cast<ISupplierPriceNode>().Where(i => i.NomenclatureToBuy == nom).ToList();
				foreach(var i in children)
					i.Parent = sNom;
				sNom.Children = children;
				ObservablePriceNodes.Add(sNom);
			}
		}

		public virtual void AddSupplierPriceItems(Nomenclature nomenclatureFromSupplier)
		{
			foreach(SupplierPaymentType type in Enum.GetValues(typeof(SupplierPaymentType))) {
				ObservableSuplierPriceItems.Add(
					new SupplierPriceItem {
						Supplier = this,
						NomenclatureToBuy = nomenclatureFromSupplier,
						PaymentType = type
					}
				);
			}
		}

		public virtual void AddFile(CounterpartyFile file)
		{
			if (ObservableFiles.Contains(file))
			{
				return;
			}
			file.Counterparty = this;
			ObservableFiles.Add(file);
		}

		public virtual void RemoveFile(CounterpartyFile file)
		{
			if (ObservableFiles.Contains(file))
			{
				ObservableFiles.Remove(file);
			}
		}

		public virtual void RemoveNomenclatureWithPrices(int nomenclatureId)
		{
			var removableItems = new List<SupplierPriceItem>(
				ObservableSuplierPriceItems.Where(i => i.NomenclatureToBuy.Id == nomenclatureId).ToList()
			);

			foreach(var item in removableItems) {
				ObservableSuplierPriceItems.Remove(item);
			}
		}

		#endregion цены поставщика

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
			KPP = string.Empty;
			JurAddress = string.Empty;
			PhoneFrom1c = string.Empty;
		}

		#region IValidatableObject implementation

		public virtual bool CheckForINNDuplicate(ICounterpartyRepository counterpartyRepository, IUnitOfWork uow)
		{
			IList<Counterparty> counterarties = counterpartyRepository.GetCounterpartiesByINN(uow, INN);
			if(counterarties == null)
				return false;
			if(counterarties.Any(x => x.Id != Id))
				return true;
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
			
			if(CargoReceiverSource == CargoReceiverSource.Special && string.IsNullOrWhiteSpace(CargoReceiver)) {
				yield return new ValidationResult("Если выбран особый грузополучатель, необходимо ввести данные о нем");
			}
			
			if(CargoReceiver != null && CargoReceiver.Length > _cargoReceiverLimitSymbols) {
				yield return new ValidationResult(
					$"Длина строки \"Грузополучатель\" не должна превышать {_cargoReceiverLimitSymbols} символов");
			}

			if(CheckForINNDuplicate(counterpartyRepository, UoW))
			{
				yield return new ValidationResult("Контрагент с данным ИНН уже существует.",
												  new[] { this.GetPropertyName(o => o.INN) });
			}
			if(UseSpecialDocFields && PayerSpecialKPP != null && PayerSpecialKPP.Length != 9) {
				yield return new ValidationResult("Длина КПП для документов должна равнятся 9-ти.",
						new[] { this.GetPropertyName(o => o.KPP) });
			}
			if(PersonType == PersonType.legal) {
				if(TypeOfOwnership == null || TypeOfOwnership.Length == 0)
				{
					yield return new ValidationResult("Не заполнена Форма собственности.",
						new[] { nameof(TypeOfOwnership) });
				}
				if(KPP?.Length != 9 && KPP?.Length != 0 && TypeOfOwnership != "ИП")
					yield return new ValidationResult("Длина КПП должна равнятся 9-ти.",
						new[] { this.GetPropertyName(o => o.KPP) });
				if(INN.Length != 10 && INN.Length != 0 && TypeOfOwnership != "ИП")
					yield return new ValidationResult("Длина ИНН должна равнятся 10-ти.",
						new[] { this.GetPropertyName(o => o.INN) });
				if(INN.Length != 12 && INN.Length != 0 && TypeOfOwnership == "ИП")
					yield return new ValidationResult("Длина ИНН для ИП должна равнятся 12-ти.",
						new[] { this.GetPropertyName(o => o.INN) });
				if(string.IsNullOrWhiteSpace(KPP) && TypeOfOwnership != "ИП")
					yield return new ValidationResult("Для организации необходимо заполнить КПП.",
						new[] { this.GetPropertyName(o => o.KPP) });
				if(string.IsNullOrWhiteSpace(INN))
					yield return new ValidationResult("Для организации необходимо заполнить ИНН.",
						new[] { this.GetPropertyName(o => o.INN) });
				if(KPP != null && !Regex.IsMatch(KPP, "^[0-9]*$") && TypeOfOwnership != "ИП")
					yield return new ValidationResult("КПП может содержать только цифры.",
						new[] { this.GetPropertyName(o => o.KPP) });
				if(!Regex.IsMatch(INN, "^[0-9]*$"))
					yield return new ValidationResult("ИНН может содержать только цифры.",
						new[] { this.GetPropertyName(o => o.INN) });
			}

			if(IsDeliveriesClosed && String.IsNullOrWhiteSpace(CloseDeliveryComment))
				yield return new ValidationResult("Необходимо заполнить комментарий по закрытию поставок",
						new[] { this.GetPropertyName(o => o.CloseDeliveryComment) });

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

			if(Id == 0 && CameFrom == null) {
				yield return new ValidationResult("Для новых клиентов необходимо заполнить поле \"Откуда клиент\"");
			}

			if (CounterpartyType == CounterpartyType.Dealer && string.IsNullOrEmpty(OGRN)) {
				yield return new ValidationResult("Для дилеров необходимо заполнить поле \"ОГРН\"");
			}
			
			if(Id == 0 && PersonType == PersonType.legal && TaxType == TaxType.None)
				yield return new ValidationResult("Для новых клиентов необходимо заполнить поле \"Налогообложение\"");

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

			foreach (var fixedPrice in NomenclatureFixedPrices) {
				var fixedPriceValidationResults = fixedPrice.Validate(validationContext);
				foreach (var fixedPriceValidationResult in fixedPriceValidationResults) {
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

			if (TechnicalProcessingDelay > 0 && Files.Count == 0)
				yield return new ValidationResult("Для установки дней отсрочки тех обработки необходимо загрузить документ");

			StringBuilder phonesValidationStringBuilder = new StringBuilder();			
			List<string> phoneNumberDuplicatesIsChecked = new List<string>();

			var phonesDuplicates = counterpartyRepository.GetNotArchivedCounterpartiesAndDeliveryPointsDescriptionsByPhoneNumber(UoW, Phones.ToList(), this.Id);
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
		}

		#endregion
	}

	public enum PersonType
	{
		[Display(Name = "Физическое лицо")]
		natural,
		[Display(Name = "Юридическое лицо")]
		legal
	}

	public class PersonTypeStringType : NHibernate.Type.EnumStringType
	{
		public PersonTypeStringType() : base(typeof(PersonType)) { }
	}

	public enum CounterpartyType
	{
		[Display(Name = "Покупатель")]
		Buyer,
		[Display(Name = "Поставщик")]
		Supplier,
		[Display(Name = "Дилер")]
		Dealer
	}

	public class CounterpartyTypeStringType : NHibernate.Type.EnumStringType
	{
		public CounterpartyTypeStringType() : base(typeof(CounterpartyType)) { }
	}

	public enum DefaultDocumentType
	{
		[ItemTitle("УПД")]
		[Display(Name = "УПД")]
		upd,
		[ItemTitle("ТОРГ-12 + Счет-Фактура")]
		[Display(Name = "ТОРГ-12 + Счет-Фактура")]
		torg12
	}

	public class DefaultDocumentTypeStringType : NHibernate.Type.EnumStringType
	{
		public DefaultDocumentTypeStringType() : base(typeof(DefaultDocumentType)) { }
	}

	public enum CargoReceiverSource
	{
		[Display(Name = "Из контрагента")]
		FromCounterparty,
		[Display(Name = "Из точки доставки")]
		FromDeliveryPoint,
		[Display(Name = "Особый")]
		Special
	}

	public class CargoReceiverTypeStringType : NHibernate.Type.EnumStringType
	{
		public CargoReceiverTypeStringType() : base(typeof(CargoReceiverSource)) { }
	}

	public enum TaxType
	{
		[Display(Name = "Не указано")]
		None,
		[Display(Name = "С НДС")]
		WithVat,
		[Display(Name = "Без НДС")]
		WithoutVat
	}

	public class TaxTypeStringType : NHibernate.Type.EnumStringType
	{
		public TaxTypeStringType() : base(typeof(TaxType)) { }
	}

	public enum ReasonForLeaving
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "Для собственных нужд")]
		ForOwnNeeds,
		[Display(Name = "Перепродажа")]
		Resale,
		[Display(Name = "Иная")]
		Other
	}

	public class ReasonForLeavingStringType : NHibernate.Type.EnumStringType
	{
		public ReasonForLeavingStringType() : base(typeof(ReasonForLeaving)) { }
	}

	public enum RegistrationInChestnyZnakStatus
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "В процессе регистрации")]
		InProcess,
		[Display(Name = "Зарегистрирован")]
		Registered,
		[Display(Name = "Заблокирован")]
		Blocked
	}

	public class RegistrationInChestnyZnakStatusStringType : NHibernate.Type.EnumStringType
	{
		public RegistrationInChestnyZnakStatusStringType() : base(typeof(RegistrationInChestnyZnakStatus)) { }
	}

	public enum ConsentForEdoStatus
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "Отправлено")]
		Sent,
		[Display(Name = "Согласен")]
		Agree,
		[Display(Name = "Отклонено")]
		Rejected
	}

	public class ConsentForEdoStatusStringType : NHibernate.Type.EnumStringType
	{
		public ConsentForEdoStatusStringType() : base(typeof(ConsentForEdoStatus)) { }
	}

	public enum OrderStatusForSendingUpd
	{
		[Display(Name = "Доставлен")]
		Delivered,
		[Display(Name = "В пути")]
		EnRoute
	}

	public class OrderStatusForSendingUpdStringType : NHibernate.Type.EnumStringType
	{
		public OrderStatusForSendingUpdStringType() : base(typeof(OrderStatusForSendingUpd)) { }
	}

	#region Для уровневого отображения цен поставщика

	public class SellingNomenclature : ISupplierPriceNode
	{
		public int Id { get; set; }
		public Nomenclature NomenclatureToBuy { get; set; }
		public SupplierPaymentType PaymentType { get; set; }
		public decimal Price { get; set; }
		public VAT VAT { get; set; }
		public PaymentCondition PaymentCondition { get; set; }
		public DeliveryType DeliveryType { get; set; }
		public string Comment { get; set; }
		public AvailabilityForSale AvailabilityForSale { get; set; }
		public DateTime ChangingDate { get; set; }
		public Counterparty Supplier { get; set; }
		public bool IsEditable => false;
		public string PosNr { get; set; }

		public ISupplierPriceNode Parent { get; set; } = null;
		public IList<ISupplierPriceNode> Children { get; set; } = new List<ISupplierPriceNode>();
	}

	public interface ISupplierPriceNode
	{
		int Id { get; set; }
		Nomenclature NomenclatureToBuy { get; set; }
		SupplierPaymentType PaymentType { get; set; }
		decimal Price { get; set; }
		VAT VAT { get; set; }
		PaymentCondition PaymentCondition { get; set; }
		DeliveryType DeliveryType { get; set; }
		string Comment { get; set; }
		AvailabilityForSale AvailabilityForSale { get; set; }
		DateTime ChangingDate { get; set; }
		Counterparty Supplier { get; set; }
		bool IsEditable { get; }
		string PosNr { get; set; }

		ISupplierPriceNode Parent { get; set; }
		IList<ISupplierPriceNode> Children { get; set; }
	}

	#endregion Для уровневого отображения цен поставщика
}

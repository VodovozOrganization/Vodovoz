using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Domain.Client;
using VodovozInfrastructure.Attributes;

namespace Vodovoz.Core.Domain.Clients
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
	public class CounterpartyEntity : AccountOwnerBase, IDomainObject, IHasAttachedFilesInformations<CounterpartyFileInformation>
	{
		private int _id;
		private OrderStatusForSendingUpd _orderStatusForSendingUpd;
		private ConsentForEdoStatus _consentForEdoStatus;
		private bool _isNewEdoProcessing;

		private bool _roboatsExclude;
		private bool _isForSalesDepartment;
		private ReasonForLeaving _reasonForLeaving;
		private bool _isPaperlessWorkflow;
		private bool _isNotSendDocumentsByEdo;
		private bool _canSendUpdInAdvance;
		private RegistrationInChestnyZnakStatus _registrationInChestnyZnakStatus;
		private string _personalAccountIdInEdo;
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
		private int _delayDaysForProviders;
		private int _delayDaysForBuyers;
		private bool _isChainStore;
		private bool _isForRetail;
		private bool _noPhoneCall;
		private int _technicalProcessingDelay;
		private CounterpartyType _counterpartyType;
		private bool _alwaysSendReceipts;
		private bool _isLiquidating;
		private bool _sendBillByEdo;
		private bool _excludeFromAutoCalls;
		private bool _hideDeliveryPointForBill;

		private OrganizationEntity _worksThroughOrganization;
		private IObservableList<CounterpartyFileInformation> _attachedFileInformations = new ObservableList<CounterpartyFileInformation>();

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

		[Display(Name = "Работает через организацию")]
		public virtual OrganizationEntity WorksThroughOrganization
		{
			get => _worksThroughOrganization;
			set => SetField(ref _worksThroughOrganization, value);
		}

		/// <summary>
		/// Документооборот по ЭДО с клиентом осуществляется по новой схеме
		/// </summary>
		[Display(Name = "Работа с ЭДО по новой схеме")]
		public virtual bool IsNewEdoProcessing
		{
			get => _isNewEdoProcessing;
			set => SetField(ref _isNewEdoProcessing, value);
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

		public virtual DebtType? CloseDeliveryDebtType
		{
			get => _closeDeliveryDebtType;
			set => SetField(ref _closeDeliveryDebtType, value);
		}

		#endregion CloseDelivery

		[Display(Name = "Максимальный кредит")]
		public virtual decimal MaxCredit
		{
			get => _maxCredit;
			set => SetField(ref _maxCredit, value);
		}

		[Display(Name = "Форма собственности")]
		[StringLength(20)]
		public virtual string TypeOfOwnership
		{
			get => _typeOfOwnership;
			set => SetField(ref _typeOfOwnership, value);
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

		[Display(Name = "Вид оплаты")]
		public virtual PaymentType PaymentMethod
		{
			get => _paymentMethod;
			set => SetField(ref _paymentMethod, value);
		}

		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Телефон для обзвона")]
		public virtual string RingUpPhone
		{
			get => _ringUpPhone;
			set => SetField(ref _ringUpPhone, value);
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

		#endregion ЭДО и Честный знак

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

		/// <summary>
		/// Отказ от автообзвона
		/// </summary>
		[Display(Name = "Отказ от автообзвонов")]
		public virtual bool ExcludeFromAutoCalls
		{
			get => _excludeFromAutoCalls;
			set => SetField(ref _excludeFromAutoCalls, value);
		}

		[Display(Name = "Отсрочка технической обработки")]
		public virtual int TechnicalProcessingDelay
		{
			get => _technicalProcessingDelay;
			set => SetField(ref _technicalProcessingDelay, value);
		}

		[RestrictedHistoryProperty]
		[IgnoreHistoryTrace]
		[Display(Name = "Всегда отправлять чеки")]
		public virtual bool AlwaysSendReceipts
		{
			get => _alwaysSendReceipts;
			set => SetField(ref _alwaysSendReceipts, value);
		}

		[Display(Name = "Скрывать ТД в счетах")]
		public virtual bool HideDeliveryPointForBill
		{
			get => _hideDeliveryPointForBill;
			set => SetField(ref _hideDeliveryPointForBill, value);
		}

		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<CounterpartyFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

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

		public virtual void RemoveFileInformation(string fileName)
		{
			AttachedFileInformations.Remove(AttachedFileInformations.FirstOrDefault(afi => afi.FileName == fileName));
		}

		private void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.CounterpartyId = Id;
			}
		}
	}
}

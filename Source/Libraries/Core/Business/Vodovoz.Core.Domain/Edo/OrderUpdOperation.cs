using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class OrderUpdOperation : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _orderId;
		private DateTime _orderDeliveryDate;
		private int _clientContractNumber;
		private DateTime _clientContractDate;
		private string _clientName;
		private string _clientAddress;
		private string _clientInn;
		private string _clientKpp;
		private bool _useSpecialDocFields;
		private string _specialCargoReceiver;
		private string _specialCustomerName;
		private string _specialContractNumber;
		private string _payerSpecialKpp;
		private string _specialGovContract;
		private string _specialDeliveryAddress;
		private string _organizationName;
		private string _organizationAddress;
		private string _organizationInn;
		private string _organizationKpp;
		private string _buhLastName;
		private string _buhName;
		private string _buhPatronymic;
		private string _leaderLastName;
		private string _leaderName;
		private string _leaderPatronymic;
		private string _bottlesInFact;
		private bool _isSelfDelivery;
		private string _cargoReceiver;
		private string _clientInnKpp;
		private int _counterpartyExternalOrderId;
		private string _paymentsInfo;

		private IObservableList<OrderUpdOperationProduct> _goods = new ObservableList<OrderUpdOperationProduct>();

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Номер заказа
		/// </summary>
		[Display(Name = "Номер заказа")]
		public virtual int OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		/// <summary>
		/// Дата доставки заказа
		/// </summary>
		[Display(Name = "Дата доставки заказа")]
		public virtual DateTime OrderDeliveryDate
		{
			get => _orderDeliveryDate;
			set => SetField(ref _orderDeliveryDate, value);
		}

		/// <summary>
		/// Номер договора клиента
		/// </summary>
		[Display(Name = "Номер договора клиента")]
		public virtual int ClientContractNumber
		{
			get => _clientContractNumber;
			set => SetField(ref _clientContractNumber, value);
		}

		/// <summary>
		/// Дата договора клиента
		/// </summary>
		[Display(Name = "Дата договора клиента")]
		public virtual DateTime ClientContractDate
		{
			get => _clientContractDate;
			set => SetField(ref _clientContractDate, value);
		}

		/// <summary>
		/// Наименование клиента
		/// </summary>
		[Display(Name = "Наименование клиента")]
		public string ClientName
		{
			get => _clientName;
			set => SetField(ref _clientName, value);
		}

		/// <summary>
		/// Адрес клиента
		/// </summary>
		[Display(Name = "Адрес клиента")]
		public string ClientAddress
		{
			get => _clientAddress;
			set => SetField(ref _clientAddress, value);
		}

		/// <summary>
		/// ИНН клиента
		/// </summary>
		[Display(Name = "ИНН клиента")]
		public string ClientInn
		{
			get => _clientInn;
			set => SetField(ref _clientInn, value);
		}

		/// <summary>
		/// КПП клиента
		/// </summary>
		[Display(Name = "КПП клиента")]
		public string ClientKpp
		{
			get => _clientKpp;
			set => SetField(ref _clientKpp, value);
		}

		/// <summary>
		/// Особая печать документа
		/// </summary>
		[Display(Name = "Особая печать документов")]
		public bool UseSpecialDocFields
		{
			get => _useSpecialDocFields;
			set => SetField(ref _useSpecialDocFields, value);
		}

		/// <summary>
		/// Особый грузополучатель
		/// </summary>
		[Display(Name = "Особый грузополучатель")]
		public string SpecialCargoReceiver
		{
			get => _specialCargoReceiver;
			set => SetField(ref _specialCargoReceiver, value);
		}

		/// <summary>
		/// Наименование особого покупателя
		/// </summary>
		[Display(Name = "Наименование особого покупателя")]
		public string SpecialCustomerName
		{
			get => _specialCustomerName;
			set => SetField(ref _specialCustomerName, value);
		}

		/// <summary>
		/// Номер особого договора
		/// </summary>
		[Display(Name = "Номер особого договора")]
		public string SpecialContractNumber
		{
			get => _specialContractNumber;
			set => SetField(ref _specialContractNumber, value);
		}

		/// <summary>
		/// Особый КПП плательщика
		/// </summary>
		[Display(Name = "Особый КПП плательщика")]
		public string PayerSpecialKpp
		{
			get => _payerSpecialKpp;
			set => SetField(ref _payerSpecialKpp, value);
		}

		/// <summary>
		/// Идентификатор государственного контракта
		/// </summary>
		[Display(Name = "Идентификатор государственного контракта")]
		public string SpecialGovContract
		{
			get => _specialGovContract;
			set => SetField(ref _specialGovContract, value);
		}

		/// <summary>
		/// Особый адрес доставки
		/// </summary>
		[Display(Name = "Особый адрес доставки")]
		public string SpecialDeliveryAddress
		{
			get => _specialDeliveryAddress;
			set => SetField(ref _specialDeliveryAddress, value);
		}

		/// <summary>
		/// Название организации
		/// </summary>
		[Display(Name = "Название организации")]
		public string OrganizationName
		{
			get => _organizationName;
			set => SetField(ref _organizationName, value);
		}

		/// <summary>
		/// Адрес организации
		/// </summary>
		[Display(Name = "Адрес организации")]
		public string OrganizationAddress
		{
			get => _organizationAddress;
			set => SetField(ref _organizationAddress, value);
		}

		/// <summary>
		/// ИНН организации
		/// </summary>
		[Display(Name = "ИНН организации")]
		public string OrganizationInn
		{
			get => _organizationInn;
			set => SetField(ref _organizationInn, value);
		}

		/// <summary>
		/// КПП организации
		/// </summary>
		[Display(Name = "КПП организации")]
		public string OrganizationKpp
		{
			get => _organizationKpp;
			set => SetField(ref _organizationKpp, value);
		}

		/// <summary>
		/// Фамилия бухгалтера
		/// </summary>
		[Display(Name = "Фамилия бухгалтера")]
		public string BuhLastName
		{
			get => _buhLastName;
			set => SetField(ref _buhLastName, value);
		}

		/// <summary>
		/// Имя бухгалтера
		/// </summary>
		[Display(Name = "Имя бухгалтера")]
		public string BuhName
		{
			get => _buhName;
			set => SetField(ref _buhName, value);
		}

		/// <summary>
		/// Отчество бухгалтера
		/// </summary>
		[Display(Name = "Отчество бухгалтера")]
		public string BuhPatronymic
		{
			get => _buhPatronymic;
			set => SetField(ref _buhPatronymic, value);
		}

		/// <summary>
		/// Фамилия руководителя
		/// </summary>
		[Display(Name = "Фамилия руководителя")]
		public string LeaderLastName
		{
			get => _leaderLastName;
			set => SetField(ref _leaderLastName, value);
		}

		/// <summary>
		/// Имя руководителя
		/// </summary>
		[Display(Name = "Имя руководителя")]
		public string LeaderName
		{
			get => _leaderName;
			set => SetField(ref _leaderName, value);
		}

		/// <summary>
		/// Отчество руководителя
		/// </summary>
		[Display(Name = "Отчество руководителя")]
		public string LeaderPatronymic
		{
			get => _leaderPatronymic;
			set => SetField(ref _leaderPatronymic, value);
		}

		/// <summary>
		/// Количество сданной пустой тары
		/// </summary>
		[Display(Name = "Количество сданной пустой тары")]
		public string BottlesInFact
		{
			get => _bottlesInFact;
			set => SetField(ref _bottlesInFact, value);
		}

		/// <summary>
		/// Заказ с самовывозом
		/// </summary>
		[Display(Name = "Заказ с самовывозом")]
		public bool IsSelfDelivery
		{
			get => _isSelfDelivery;
			set => SetField(ref _isSelfDelivery, value);
		}

		/// <summary>
		/// Грузополучатель
		/// </summary>
		[Display(Name = "Грузополучатель")]
		public string CargoReceiver
		{
			get => _cargoReceiver;
			set => SetField(ref _cargoReceiver, value);
		}

		/// <summary>
		/// ИНН и КПП клиента
		/// </summary>
		[Display(Name = "ИНН и КПП клиента")]
		public string ClientInnKpp
		{
			get => _clientInnKpp;
			set => SetField(ref _clientInnKpp, value);
		}

		/// <summary>
		/// Номер заказа покупателя
		/// </summary>
		[Display(Name = "Номер заказа покупателя")]
		public int CounterpartyExternalOrderId
		{
			get { return _counterpartyExternalOrderId; }
			set => SetField(ref _counterpartyExternalOrderId, value);
		}

		/// <summary>
		/// Информация о платеже
		/// </summary>
		[Display(Name = "Информация о платеже")]
		public string PaymentsInfo
		{
			get { return _paymentsInfo; }
			set => SetField(ref _paymentsInfo, value);
		}

		/// <summary>
		/// Товары
		/// </summary>
		[Display(Name = "Товары")]
		public virtual IObservableList<OrderUpdOperationProduct> Goods
		{
			get => _goods;
			set => SetField(ref _goods, value);
		}
	}
}

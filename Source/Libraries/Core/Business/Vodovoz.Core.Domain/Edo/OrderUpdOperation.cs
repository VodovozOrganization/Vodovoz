using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Операция УПД по заказу
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "операция УПД по заказу",
		NominativePlural = "операции УПД по заказам"
	)]
	public class OrderUpdOperation : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _orderId;
		private int? _counterpartyExternalOrderId;
		private bool _isOrderForOwnNeeds;
		private DateTime _orderDeliveryDate;

		private string _clientContractDocumentName;
		private string _clientContractNumber;
		private DateTime _clientContractDate;

		private string _clientName;
		private string _clientAddress;
		private string _clientInn;
		private string _clientKpp;

		private string _consigneeName;
		private string _consigneeAddress;
		private string _consigneeInn;
		private string _consigneeKpp;
		private string _consigneeSummary;

		private string _organizationName;
		private string _organizationAddress;
		private string _organizationInn;
		private string _organizationKpp;
		private string _organizationTaxcomEdoAccountId;

		private string _buhLastName;
		private string _buhName;
		private string _buhPatronymic;
		private string _leaderLastName;
		private string _leaderName;
		private string _leaderPatronymic;
		private string _bottlesInFact;
		private bool _isSelfDelivery;

		private IObservableList<OrderUpdOperationPayment> _payments = new ObservableList<OrderUpdOperationPayment>();
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
		/// Номер заказа покупателя
		/// </summary>
		[Display(Name = "Номер заказа покупателя")]
		public virtual int? CounterpartyExternalOrderId
		{
			get => _counterpartyExternalOrderId;
			set => SetField(ref _counterpartyExternalOrderId, value);
		}

		/// <summary>
		/// Заказ для собственных нужд
		/// </summary>
		[Display(Name = "Заказ для собственных нужд")]
		public virtual bool IsOrderForOwnNeeds
		{
			get => _isOrderForOwnNeeds;
			set => SetField(ref _isOrderForOwnNeeds, value);
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
		/// Наименование договора клиента
		/// </summary>
		[Display(Name = "Наименование договора клиента")]
		public virtual string ClientContractDocumentName
		{
			get => _clientContractDocumentName;
			set => SetField(ref _clientContractDocumentName, value);
		}

		/// <summary>
		/// Номер договора клиента
		/// </summary>
		[Display(Name = "Номер договора клиента")]
		public virtual string ClientContractNumber
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
		public virtual string ClientName
		{
			get => _clientName;
			set => SetField(ref _clientName, value);
		}

		/// <summary>
		/// Адрес клиента
		/// </summary>
		[Display(Name = "Адрес клиента")]
		public virtual string ClientAddress
		{
			get => _clientAddress;
			set => SetField(ref _clientAddress, value);
		}

		/// <summary>
		/// ИНН клиента
		/// </summary>
		[Display(Name = "ИНН клиента")]
		public virtual string ClientInn
		{
			get => _clientInn;
			set => SetField(ref _clientInn, value);
		}

		/// <summary>
		/// КПП клиента
		/// </summary>
		[Display(Name = "КПП клиента")]
		public virtual string ClientKpp
		{
			get => _clientKpp;
			set => SetField(ref _clientKpp, value);
		}

		/// <summary>
		/// Наименование грузополучателя
		/// </summary>
		[Display(Name = "Наименование грузополучателя")]
		public virtual string ConsigneeName
		{
			get => _consigneeName;
			set => SetField(ref _consigneeName, value);
		}

		/// <summary>
		/// Адрес грузополучателя
		/// </summary>
		[Display(Name = "Адрес грузополучателя")]
		public virtual string ConsigneeAddress
		{
			get => _consigneeAddress;
			set => SetField(ref _consigneeAddress, value);
		}

		/// <summary>
		/// ИНН грузополучателя
		/// </summary>
		[Display(Name = "ИНН грузополучателя")]
		public virtual string ConsigneeInn
		{
			get => _consigneeInn;
			set => SetField(ref _consigneeInn, value);
		}

		/// <summary>
		/// КПП грузополучателя
		/// </summary>
		[Display(Name = "КПП грузополучателя")]
		public virtual string ConsigneeKpp
		{
			get => _consigneeKpp;
			set => SetField(ref _consigneeKpp, value);
		}


		/// <summary>
		/// Суммарная информация о грузополучателе
		/// </summary>
		[Display(Name = "Суммарная информация о грузополучателе")]
		public virtual string ConsigneeSummary
		{
			get => _consigneeSummary;
			set => SetField(ref _consigneeSummary, value);
		}

		/// <summary>
		/// Название организации
		/// </summary>
		[Display(Name = "Название организации")]
		public virtual string OrganizationName
		{
			get => _organizationName;
			set => SetField(ref _organizationName, value);
		}

		/// <summary>
		/// Адрес организации
		/// </summary>
		[Display(Name = "Адрес организации")]
		public virtual string OrganizationAddress
		{
			get => _organizationAddress;
			set => SetField(ref _organizationAddress, value);
		}

		/// <summary>
		/// ИНН организации
		/// </summary>
		[Display(Name = "ИНН организации")]
		public virtual string OrganizationInn
		{
			get => _organizationInn;
			set => SetField(ref _organizationInn, value);
		}

		/// <summary>
		/// КПП организации
		/// </summary>
		[Display(Name = "КПП организации")]
		public virtual string OrganizationKpp
		{
			get => _organizationKpp;
			set => SetField(ref _organizationKpp, value);
		}

		/// <summary>
		/// Id кабинета в Такскоме
		[Display(Name = "Id кабинета в Такскоме")]
		/// </summary>
		public virtual string OrganizationTaxcomEdoAccountId
		{
			get => _organizationTaxcomEdoAccountId;
			set => SetField(ref _organizationTaxcomEdoAccountId, value);
		}

		/// <summary>
		/// Фамилия бухгалтера
		/// </summary>
		[Display(Name = "Фамилия бухгалтера")]
		public virtual string BuhLastName
		{
			get => _buhLastName;
			set => SetField(ref _buhLastName, value);
		}

		/// <summary>
		/// Имя бухгалтера
		/// </summary>
		[Display(Name = "Имя бухгалтера")]
		public virtual string BuhName
		{
			get => _buhName;
			set => SetField(ref _buhName, value);
		}

		/// <summary>
		/// Отчество бухгалтера
		/// </summary>
		[Display(Name = "Отчество бухгалтера")]
		public virtual string BuhPatronymic
		{
			get => _buhPatronymic;
			set => SetField(ref _buhPatronymic, value);
		}

		/// <summary>
		/// Фамилия руководителя
		/// </summary>
		[Display(Name = "Фамилия руководителя")]
		public virtual string LeaderLastName
		{
			get => _leaderLastName;
			set => SetField(ref _leaderLastName, value);
		}

		/// <summary>
		/// Имя руководителя
		/// </summary>
		[Display(Name = "Имя руководителя")]
		public virtual string LeaderName
		{
			get => _leaderName;
			set => SetField(ref _leaderName, value);
		}

		/// <summary>
		/// Отчество руководителя
		/// </summary>
		[Display(Name = "Отчество руководителя")]
		public virtual string LeaderPatronymic
		{
			get => _leaderPatronymic;
			set => SetField(ref _leaderPatronymic, value);
		}

		/// <summary>
		/// Количество сданной пустой тары
		/// </summary>
		[Display(Name = "Количество сданной пустой тары")]
		public virtual string BottlesInFact
		{
			get => _bottlesInFact;
			set => SetField(ref _bottlesInFact, value);
		}

		/// <summary>
		/// Заказ с самовывозом
		/// </summary>
		[Display(Name = "Заказ с самовывозом")]
		public virtual bool IsSelfDelivery
		{
			get => _isSelfDelivery;
			set => SetField(ref _isSelfDelivery, value);
		}

		/// <summary>
		/// Платежи
		/// </summary>
		[Display(Name = "Платежи")]
		public virtual IObservableList<OrderUpdOperationPayment> Payments
		{
			get => _payments;
			set => SetField(ref _payments, value);
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

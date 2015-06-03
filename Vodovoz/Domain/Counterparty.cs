using System;
using QSOrmProject;
using System.Data.Bindings;
using System.Collections.Generic;
using QSContacts;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using QSProjectsLib;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = GrammaticalGender.Masculine,
		NominativePlural = "контрагенты",
		Nominative = "контрагент",
		Accusative = "контрагента",
		Genitive = "контрагента"
	)]
	public class Counterparty : QSBanks.AccountOwnerBase, IDomainObject, IContractOwner, IContactOwner, IProxyOwner, IDeliveryPointOwner
	{
		private IList<CounterpartyContract> counterpartyContracts;

		#region IContractOwner implementation

		[Display(Name = "Договоры")]
		public IList<CounterpartyContract> CounterpartyContracts {
			get { return counterpartyContracts; }
			set { SetField (ref counterpartyContracts, value, () => CounterpartyContracts); }
		}

		#endregion

		private IList<DeliveryPoint> deliveryPoints;

		#region IDeliveryPointOwner implementation

		[Display(Name = "Точки доставки")]
		public IList<DeliveryPoint> DeliveryPoints {
			get { return deliveryPoints; }
			set { SetField (ref deliveryPoints, value, () => DeliveryPoints); }
		}

		#endregion

		private IList<Contact> contact;

		#region IContact implementation

		[Display(Name = "Контактные лица")]
		public virtual IList<Contact> Contacts {
			get { return contact; }
			set { SetField (ref contact, value, () => Contacts); }
		}

		#endregion

		private IList<Proxy> proxies;

		#region IProxyOwner implementation

		[Display(Name = "Доверенности")]
		public virtual IList<Proxy> Proxies {
			get { return proxies; }
			set { SetField (ref proxies, value, () => Proxies); }
		}

		#endregion

		#region Свойства

		public virtual int Id { get; set; }

		decimal maxCredit;

		[Display(Name = "Максимальный кредит")]
		public virtual decimal MaxCredit {
			get { return maxCredit; }
			set { SetField (ref maxCredit, value, () => MaxCredit); }
		}

		string name;

		[Required (ErrorMessage = "Название контрагента должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string fullName;
		[Display(Name = "Полное название")]
		public virtual string FullName {
			get { return fullName; }
			set { SetField (ref fullName, value, () => FullName); }
		}

		string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		string waybillComment;
		[Display(Name = "Комментарий для накладной")]
		public virtual string WaybillComment {
			get { return waybillComment; }
			set { SetField (ref waybillComment, value, () => WaybillComment); }
		}

		string iNN;

		[Digits (ErrorMessage = "ИНН может содержать только цифры.")]
		[StringLength (12, MinimumLength = 0, ErrorMessage = "Номер ИНН не должен превышать 12.")]
		[Display(Name = "ИНН")]
		public virtual string INN {
			get { return iNN; }
			set { SetField (ref iNN, value, () => INN); }
		}

		string kPP;

		[Display(Name = "КПП")]
		[Digits (ErrorMessage = "КПП может содержать только цифры.")]
		[StringLength (9, MinimumLength = 0, ErrorMessage = "Номер КПП не должен превышать 9 цифр.")]
		public virtual string KPP {
			get { return kPP; }
			set { SetField (ref kPP, value, () => KPP); }
		}

		string jurAddress;
		[Display(Name = "Юридический адрес")]
		public virtual string JurAddress {
			get { return jurAddress; }
			set { SetField (ref jurAddress, value, () => JurAddress); }
		}

		Payment paymentMethod;
		[Display(Name = "Вид оплаты")]
		public virtual Payment PaymentMethod {
			get { return paymentMethod; }
			set { SetField (ref paymentMethod, value, () => PaymentMethod); }
		}

		PersonType personType;

		[Display(Name = "Форма контрагента")]
		public virtual PersonType PersonType {
			get { return personType; }
			set { SetField (ref personType, value, () => PersonType); }
		}

		Significance significance;

		[Display(Name = "Значимость")]
		public virtual Significance Significance {
			get { return significance; }
			set { SetField (ref significance, value, () => Significance); }
		}

		Counterparty mainCounterparty;

		[Display(Name = "Головная организация")]
		public virtual Counterparty MainCounterparty {
			get { return mainCounterparty; }
			set { SetField (ref mainCounterparty, value, () => MainCounterparty); }
		}

		CounterpartyType counterpartyType;
		[Display(Name = "Тип контрагента")]
		public virtual CounterpartyType CounterpartyType {
			get { return counterpartyType; }
			set { SetField (ref counterpartyType, value, () => CounterpartyType); }
		}

		CounterpartyStatus status;
		[Display(Name = "Статус")]
		public virtual CounterpartyStatus Status {
			get { return status; }
			set { SetField (ref status, value, () => Status); }
		}

		IList<Phone> phones;
		[Display(Name = "Телефоны")]
		public virtual IList<Phone> Phones {
			get { return phones; }
			set { SetField (ref phones, value, () => Phones); }
		}

		IList<Email> emails;
		[Display(Name = "E-mail адреса")]
		public virtual IList<Email> Emails {
			get { return emails; }
			set { SetField (ref emails, value, () => Emails); }
		}

		Employee accountant;
		[Display(Name = "Бухгалтер")]
		public virtual Employee Accountant {
			get { return accountant; }
			set { SetField (ref accountant, value, () => Accountant); }
		}

		Employee salesManager;
		[Display(Name = "Менеджер по продажам")]
		public virtual Employee SalesManager {
			get { return salesManager; }
			set { SetField (ref salesManager, value, () => SalesManager); }
		}

		Employee bottlesManager;
		[Display(Name = "Менеджер по бутылям")]
		public virtual Employee BottlesManager {
			get { return bottlesManager; }
			set { SetField (ref bottlesManager, value, () => BottlesManager); }
		}

		Contact mainContact;

		[Display(Name = "Главное контактное лицо")]
		public virtual Contact MainContact {
			get { return mainContact; }
			set { SetField (ref mainContact, value, () => MainContact); }
		}

		Contact financialContact;

		[Display(Name = "Контакт по финансовым вопросам")]
		public virtual Contact FinancialContact {
			get { return financialContact; }
			set { SetField (ref financialContact, value, () => FinancialContact); }
		}

		#endregion

		public Counterparty ()
		{
			Name = String.Empty;
			FullName = String.Empty;
			Comment = String.Empty;
			WaybillComment = String.Empty;
			INN = String.Empty;
			KPP = String.Empty;
			JurAddress = String.Empty;
		}
	}

	public enum PersonType
	{
		[ItemTitleAttribute ("Физическое лицо")]
		natural,
		[ItemTitleAttribute ("Юридическое лицо")]
		legal
	}

	public class PersonTypeStringType : NHibernate.Type.EnumStringType
	{
		public PersonTypeStringType () : base (typeof(PersonType))
		{
		}
	}

	public enum Payment
	{
		[ItemTitleAttribute ("Наличная")]
		cash,
		[ItemTitleAttribute ("Безналичная")]
		cashless
	}

	public class PaymentStringType : NHibernate.Type.EnumStringType
	{
		public PaymentStringType () : base (typeof(Payment))
		{
		}
	}

	public enum CounterpartyType
	{
		[ItemTitleAttribute ("Покупатель")]
		customer,
		[ItemTitleAttribute ("Поставщик")]
		supplier,
		[ItemTitleAttribute ("Партнер")]
		partner,
	}

	public class CounterpartyTypeStringType : NHibernate.Type.EnumStringType
	{
		public CounterpartyTypeStringType () : base (typeof(CounterpartyType))
		{
		}
	}

	public interface IContractOwner
	{
		IList<CounterpartyContract> CounterpartyContracts { get; set; }
	}
}


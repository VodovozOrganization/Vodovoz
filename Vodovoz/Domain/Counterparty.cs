using System;
using QSOrmProject;
using System.Data.Bindings;
using System.Collections.Generic;
using QSContacts;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Vodovoz
{
	[OrmSubject ("Контрагенты")]
	public class Counterparty : QSBanks.AccountOwnerBase, QSContacts.IContactOwner, IProxyOwner, IDeliveryPointOwner
	{
		private IList<DeliveryPoint> deliveryPoints;

		#region IDeliveryPointOwner implementation

		public IList<DeliveryPoint> DeliveryPoints {
			get { return deliveryPoints; }
			set { SetField (ref deliveryPoints, value, () => DeliveryPoints); }
		}

		#endregion

		private IList<Contact> contact;

		#region IContact implementation

		public virtual IList<Contact> Contacts {
			get { return contact; }
			set { SetField (ref contact, value, () => Contacts); }
		}

		#endregion

		private IList<Proxy> proxies;

		#region IProxyOwner implementation

		public virtual IList<Proxy> Proxies {
			get { return proxies; }
			set { SetField (ref proxies, value, () => Proxies); }
		}

		#endregion

		#region Свойства

		public virtual int Id { get; set; }

		decimal maxCredit;

		public virtual decimal MaxCredit {
			get { return maxCredit; }
			set { SetField (ref maxCredit, value, () => MaxCredit); }
		}

		string name;

		[Required (ErrorMessage = "Название контрагента должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string fullName;

		public virtual string FullName {
			get { return fullName; }
			set { SetField (ref fullName, value, () => FullName); }
		}

		string comment;

		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		string waybillComment;

		public virtual string WaybillComment {
			get { return waybillComment; }
			set { SetField (ref waybillComment, value, () => WaybillComment); }
		}

		string iNN;

		[Digits (ErrorMessage = "ИНН может содержать только цифры.")]
		[StringLength (12, MinimumLength = 0, ErrorMessage = "Номер ИНН не должен превышать 12.")]
		public virtual string INN {
			get { return iNN; }
			set { SetField (ref iNN, value, () => INN); }
		}

		string kPP;

		[Digits (ErrorMessage = "КПП может содержать только цифры.")]
		[StringLength (9, MinimumLength = 0, ErrorMessage = "Номер КПП не должен превышать 9 цифр.")]
		public virtual string KPP {
			get { return kPP; }
			set { SetField (ref kPP, value, () => KPP); }
		}

		string jurAddress;

		public virtual string JurAddress {
			get { return jurAddress; }
			set { SetField (ref jurAddress, value, () => JurAddress); }
		}

		Payment paymentMethod;

		public virtual Payment PaymentMethod {
			get { return paymentMethod; }
			set { SetField (ref paymentMethod, value, () => PaymentMethod); }
		}

		PersonType personType;

		public virtual PersonType PersonType {
			get { return personType; }
			set { SetField (ref personType, value, () => PersonType); }
		}

		Significance significance;

		public virtual Significance Significance {
			get { return significance; }
			set { SetField (ref significance, value, () => Significance); }
		}

		Counterparty mainCounterparty;

		public virtual Counterparty MainCounterparty {
			get { return mainCounterparty; }
			set { SetField (ref mainCounterparty, value, () => MainCounterparty); }
		}

		CounterpartyContract contract;

		public virtual CounterpartyContract Contract {
			get { return contract; }
			set { SetField (ref contract, value, () => Contract); }
		}

		CounterpartyType counterpartyType;

		public virtual CounterpartyType CounterpartyType {
			get { return counterpartyType; }
			set { SetField (ref counterpartyType, value, () => CounterpartyType); }
		}

		CounterpartyStatus status;

		public virtual CounterpartyStatus Status {
			get { return status; }
			set { SetField (ref status, value, () => Status); }
		}

		IList<Phone> phones;

		public virtual IList<Phone> Phones {
			get { return phones; }
			set { SetField (ref phones, value, () => Phones); }
		}

		IList<Email> emails;

		public virtual IList<Email> Emails {
			get { return emails; }
			set { SetField (ref emails, value, () => Emails); }
		}

		Employee accountant;

		public virtual Employee Accountant {
			get { return accountant; }
			set { SetField (ref accountant, value, () => Accountant); }
		}

		Employee salesManager;

		public virtual Employee SalesManager {
			get { return salesManager; }
			set { SetField (ref salesManager, value, () => SalesManager); }
		}

		Employee bottlesManager;

		public virtual Employee BottlesManager {
			get { return bottlesManager; }
			set { SetField (ref bottlesManager, value, () => BottlesManager); }
		}

		Contact mainContact;

		public virtual Contact MainContact {
			get { return mainContact; }
			set { SetField (ref mainContact, value, () => MainContact); }
		}

		Contact financialContact;

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
}


using System;
using QSOrmProject;
using System.Data.Bindings;
using System.Collections.Generic;
using QSContacts;
using System.ComponentModel.DataAnnotations;
using QSProjectsLib;
using System.Data.Bindings.Collections.Generic;
using System.Text.RegularExpressions;
using Vodovoz.Domain.Cash;
using Gamma.Utilities;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = GrammaticalGender.Masculine,
		NominativePlural = "контрагенты",
		Nominative = "контрагент",
		Accusative = "контрагента",
		Genitive = "контрагента"
	)]
	public class Counterparty : QSBanks.AccountOwnerBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		private IList<CounterpartyContract> counterpartyContracts;

		[Display (Name = "Договоры")]
		public virtual IList<CounterpartyContract> CounterpartyContracts {
			get { return counterpartyContracts; }
			set { SetField (ref counterpartyContracts, value, () => CounterpartyContracts); }
		}

		private IList<DeliveryPoint> deliveryPoints;

		[Display (Name = "Точки доставки")]
		public virtual IList<DeliveryPoint> DeliveryPoints {
			get { return deliveryPoints; }
			set { SetField (ref deliveryPoints, value, () => DeliveryPoints); }
		}

		GenericObservableList<DeliveryPoint> observableDeliveryPoints;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryPoint> ObservableDeliveryPoints {
			get {
				if (observableDeliveryPoints == null)
					observableDeliveryPoints = new GenericObservableList<DeliveryPoint> (DeliveryPoints);
				return observableDeliveryPoints;
			}
		}

		private IList<Contact> contact = new List<Contact> ();

		[Display (Name = "Контактные лица")]
		public virtual IList<Contact> Contacts {
			get { return contact; }
			set { SetField (ref contact, value, () => Contacts); }
		}

		private IList<Proxy> proxies;

		[Display (Name = "Доверенности")]
		public virtual IList<Proxy> Proxies {
			get { return proxies; }
			set { SetField (ref proxies, value, () => Proxies); }
		}

		public virtual int Id { get; set; }

		decimal maxCredit;

		[Display (Name = "Максимальный кредит")]
		public virtual decimal MaxCredit {
			get { return maxCredit; }
			set { SetField (ref maxCredit, value, () => MaxCredit); }
		}

		string name;

		[Required (ErrorMessage = "Название контрагента должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { 
				if (SetField (ref name, value, () => Name)) {
					if (PersonType == PersonType.natural) {
						FullName = Name;
					}
				}
			}
		}

		string typeOfOwnership;

		[Display (Name = "Форма собственности")]
		[StringLength (10)]
		public virtual string TypeOfOwnership {
			get { return typeOfOwnership; }
			set { SetField (ref typeOfOwnership, value, () => TypeOfOwnership); }
		}

		string fullName;

		[Display (Name = "Полное название")]
		public virtual string FullName {
			get { return fullName; }
			set { SetField (ref fullName, value, () => FullName); }
		}

		string code1c;

		public virtual string Code1c {
			get { return code1c; }
			set { SetField (ref code1c, value, () => Code1c); }
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		string waybillComment;

		[Display (Name = "Комментарий для накладной")]
		public virtual string WaybillComment {
			get { return waybillComment; }
			set { SetField (ref waybillComment, value, () => WaybillComment); }
		}

		string iNN;

		[Display (Name = "ИНН")]
		public virtual string INN {
			get { return iNN; }
			set { SetField (ref iNN, value, () => INN); }
		}

		string kPP;

		[Display (Name = "КПП")]
		public virtual string KPP {
			get { return kPP; }
			set { SetField (ref kPP, value, () => KPP); }
		}

		string jurAddress;

		[Display (Name = "Юридический адрес")]
		public virtual string JurAddress {
			get { return jurAddress; }
			set { SetField (ref jurAddress, value, () => JurAddress); }
		}

		string address;

		[Display (Name = "Фактический адрес")]
		public virtual string Address {
			get { return address; }
			set { SetField (ref address, value, () => Address); }
		}

		PaymentType paymentMethod;

		[Display (Name = "Вид оплаты")]
		public virtual PaymentType PaymentMethod {
			get { return paymentMethod; }
			set { SetField (ref paymentMethod, value, () => PaymentMethod); }
		}

		PersonType personType;

		[Display (Name = "Форма контрагента")]
		public virtual PersonType PersonType {
			get { return personType; }
			set { SetField (ref personType, value, () => PersonType); }
		}

		ExpenseCategory defaultExpenseCategory;

		[Display (Name = "Расход по-умолчанию")]
		public virtual ExpenseCategory DefaultExpenseCategory {
			get { return defaultExpenseCategory; }
			set { SetField (ref defaultExpenseCategory, value, () => DefaultExpenseCategory); }
		}

		Significance significance;

		[Display (Name = "Значимость")]
		public virtual Significance Significance {
			get { return significance; }
			set { SetField (ref significance, value, () => Significance); }
		}

		Counterparty mainCounterparty;

		[Display (Name = "Головная организация")]
		public virtual Counterparty MainCounterparty {
			get { return mainCounterparty; }
			set { SetField (ref mainCounterparty, value, () => MainCounterparty); }
		}

		CounterpartyType counterpartyType;

		[Display (Name = "Тип контрагента")]
		public virtual CounterpartyType CounterpartyType {
			get { return counterpartyType; }
			set { SetField (ref counterpartyType, value, () => CounterpartyType); }
		}

		CounterpartyStatus status;

		[Display (Name = "Статус")]
		public virtual CounterpartyStatus Status {
			get { return status; }
			set { SetField (ref status, value, () => Status); }
		}

		IList<Phone> phones;

		[Display (Name = "Телефоны")]
		public virtual IList<Phone> Phones {
			get { return phones; }
			set { SetField (ref phones, value, () => Phones); }
		}

		IList<Email> emails;

		[Display (Name = "E-mail адреса")]
		public virtual IList<Email> Emails {
			get { return emails; }
			set { SetField (ref emails, value, () => Emails); }
		}

		Employee accountant;

		[Display (Name = "Бухгалтер")]
		public virtual Employee Accountant {
			get { return accountant; }
			set { SetField (ref accountant, value, () => Accountant); }
		}

		Employee salesManager;

		[Display (Name = "Менеджер по продажам")]
		public virtual Employee SalesManager {
			get { return salesManager; }
			set { SetField (ref salesManager, value, () => SalesManager); }
		}

		Employee bottlesManager;

		[Display (Name = "Менеджер по бутылям")]
		public virtual Employee BottlesManager {
			get { return bottlesManager; }
			set { SetField (ref bottlesManager, value, () => BottlesManager); }
		}

		Contact mainContact;

		[Display (Name = "Главное контактное лицо")]
		public virtual Contact MainContact {
			get { return mainContact; }
			set { SetField (ref mainContact, value, () => MainContact); }
		}

		Contact financialContact;

		[Display (Name = "Контакт по финансовым вопросам")]
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

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (PersonType == PersonType.legal) {
				if (KPP.Length != 9 && KPP.Length != 0)
					yield return new ValidationResult ("Длина КПП должна равнятся 9-ти.",
						new[] { this.GetPropertyName (o => o.KPP) });
				if (INN.Length != 10 && INN.Length != 0)
					yield return new ValidationResult ("Длина ИНН должна равнятся 10-ти.",
						new[] { this.GetPropertyName (o => o.INN) });
				if (String.IsNullOrWhiteSpace (KPP))
					yield return new ValidationResult ("Для организации необходимо заполнить КПП.",
						new[] { this.GetPropertyName (o => o.KPP) });
				if (String.IsNullOrWhiteSpace (INN))
					yield return new ValidationResult ("Для организации необходимо заполнить ИНН.",
						new[] { this.GetPropertyName (o => o.INN) });
				if (!Regex.IsMatch (KPP, "^[0-9]*$"))
					yield return new ValidationResult ("КПП может содержать только цифры.",
						new[] { this.GetPropertyName (o => o.KPP) });
				if (!Regex.IsMatch (INN, "^[0-9]*$"))
					yield return new ValidationResult ("ИНН может содержать только цифры.",
						new[] { this.GetPropertyName (o => o.INN) });
			}
		}

		#endregion
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


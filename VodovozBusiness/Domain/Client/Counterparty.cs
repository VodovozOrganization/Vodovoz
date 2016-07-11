using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gamma.Utilities;
using QSContacts;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Client
{
	[OrmSubject (Gender = GrammaticalGender.Masculine,
		NominativePlural = "контрагенты",
		Nominative = "контрагент",
		Accusative = "контрагента",
		Genitive = "контрагента"
	)]
	public class Counterparty : QSBanks.AccountOwnerBase, IDomainObject, IValidatableObject
	{
		//Используется для валидации, не получается истолльзовать бизнес объект так как наследуемся от AccountOwnerBase
		public virtual IUnitOfWork UoW { get; set;}

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

		bool isArchive;

		[Display (Name = "Архивный")]
		public virtual bool IsArchive {
			get { return isArchive; }
			set { SetField (ref isArchive, value, () => IsArchive); }
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

		DefaultDocumentType ? defaultDocumentType;

		[Display (Name = "Тип безналичных документов по-умолчанию")]
		public virtual DefaultDocumentType ? DefaultDocumentType {
			get { return defaultDocumentType; }
			set { SetField(ref defaultDocumentType, value, () => DefaultDocumentType); }
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

			if(IsArchive)
			{
				var unclosedContracts = CounterpartyContracts.Where(c => !c.IsArchive)
					.Select(c => c.Id.ToString()).ToList();
				if(unclosedContracts.Count > 0)
					yield return new ValidationResult (
						String.Format("Вы не можете сдать контрагента в архив с открытыми договорами: {0}", String.Join(", ", unclosedContracts)),
						new[] { this.GetPropertyName (o => o.CounterpartyContracts) });

				var balance = Repository.Operations.MoneyRepository.GetCounterpartyDebt(UoW, this);
				if(balance != 0)
					yield return new ValidationResult (
						String.Format("Вы не можете сдать контрагента в архив так как у него имеется долг: {0}", CurrencyWorks.GetShortCurrencyString(balance)));

				var activeOrders = Repository.OrderRepository.GetCurrentOrders(UoW, this);
				if(activeOrders.Count > 0)
					yield return new ValidationResult (
						String.Format("Вы не можете сдать контрагента в архив с незакрытыми заказами: {0}", String.Join(", ", activeOrders.Select(o => o.Id.ToString()))),
						new[] { this.GetPropertyName (o => o.CounterpartyContracts) });
				
				var deposit = Repository.Operations.DepositRepository.GetDepositsAtCounterparty(UoW, this);
				if(balance != 0)
					yield return new ValidationResult (
						String.Format("Вы не можете сдать контрагента в архив так как у него есть невозвращенные залоги: {0}", CurrencyWorks.GetShortCurrencyString(deposit)));

				var bottles = Repository.Operations.BottlesRepository.GetBottlesAtCounterparty(UoW, this);
				if(balance != 0)
					yield return new ValidationResult (
						String.Format("Вы не можете сдать контрагента в архив так как он не вернул {0} бутылей", bottles));
				
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

	public enum DefaultDocumentType
	{
		[ItemTitleAttribute ("УПД")]
		[Display (Name = "УПД")]
		upd,
		[ItemTitleAttribute ("ТОРГ-12 + Счет-Фактура")]
		[Display (Name = "ТОРГ-12 + Счет-Фактура")]
		torg12
	}

	public class DefaultDocumentTypeStringType : NHibernate.Type.EnumStringType
	{
		public DefaultDocumentTypeStringType () : base (typeof(DefaultDocumentType))
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


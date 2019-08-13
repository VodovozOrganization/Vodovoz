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
using QS.Project.Repositories;
using QSContacts;
using QSProjectsLib;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Repositories.Orders;
using Vodovoz.Repository;

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
			set => SetField(ref typeOfOwnership, value, () => TypeOfOwnership);
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
			set {
				if(SetField(ref paymentMethod, value, () => PaymentMethod)) {
					if(!CounterpartyRepository.IsCashPayment(PaymentMethod))
						NeedCheque = null;
					else
						NeedCheque = ChequeResponse.Unknown;
				}
			}
		}

		PersonType personType;

		[Display(Name = "Форма контрагента")]
		public virtual PersonType PersonType {
			get => personType;
			set => SetField(ref personType, value, () => PersonType);
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

		IList<Phone> phones;

		[Display(Name = "Телефоны")]
		public virtual IList<Phone> Phones {
			get => phones;
			set => SetField(ref phones, value, () => Phones);
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

		#region ОсобаяПечать
		bool useSpecialDocFields;

		[Display(Name = "Особая печать документов ")]
		public virtual bool UseSpecialDocFields {
			get => useSpecialDocFields;
			set => SetField(ref useSpecialDocFields, value, () => UseSpecialDocFields);
		}

		string contractNumber;
		[Display(Name = "Особый номер договора")]
		public virtual string SpecialContractNumber {
			get => contractNumber;
			set => SetField(ref contractNumber, value, () => SpecialContractNumber);
		}

		string specialKPP;
		[Display(Name = "Особый КПП")]
		public virtual string SpecialKPP {
			get => specialKPP;
			set => SetField(ref specialKPP, value, () => SpecialKPP);
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

		#endregion ОсобаяПечать

		ChequeResponse? needCheque;
		[Display(Name = "Требуется печать чека")]
		public virtual ChequeResponse? NeedCheque {
			get => needCheque;
			set => SetField(ref needCheque, value);
		}

		#endregion

		CounterpartyType counterpartyType;
		[Display(Name = "Тип контрагента")]
		public virtual CounterpartyType CounterpartyType {
			get => counterpartyType;
			set => SetField(ref counterpartyType, value);
		}

		IList<SuplierPriceItem> suplierPriceItems = new List<SuplierPriceItem>();
		[PropertyChangedAlso("PriceNodes")]
		[Display(Name = "Цены на ТМЦ")]
		public virtual IList<SuplierPriceItem> SuplierPriceItems {
			get => suplierPriceItems;
			set => SetField(ref suplierPriceItems, value);
		}

		GenericObservableList<SuplierPriceItem> observableSuplierPriceItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SuplierPriceItem> ObservableSuplierPriceItems {
			get {
				if(observableSuplierPriceItems == null)
					observableSuplierPriceItems = new GenericObservableList<SuplierPriceItem>(SuplierPriceItems);
				return observableSuplierPriceItems;
			}
		}

		#region Calculated Properties

		public virtual string RawJurAddress {
			get => JurAddress;
			set {
				StringBuilder sb = new StringBuilder(value);
				sb.Replace("\n", "");
				JurAddress = sb.ToString();
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
				CheckSpecialField(ref result, SpecialContractNumber);
				CheckSpecialField(ref result, SpecialKPP);
				CheckSpecialField(ref result, CargoReceiver);
				CheckSpecialField(ref result, SpecialCustomer);
				CheckSpecialField(ref result, GovContract);
				CheckSpecialField(ref result, SpecialDeliveryAddress);
				return result;
			}
		}

		IList<ISupplierPriceNode> priceNodes = new List<ISupplierPriceNode>();
		public virtual IList<ISupplierPriceNode> PriceNodes {
			get {
				var lst = new List<ISupplierPriceNode>();
				int cnt = 0;
				foreach(var nom in SuplierPriceItems.Select(i => i.NomenclatureToBuy).Distinct()) {
					var sNom = new SellingNomenclature {
						NomenclatureToBuy = nom,
						Parent = null,
						PosNr = (++cnt).ToString()
					};

					var children = SuplierPriceItems.Cast<ISupplierPriceNode>().Where(i => i.NomenclatureToBuy == nom).ToList();
					foreach(var i in children)
						i.Parent = sNom;
					sNom.Children = children;
					lst.Add(sNom);
				}
				return lst;
			}
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

		public virtual void AddCloseDeliveryComment(string comment, IUnitOfWork UoW)
		{
			var employee = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			CloseDeliveryComment = employee.ShortName + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + ": " + comment;
		}

		protected virtual bool CloseDelivery(IUnitOfWork UoW)
		{
			if(!UserPermissionRepository.CurrentUserPresetPermissions["can_close_deliveries_for_counterparty"])
				return false;
			IsDeliveriesClosed = true;
			CloseDeliveryDate = DateTime.Now;
			CloseDeliveryPerson = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			return true;
		}


		protected virtual bool OperDelivery(IUnitOfWork UoW)
		{
			if(!UserPermissionRepository.CurrentUserPresetPermissions["can_close_deliveries_for_counterparty"])
				return false;

			IsDeliveriesClosed = false;
			CloseDeliveryDate = null;
			CloseDeliveryPerson = null;
			CloseDeliveryComment = null;

			return true;
		}

		public virtual bool ToogleDeliveryOption(IUnitOfWork UoW)
		{
			return IsDeliveriesClosed ? OperDelivery(UoW) : CloseDelivery(UoW);
		}

		public virtual string GetCloseDeliveryInfo()
		{
			return CloseDeliveryPerson?.ShortName + " " + CloseDeliveryDate?.ToString("dd/MM/yyyy HH:mm");
		}

		#endregion CloseDelivery

		public Counterparty()
		{
			Name = string.Empty;
			FullName = string.Empty;
			Comment = string.Empty;
			INN = string.Empty;
			KPP = string.Empty;
			JurAddress = string.Empty;
			PhoneFrom1c = string.Empty;
		}

		#region IValidatableObject implementation

		private bool CheckForINNDuplicate()
		{
			IList<Counterparty> counterarties = CounterpartyRepository.GetCounterpartiesByINN(UoW, INN);
			if(counterarties == null)
				return false;
			if(counterarties.Any(x => x.Id != Id))
				return true;
			return false;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(CheckForINNDuplicate()) {
				yield return new ValidationResult("Контрагент с данным ИНН уже существует.",
												  new[] { this.GetPropertyName(o => o.INN) });
			}
			if(UseSpecialDocFields && SpecialKPP != null && SpecialKPP.Length != 9) {
				yield return new ValidationResult("Длина КПП для документов должна равнятся 9-ти.",
						new[] { this.GetPropertyName(o => o.KPP) });
			}
			if(PersonType == PersonType.legal) {
				if(KPP.Length != 9 && KPP.Length != 0 && TypeOfOwnership != "ИП")
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
				if(!Regex.IsMatch(KPP, "^[0-9]*$") && TypeOfOwnership != "ИП")
					yield return new ValidationResult("КПП может содержать только цифры.",
						new[] { this.GetPropertyName(o => o.KPP) });
				if(!Regex.IsMatch(INN, "^[0-9]*$"))
					yield return new ValidationResult("ИНН может содержать только цифры.",
						new[] { this.GetPropertyName(o => o.INN) });
			}

			if(IsDeliveriesClosed && String.IsNullOrWhiteSpace(CloseDeliveryComment))
				yield return new ValidationResult("Неоходимо заполнить комментарий по закрытию поставок",
						new[] { this.GetPropertyName(o => o.CloseDeliveryComment) });

			if(IsArchive) {
				var unclosedContracts = CounterpartyContracts.Where(c => !c.IsArchive)
					.Select(c => c.Id.ToString()).ToList();
				if(unclosedContracts.Count > 0)
					yield return new ValidationResult(
						string.Format("Вы не можете сдать контрагента в архив с открытыми договорами: {0}", string.Join(", ", unclosedContracts)),
						new[] { this.GetPropertyName(o => o.CounterpartyContracts) });

				var balance = Repository.Operations.MoneyRepository.GetCounterpartyDebt(UoW, this);
				if(balance != 0)
					yield return new ValidationResult(
						string.Format("Вы не можете сдать контрагента в архив так как у него имеется долг: {0}", CurrencyWorks.GetShortCurrencyString(balance)));

				var activeOrders = OrderRepository.GetCurrentOrders(UoW, this);
				if(activeOrders.Count > 0)
					yield return new ValidationResult(
						string.Format("Вы не можете сдать контрагента в архив с незакрытыми заказами: {0}", string.Join(", ", activeOrders.Select(o => o.Id.ToString()))),
						new[] { this.GetPropertyName(o => o.CounterpartyContracts) });

				var deposit = Repository.Operations.DepositRepository.GetDepositsAtCounterparty(UoW, this, null);
				if(balance != 0)
					yield return new ValidationResult(
						string.Format("Вы не можете сдать контрагента в архив так как у него есть невозвращенные залоги: {0}", CurrencyWorks.GetShortCurrencyString(deposit)));

				var bottles = Repository.Operations.BottlesRepository.GetBottlesAtCounterparty(UoW, this);
				if(balance != 0)
					yield return new ValidationResult(
						string.Format("Вы не можете сдать контрагента в архив так как он не вернул {0} бутылей", bottles));

			}

			if(Id == 0 && CameFrom == null) {
				yield return new ValidationResult("Для новых клиентов необходимо заполнить поле \"Откуда клиент\"");
			}

			if(CounterpartyRepository.IsCashPayment(PaymentMethod) && (!NeedCheque.HasValue || NeedCheque.Value == ChequeResponse.Unknown))
				yield return new ValidationResult(
					"Укажите, требуется ли печать чека для контрагента",
					new[] { this.GetPropertyName(o => o.NeedCheque) }
				);
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
		Supplier
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

	public enum ChequeResponse
	{
		[Display(Name = "Не знаю")]
		Unknown,
		[Display(Name = "Да")]
		Yes,
		[Display(Name = "Нет")]
		No
	}

	public class ChequeResponseStringType : NHibernate.Type.EnumStringType
	{
		public ChequeResponseStringType() : base(typeof(ChequeResponse)) { }
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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSBanks;
using QSProjectsLib;

namespace Vodovoz.Domain.Employees
{
	public abstract class Personnel : PropertyChangedBase, IValidatableObject, IPersonnel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public virtual IUnitOfWork UoW { set; get; }

		public virtual int Id { get; set; }

		private DateTime dateOfCreate;

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate {
			get { return dateOfCreate; }
			set { SetField(ref dateOfCreate, value, () => CreationDate); }
		}

		string name;

		[Display(Name = "Имя")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value?.Trim(), () => Name); }
		}

		string lastName;

		[Display(Name = "Фамилия")]
		public virtual string LastName {
			get { return lastName; }
			set { SetField(ref lastName, value?.Trim(), () => LastName); }
		}

		string patronymic;

		[Display(Name = "Отчество")]
		public virtual string Patronymic {
			get { return patronymic; }
			set { SetField(ref patronymic, value?.Trim(), () => Patronymic); }
		}

		bool isRussianCitizen=true;

		[Display(Name = "Российское гражданство")]
		public virtual bool IsRussianCitizen {
			get { return isRussianCitizen; }
			set { SetField(ref isRussianCitizen, value, () => IsRussianCitizen); }
		}

		Citizenship citizenship;

		[Display(Name = "Иностранное граждансво")]
		public virtual Citizenship Citizenship {
			get { return citizenship; }
			set { SetField(ref citizenship, value, () => Citizenship); }
		}

		IList<EmployeeDocument> documents = new List<EmployeeDocument>();

		[Display(Name = "Документы")]
		public virtual IList<EmployeeDocument> Documents {
			get { return documents; }
			set { SetField(ref documents, value, () => Documents); }
		}

		GenericObservableList<EmployeeDocument> observableDocuments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<EmployeeDocument> ObservableDocuments {
			get {
				if(observableDocuments == null) {
					observableDocuments = new GenericObservableList<EmployeeDocument>(Documents);
				}
				return observableDocuments;
			}
		}

		DateTime? birthdayDate;

		[Display(Name = "Дата рождения")]
		public virtual DateTime? BirthdayDate {
			get { return birthdayDate; }
			set { SetField(ref birthdayDate,value,() => BirthdayDate); }
		}

		[Display(Name = "Тип")]
		public abstract EmployeeType EmployeeType { get; set; }

		string drivingNumber;

		[Display(Name = "Водительское удостоверение")]
		public virtual string DrivingNumber {
			get { return drivingNumber; }
			set { SetField(ref drivingNumber, value, () => DrivingNumber); }
		}

		string addressRegistration;

		[Display(Name = "Адрес регистрации")]
		public virtual string AddressRegistration {
			get { return addressRegistration; }
			set { SetField(ref addressRegistration, value, () => AddressRegistration); }
		}

		string addressCurrent;

		[Display(Name = "Фактический адрес")]
		public virtual string AddressCurrent {
			get { return addressCurrent; }
			set { SetField(ref addressCurrent, value, () => AddressCurrent); }
		}

		string inn;

		[Display(Name = "ИНН")]
		public virtual string INN {
			get { return inn; }
			set { SetField(ref inn, value, () => INN); }
		}

		IList<QSContacts.Phone> phones = new List<QSContacts.Phone>();

		[Display(Name = "Телефоны")]
		public virtual IList<QSContacts.Phone> Phones {
			get { return phones; }
			set { SetField(ref phones, value, () => Phones); }
		}



		IList<EmployeeContract> contracts = new List<EmployeeContract>();

		[Display(Name = "Документы")]
		public virtual IList<EmployeeContract> Contracts {
			get { return contracts; }
			set { SetField(ref contracts, value, () => Contracts); }
		}

		Nationality nationality;

		[Display(Name = "Национальность")]
		public virtual Nationality Nationality {
			get { return nationality; }
			set { SetField(ref nationality, value, () => Nationality); }
		}

		byte[] photo;

		[Display(Name = "Фотография")]
		public virtual byte[] Photo {
			get { return photo; }
			set { SetField(ref photo, value, () => Photo); }
		}

		#region IAccountOwner implementation

		private IList<Account> accounts = new List<Account>();

		public virtual IList<Account> Accounts {
			get { return accounts; }
			set {
				SetField(ref accounts, value, () => Accounts);
			}
		}

		GenericObservableList<Account> observableAccounts;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Account> ObservableAccounts {
			get {
				if(observableAccounts == null) {
					observableAccounts = new GenericObservableList<Account>(Accounts);
				}
				return observableAccounts;
			}
		}

		[Display(Name = "Основной счет")]
		public virtual Account DefaultAccount {
			get {
				return ObservableAccounts.FirstOrDefault(x => x.IsDefault);
			}
			set {
				Account oldDefAccount = ObservableAccounts.FirstOrDefault(x => x.IsDefault);
				if(oldDefAccount != null && value != null && oldDefAccount.Id != value.Id) {
					oldDefAccount.IsDefault = false;
				}
				value.IsDefault = true;
			}
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrEmpty(LastName))
				yield return new ValidationResult("Фамилия должна быть заполнена", new[] { "LastName" });

			var employees = UoW.Session.QueryOver<Employee>()
				.Where(e => e.Name == this.Name && e.LastName == this.LastName && e.Patronymic == this.Patronymic)
				.WhereNot(e => e.Id == this.Id)
				.List();

			if(employees.Count > 0)
				yield return new ValidationResult("Сотрудник уже существует", new[] { "Duplication" });
		}

		#endregion

		#region Свойства без маппинга

		[Display(Name = "ФИО")]
		public virtual string FullName {
			get { return String.Format("{0} {1} {2}", LastName, Name, Patronymic); }
		}

		[Display(Name = "Фамилия и инициалы")]
		public virtual string ShortName {
			get { return StringWorks.PersonNameWithInitials(LastName, Name, Patronymic); }
		}

		public virtual string Title {
			get { return ShortName; }
		}

		#endregion

		#region Methods

		public virtual void AddAccount(Account account)
		{
			ObservableAccounts.Add(account);
			account.Owner = this;
			if(DefaultAccount == null)
				account.IsDefault = true;
		}

		#endregion

		#region static methods

		public static void ChangeTraineeToEmployee(IUnitOfWorkGeneric<Employee> uow, ITrainee t)
		{
			Employee employee = uow.Root;
			ITrainee trainee = uow.GetById<Trainee>(t.Id);
			PropertyInfo[] properties = typeof(IPersonnel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach(var prop in properties) {
				if(prop.PropertyType != typeof(string) && !prop.PropertyType.IsValueType || !prop.CanWrite) {
					continue;
				}
				prop.SetValue(employee, prop.GetValue(trainee));
			}

			employee.Id = trainee.Id;

			foreach(var phone in trainee.Phones) {
				employee.Phones.Add(phone);
			}
			foreach(var account in trainee.Accounts) {
				employee.Accounts.Add(account);
			}
			employee.Nationality = trainee.Nationality;
			employee.Photo = trainee.Photo;
			uow.Session.Evict(trainee);
		}

		#endregion
	}


	public interface IPersonnel : IDomainObject, IBusinessObject, IAccountOwner
	{
		DateTime CreationDate { get; set; }
		string Name { get; set; }
		string LastName { get; set; }
		string Patronymic { get; set; }
		string DrivingNumber { get; set; }
		string AddressRegistration { get; set; }
		string AddressCurrent { get; set; }
		string INN { get; set; }
		IList<QSContacts.Phone> Phones { get; set; }
		Nationality Nationality { get; set; }
		byte[] Photo { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Domain.Employees
{
	public abstract class Personnel : PropertyChangedBase, IValidatableObject, IPersonnel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private IList<Attachment> _attachments = new List<Attachment>();
		private GenericObservableList<Attachment> _observableAttachments;
		
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
		public abstract EmployeeType EmployeeType { get; }

		string drivingLicense;

		[Display(Name = "Водительское удостоверение")]
		public virtual string DrivingLicense {
			get { return drivingLicense; }
			set { SetField(ref drivingLicense, value, () => DrivingLicense); }
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

		IList<Phone> phones = new List<Phone>();

		[Display(Name = "Телефоны")]
		public virtual IList<Phone> Phones {
			get { return phones; }
			set { SetField(ref phones, value, () => Phones); }
		}

		EmployeePost post;

		[Display(Name = "Должность")]
		public virtual EmployeePost Post
		{
			get => post;
			set => SetField(ref post, value);
		}

		int? skilllevel;
		[Display(Name = "Уровень квалификации")]
		public virtual int? SkillLevel
		{
			get => skilllevel;
			set => SetField(ref skilllevel, value);
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

		public virtual List<EmployeeDocument> GetMainDocuments()
		{
			List<EmployeeDocument> mainDocuments = new List<EmployeeDocument>(); 
			foreach(var doc in Documents) {
				if(doc.MainDocument == true)
					mainDocuments.Add(doc);
			}
			return mainDocuments;
		}
		
		[Display(Name = "Прикрепленные файлы")]
		public virtual IList<Attachment> Attachments
		{
			get => _attachments;
			set => SetField(ref _attachments, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Attachment> ObservableAttachments => 
			_observableAttachments ?? (_observableAttachments = new GenericObservableList<Attachment>(Attachments));

		public virtual List<int> GetSkillLevels() => new List<int> { 0, 1, 2, 3, 4, 5 };

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrEmpty(LastName))
				yield return new ValidationResult("Фамилия должна быть заполнена", new[] { "LastName" });

			var personnels = UoW.Session.QueryOver<Personnel>()
				.Where(p => p.Name == this.Name && p.LastName == this.LastName && p.Patronymic == this.Patronymic)
				.WhereNot(p => p.Id == this.Id)
				.List();

			if(personnels.Count > 0)
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
			get { return PersonHelper.PersonNameWithInitials(LastName, Name, Patronymic); }
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

		public static void ChangeTraineeToEmployee(Employee employee, ITrainee trainee)
		{
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
			foreach(var document in trainee.Documents) {
				employee.Documents.Add(document);
			}
			employee.isRussianCitizen = trainee.IsRussianCitizen;
			employee.Citizenship = trainee.Citizenship;
			employee.Nationality = trainee.Nationality;
			employee.Photo = trainee.Photo;
		}

		#endregion
	}


	public interface IPersonnel : IDomainObject, IBusinessObject, IAccountOwner
	{
		DateTime CreationDate { get; set; }
		string Name { get; set; }
		string LastName { get; set; }
		string Patronymic { get; set; }
		string DrivingLicense { get; set; }
		string AddressRegistration { get; set; }
		string AddressCurrent { get; set; }
		string INN { get; set; }
		IList<Phone> Phones { get; set; }
		EmployeePost Post { get; set; }
		int? SkillLevel { get; set; }
		IList<EmployeeDocument> Documents { get; set; }
		Nationality Nationality { get; set; }
		bool IsRussianCitizen { get; set; }
		Citizenship Citizenship { get; set; }
		byte[] Photo { get; set; }
	}
}

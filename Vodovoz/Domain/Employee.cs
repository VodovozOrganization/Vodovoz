using System;
using QSOrmProject;
using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using QSProjectsLib;
using QSBanks;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "сотрудники",
		Nominative = "сотрудник")]
	public class Employee : AccountOwnerBase, IDomainObject, IValidatableObject, ISpecialRowsRender
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Имя")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string lastName;

		[Display (Name = "Фамилия")]
		public virtual string LastName {
			get { return lastName; }
			set { SetField (ref lastName, value, () => LastName); }
		}

		string patronymic;

		[Display (Name = "Отчество")]
		public virtual string Patronymic {
			get { return patronymic; }
			set { SetField (ref patronymic, value, () => Patronymic); }
		}

		EmployeeCategory category;

		[Display (Name = "Категория")]
		public virtual EmployeeCategory Category {
			get { return category; }
			set { SetField (ref category, value, () => Category); }
		}

		string passportSeria;

		[Display (Name = "Серия паспорта")]
		public virtual string PassportSeria {
			get { return passportSeria; }
			set { SetField (ref passportSeria, value, () => PassportSeria); }
		}

		string passportNumber;

		[Display (Name = "Номер паспорта")]
		public virtual string PassportNumber {
			get { return passportNumber; }
			set { SetField (ref passportNumber, value, () => PassportNumber); }
		}

		string drivingNumber;

		[Display (Name = "Водительское удостоверение")]
		public virtual string DrivingNumber {
			get { return drivingNumber; }
			set { SetField (ref drivingNumber, value, () => DrivingNumber); }
		}

		string addressRegistration;

		[Display (Name = "Адрес регистрации")]
		public virtual string AddressRegistration {
			get { return addressRegistration; }
			set { SetField (ref addressRegistration, value, () => AddressRegistration); }
		}

		string addressCurrent;

		[Display (Name = "Фактический адрес")]
		public virtual string AddressCurrent {
			get { return addressCurrent; }
			set { SetField (ref addressCurrent, value, () => AddressCurrent); }
		}

		string inn;

		[Display (Name = "ИНН")]
		public virtual string INN {
			get { return inn; }
			set { SetField (ref inn, value, () => INN); }
		}

		bool isFired;

		[Display (Name = "Сотрудник уволен")]
		public virtual bool IsFired {
			get { return isFired; }
			set { SetField (ref isFired, value, () => IsFired); }
		}

		IList<QSContacts.Phone> phones;

		[Display (Name = "Телефоны")]
		public virtual IList<QSContacts.Phone> Phones {
			get { return phones; }
			set { SetField (ref phones, value, () => Phones); }
		}

		Nationality nationality;

		[Display (Name = "Национальность")]
		public virtual Nationality Nationality {
			get { return nationality; }
			set { SetField (ref nationality, value, () => Nationality); }
		}

		User user;

		[Display (Name = "Пользователь")]
		public virtual User User {
			get { return user; }
			set { SetField (ref user, value, () => User); }
		}

		byte[] photo;

		[Display (Name = "Фотография")]
		public virtual byte[] Photo {
			get { return photo; }
			set { SetField (ref photo, value, () => Photo); }
		}

		#endregion

		public Employee ()
		{
			Name = String.Empty;
			LastName = String.Empty;
			Patronymic = String.Empty;
			PassportSeria = String.Empty;
			PassportNumber = String.Empty;
			DrivingNumber = String.Empty;
			Category = EmployeeCategory.office;
			AddressRegistration = String.Empty;
			AddressCurrent = String.Empty;
		}

		public string FullName {
			get { return String.Format ("{0} {1} {2}", LastName, Name, Patronymic); }
		}

		public string ShortName {
			get { return StringWorks.PersonNameWithInitials (LastName, Name, Patronymic); }
		}

		public string Title {
			get { return ShortName;}
		}

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (String.IsNullOrEmpty (Name) && String.IsNullOrEmpty (LastName) && String.IsNullOrEmpty (Patronymic))
				yield return new ValidationResult ("Должно быть заполнено хотя бы одно из следующих полей: " +
				"Фамилия, Имя, Отчество)", new[] { "Name", "LastName", "Patronymic" });
		}

		#endregion

		#region ISpecialRowsRender implementation

		public string TextColor { get { return IsFired ? "grey" : "black"; } }

		#endregion
	}

	public enum EmployeeCategory
	{
		[ItemTitleAttribute ("Офисный работник")]
		office,
		[ItemTitleAttribute ("Водитель")]
		driver,
		[ItemTitleAttribute ("Экспедитор")]
		forwarder
	}

	public class EmployeeCategoryStringType : NHibernate.Type.EnumStringType
	{
		public EmployeeCategoryStringType () : base (typeof(EmployeeCategory))
		{
		}
	}
}


using System;
using QSOrmProject;
using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubject ("Сотрудники")]
	public class Employee : PropertyChangedBase, IDomainObject, IValidatableObject, ISpecialRowsRender
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string lastName;

		public virtual string LastName {
			get { return lastName; }
			set { SetField (ref lastName, value, () => LastName); }
		}

		string patronymic;

		public virtual string Patronymic {
			get { return patronymic; }
			set { SetField (ref patronymic, value, () => Patronymic); }
		}

		EmployeeCategory category;

		public virtual EmployeeCategory Category {
			get { return category; }
			set { SetField (ref category, value, () => Category); }
		}

		string passportSeria;

		public virtual string PassportSeria {
			get { return passportSeria; }
			set { SetField (ref passportSeria, value, () => PassportSeria); }
		}

		string passportNumber;

		public virtual string PassportNumber {
			get { return passportNumber; }
			set { SetField (ref passportNumber, value, () => PassportNumber); }
		}

		string drivingNumber;

		public virtual string DrivingNumber {
			get { return drivingNumber; }
			set { SetField (ref drivingNumber, value, () => DrivingNumber); }
		}

		string addressRegistration;

		public virtual string AddressRegistration {
			get { return addressRegistration; }
			set { SetField (ref addressRegistration, value, () => AddressRegistration); }
		}

		string addressCurrent;

		public virtual string AddressCurrent {
			get { return addressCurrent; }
			set { SetField (ref addressCurrent, value, () => AddressCurrent); }
		}

		bool isFired;

		public virtual bool IsFired {
			get { return isFired; }
			set { SetField (ref isFired, value, () => IsFired); }
		}

		IList<QSContacts.Phone> phones;

		public virtual IList<QSContacts.Phone> Phones {
			get { return phones; }
			set { SetField (ref phones, value, () => Phones); }
		}

		Nationality nationality;

		public virtual Nationality Nationality {
			get { return nationality; }
			set { SetField (ref nationality, value, () => Nationality); }
		}

		User user;

		public virtual User User {
			get { return user; }
			set { SetField (ref user, value, () => User); }
		}

		byte[] photo;

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
		driver
	}

	public class EmployeeCategoryStringType : NHibernate.Type.EnumStringType
	{
		public EmployeeCategoryStringType () : base (typeof(EmployeeCategory))
		{
		}
	}
}


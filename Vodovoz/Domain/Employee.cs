using System;
using QSOrmProject;
using NHibernate;
using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubject ("Сотрудники")]
	public class Employee : PropertyChangedBase, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		public virtual string Name { get; set; }

		public virtual string LastName { get; set; }

		public virtual string Patronymic { get; set; }

		public virtual EmployeeCategory Category { get; set; }

		public virtual string PassportSeria { get; set; }

		public virtual string PassportNumber { get; set; }

		public virtual string DrivingNumber { get; set; }

		public virtual string AddressRegistration { get; set; }

		public virtual string AddressCurrent { get; set; }

		public virtual bool IsFired { get; set; }

		public virtual IList<QSContacts.Phone> Phones { get; set; }

		public virtual Nationality Nationality { get; set; }

		public virtual User User { get; set; }

		byte[] photo;

		public virtual byte[] Photo {
			get {
				return photo;
			}
			set {
				SetField (ref photo, value, () => Photo);
			}
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
			get {
				return String.Format ("{0} {1} {2}", LastName, Name, Patronymic);
			}
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (String.IsNullOrEmpty (Name) && String.IsNullOrEmpty (LastName) && String.IsNullOrEmpty (Patronymic))
				yield return new ValidationResult ("Должно быть заполнено хотя бы одно из следующих полей: " +
				"Фамилия, Имя, Отчество)", new[] { "Name", "LastName", "Patronymic" });
		}

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


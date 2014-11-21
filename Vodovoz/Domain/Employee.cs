using System;
using QSOrmProject;
using NHibernate;
using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubjectAttributes("Сотрудники")]
	[Magic]
	public class Employee : PropertyChangedBase
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		[Required(ErrorMessage = "Необходимо заполнить хотя бы фамилию сотрудника.")]
		public virtual string LastName { get; set; }
		public virtual string Patronymic { get; set; }
		public virtual EmployeeCategory Category { get; set; }
		public virtual string PassportSeria { get; set; }
		public virtual string PassportNumber { get; set; }
		public virtual string DrivingNumber { get; set; }
		public virtual Nationality Nationality { get; set; }
		public virtual User User { get; set; }
		byte[] photo;
		[Magic]
		public virtual byte[] Photo
		{
			get
			{
				return photo;
			}
			set
			{
				photo = value;
				RaisePropertyChanged("Photo");
			}
		}
		#endregion

		public Employee()
		{
			Name = String.Empty;
			LastName = String.Empty;
			Patronymic = String.Empty;
			PassportSeria = String.Empty;
			PassportNumber = String.Empty;
			DrivingNumber = String.Empty;
			Category = EmployeeCategory.office;
		}

		public string FullName {
			get {
				return String.Format ("{0} {1} {2}", LastName, Name, Patronymic);
			}
		}
	}

	public enum EmployeeCategory{
		[ItemTitleAttribute("Офисный работник")]
		office,
		[ItemTitleAttribute("Водитель")]
		driver
	}

	public class EmployeeCategoryStringType : NHibernate.Type.EnumStringType 
	{
		public EmployeeCategoryStringType() : base(typeof(EmployeeCategory))
		{}
	}
}


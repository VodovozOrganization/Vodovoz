using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject ("Оборудование")]
	public class Equipment: IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		public virtual bool OnDuty { get; set; }

		public virtual string Serial { get; set; }

		public virtual string Comment { get; set; }

		public virtual Nomenclature Nomenclature { get; set; }

		public virtual DateTime LastServiceDate { get; set; }

		public virtual DateTime WarrantyEndDate { get; set; }

		#endregion

		public virtual string Type { get { return Nomenclature == null ? String.Empty : Nomenclature.Type.Name; } }

		public virtual string NomenclatureName { get { return Nomenclature == null ? String.Empty : Nomenclature.Name; } }


		public virtual string LastServiceDateString { get { return LastServiceDate.ToShortDateString (); } }

		public Equipment ()
		{
			Serial = Comment = String.Empty;
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (LastServiceDate > DateTime.Now)
				yield return new ValidationResult ("Дата последней санитарной обработки не может быть в будущем.");
			if (Serial == String.Empty)
				yield return new ValidationResult ("Серийный номер должен быть заполнен.");
			if (Nomenclature == null)
				yield return new ValidationResult ("Должна быть указана номенклатура.");
		}

		#endregion
	}
}


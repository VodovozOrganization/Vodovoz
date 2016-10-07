using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Vodovoz.Domain.Logistic
{

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "талоны на топливо",
		Nominative = "талон на топливо")]
	public class GazTicket : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название должно быть заполнено.")]
		[StringLength(45)]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}
	
		FuelType fuelType;

		[Display (Name = "Вид топлива")]
		[Required (ErrorMessage = "Необходим вид топлива.")]
		public virtual FuelType FuelType {
			get { return fuelType; }
			set { SetField (ref fuelType, value, () => FuelType); }
		}

		int liters;

		[Display (Name = "Литры")]
		public virtual int Liters {
			get { return liters; }
			set { SetField (ref liters, value, () => Liters); }
		}

		public GazTicket()
		{
		}

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Liters == 0)
				yield return new ValidationResult("Количество литров не может быть 0",
					new[] {Gamma.Utilities.PropertyUtil.GetPropertyName(this, o=>o.Liters)});
		}
	}
}


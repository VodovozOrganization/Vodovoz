using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Gamma.Utilities;

namespace Vodovoz.Domain.Logistic
{

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "смены доставки",
		Nominative = "смена доставки")]
	public class DeliveryShift : PropertyChangedBase, IDomainObject, IValidatableObject
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

		TimeSpan startTime;

		[Display(Name = "От часа")]
		public virtual TimeSpan StartTime {
			get { return startTime; }
			set { SetField(ref startTime, value, () => StartTime); }
		}

		TimeSpan endTime;

		[Display(Name = "До часа")]
		public virtual TimeSpan EndTime {
			get { return endTime; }
			set { SetField(ref endTime, value, () => EndTime); }
		}

		public virtual string DeliveryTime { get { return String.Format("с {0:hh\\:mm} до {1:hh\\:mm}", startTime, endTime); } }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartTime > EndTime)
				yield return new ValidationResult("Окончание работы не может быть раньше его начала.", new[] {
					this.GetPropertyName (o => o.StartTime),
					this.GetPropertyName (o => o.EndTime)
				});

			if(StartTime.TotalMinutes < 1)
				yield return new ValidationResult("Время начало работы не может быть 0.", new[] {
					this.GetPropertyName (o => o.StartTime)
				});

			if(EndTime.TotalMinutes < 1)
				yield return new ValidationResult("Время окончания работы не может быть 0.", new[] {
					this.GetPropertyName (o => o.EndTime)
				});

			if(StartTime.TotalDays > 1)
				yield return new ValidationResult("Время начало работы не может быть больше 24 часов.", new[] {
					this.GetPropertyName (o => o.StartTime)
				});

			if(EndTime.TotalDays > 1)
				yield return new ValidationResult("Время окончания работы не может быть больше 24 часов.", new[] {
					this.GetPropertyName (o => o.EndTime)
				});

		}

		public DeliveryShift ()
		{
		}
	}
}


using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Gamma.Utilities;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "графики доставки",
		Nominative = "график доставки")]
	public class DeliverySchedule: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Не заполнено название.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		TimeSpan from;

		[Display (Name = "От часа")]
		public virtual TimeSpan From {
			get { return from; } 
			set { SetField (ref from, value, () => From); }
		}

		TimeSpan to;

		[Display (Name = "До часа")]
		public virtual TimeSpan To {
			get { return to; }
			set { SetField (ref to, value, () => To); }
		}

		public virtual string DeliveryTime { get { return String.Format ("с {0:hh\\:mm} до {1:hh\\:mm}", from, to); } }

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(!QSProjectsLib.QSMain.User.Permissions["can_edit_delivery_schedule"] && Id > 0)
				yield return new ValidationResult("Вы не можете изменять график доставки");

			if (From > To)
				yield return new ValidationResult ("Окончание периода доставки не может быть раньше его начала.", new[] {
					this.GetPropertyName (o => o.From),
					this.GetPropertyName (o => o.To)
				});

			if(From.TotalMinutes < 1)
				yield return new ValidationResult ("Время начало периода не может быть 0.", new[] {
					this.GetPropertyName (o => o.From)
				});

			if(To.TotalMinutes < 1)
				yield return new ValidationResult ("Время окончания периода не может быть 0.", new[] {
					this.GetPropertyName (o => o.To)
				});

			if(From.TotalDays > 1)
				yield return new ValidationResult ("Время начало периода не может быть больше 24 часов.", new[] {
					this.GetPropertyName (o => o.From)
				});

			if(To.TotalDays > 1)
				yield return new ValidationResult ("Время окончания периода не может быть больше 24 часов.", new[] {
					this.GetPropertyName (o => o.To)
				});
		}

		#endregion

		public DeliverySchedule ()
		{
			Name = String.Empty;
		}

	}
}


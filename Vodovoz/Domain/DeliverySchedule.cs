using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubject (JournalName = "Графики доставки", ObjectName = "график доставки")]
	public class DeliverySchedule: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string name;
		[Required (ErrorMessage = "Не заполнено название.")]
		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		TimeSpan from;

		[Display(Name = "От часа")]
		public virtual TimeSpan From {
			get { return from; } 
			set { SetField (ref from, value, () => From); }
		}

		TimeSpan to;

		[Display(Name = "До часа")]
		public virtual TimeSpan To {
			get { return to; }
			set { SetField (ref to, value, () => To); }
		}

		public virtual string DeliveryTime { get { return String.Format ("с {0:hh\\:mm} до {1:hh\\:mm}", from, to); } }

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (From > To)
				yield return new ValidationResult ("Окончание периода доставки не может быть раньше его начала.", new[] { "From", "To" });
		}

		#endregion

		public DeliverySchedule ()
		{
			Name = String.Empty;
		}

	}
}


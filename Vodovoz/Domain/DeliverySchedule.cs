using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubject ("Граффики доставки")]
	public class DeliverySchedule: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string name;

		public virtual string Name {
			get {
				return name;
			}
			set {
				SetField (ref name, value, () => Name);
			}
		}

		DateTime from;

		public virtual DateTime From {
			get {
				return from;
			}
			set {
				SetField (ref from, value, () => From);
			}
		}

		DateTime to;

		public virtual DateTime To {
			get {
				return to;
			}
			set {
				SetField (ref to, value, () => To);
			}
		}

		public virtual string DeliveryTime { get { return String.Format ("с {0} до {1}", from.ToShortTimeString (), to.ToShortTimeString ()); } }

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (String.IsNullOrEmpty (Name))
				yield return new ValidationResult ("Не заполнено название.", new[] { "Name" });
			if (From.Hour > To.Hour || (From.Hour == To.Hour && From.Minute > To.Minute))
				yield return new ValidationResult ("Окончание периода доставки не может быть раньше его начала.", new[] { "From", "To" });
			
			
		}

		#endregion

		public DeliverySchedule ()
		{
			Name = String.Empty;
		}

	}
}


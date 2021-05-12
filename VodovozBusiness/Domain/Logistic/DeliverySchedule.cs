using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Project.Services;

namespace Vodovoz.Domain.Logistic
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "графики доставки",
		Nominative = "график доставки")]
	[EntityPermission]
	public class DeliverySchedule: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public DeliverySchedule()
		{
			Name = String.Empty;
		}
		
		public virtual int Id { get; set; }

		private string name;
		[Required (ErrorMessage = "Не заполнено название.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField (ref name, value, () => Name);
		}

		private TimeSpan from;
		[Display (Name = "От часа")]
		public virtual TimeSpan From {
			get => from;
			set => SetField (ref @from, value, () => From);
		}

		private TimeSpan to;
		[Display (Name = "До часа")]
		public virtual TimeSpan To {
			get => to;
			set => SetField (ref to, value, () => To);
		}

		public virtual string DeliveryTime => $"с {from:hh\\:mm} до {to:hh\\:mm}";

		private bool isArchive;
		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value);
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_delivery_schedule") && Id > 0)
				yield return new ValidationResult("Вы не можете изменять график доставки");

			if(From > To)
				yield return new ValidationResult("Окончание периода доставки не может быть раньше его начала.",
					new[] { nameof(From), nameof(To) });

			if(From.TotalMinutes < 1)
				yield return new ValidationResult("Время начало периода не может быть 0.",
					new[] { nameof(From) });

			if(To.TotalMinutes < 1)
				yield return new ValidationResult("Время окончания периода не может быть 0.",
					new[] { nameof(To) });

			if(From.TotalDays > 1)
				yield return new ValidationResult("Время начало периода не может быть больше 24 часов.",
					new[] { nameof(From) });

			if(To.TotalDays > 1)
				yield return new ValidationResult("Время окончания периода не может быть больше 24 часов.",
					new[] { nameof(To) });
		}

		#endregion
		
	}
}


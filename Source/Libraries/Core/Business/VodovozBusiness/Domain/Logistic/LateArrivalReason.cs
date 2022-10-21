using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "причины опозданий водителей",
		Nominative = "причина опозданий водителей",
		Prepositional = "причине опозданий водителей",
		PrepositionalPlural = "причинах опозданий водителей"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class LateArrivalReason : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Cвойства

		public virtual int Id { get; set; }

		public virtual string Title => $"Причина опоздания водителя №{Id} - {Name}";

		string name;
		[Display(Name = "Причина опоздания")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}
		
		DateTime createDate;
		[Display(Name = "Дата создания")]
		[IgnoreHistoryTrace]
		public virtual DateTime CreateDate {
			get => createDate;
			set => SetField(ref createDate, value);
		}
		
		bool isArchive;
		[Display(Name = "В архиве?")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value);
		}

		#endregion

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name)) {
				yield return new ValidationResult(
					"Причина должна быть заполнена.",
					new[] { nameof(Name) }
				);
			}
		}
	}
}

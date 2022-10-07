using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "источники рекламаций",
		Nominative = "источник рекламации"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintSource : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult(
					"Необходимо заполнить название",
					new[] { this.GetPropertyName(o => o.Name) }
				);
		}
	}
}

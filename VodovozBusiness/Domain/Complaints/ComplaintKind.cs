using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "вид рекламации",
		Nominative = "вид рекламации",
		Prepositional = "виде рекламации",
		PrepositionalPlural = "видах рекламаций"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintKind : BusinessObjectBase<Complaint>, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название вида")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		bool isArchive;
		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value);
		}

		public virtual string GetFullName => !IsArchive ? Name : string.Format("(Архив) {0}", Name);

		public virtual string Title => string.Format("Вид рекламации №{0} ({1})", Id, Name);

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult(
					"Укажите название вида рекламации",
					new[] { this.GetPropertyName(o => o.Name) }
				);
		}
	}
}
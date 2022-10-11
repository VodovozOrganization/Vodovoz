using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
			NominativePlural = "источники проблем недовезенного заказа",
			Nominative = "источник проблемы недовезенного заказа",
			Prepositional = "источнике проблемы недовезенного заказа",
			PrepositionalPlural = "источниках проблем недовезенного заказа"
		   )
	]
	[HistoryTrace]
	[EntityPermission]
	public class UndeliveryProblemSource : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название источника проблемы")]
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

		public virtual string Title => string.Format("Источник проблемы №{0} ({1})", Id, Name);

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult( "Укажите название источника проблемы",
					new[] { this.GetPropertyName(o => o.Name) }
				);
		}
	}
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.WageCalculation
{
	[
		Appellative(
			Gender = GrammaticalGender.Feminine,
			NominativePlural = "планы продаж",
			Nominative = "план продаж"
		)
	]
	[HistoryTrace]
	[EntityPermission]
	public class SalesPlan : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название")]
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

		int fullBottlesToSell;
		[Display(Name = "Кол-во полных бутылей для продажи")]
		public virtual int FullBottleToSell {
			get => fullBottlesToSell;
			set => SetField(ref fullBottlesToSell, value);
		}

		int emptyBottlesToTake;
		[Display(Name = "Кол-во пустых бутылей для забора")]
		public virtual int EmptyBottlesToTake {
			get => emptyBottlesToTake;
			set => SetField(ref emptyBottlesToTake, value);
		}

		public virtual string Title {
			get {
				return string.Format(
					"продажа - {1} бут., забор - {2} бут. (№{0})",
					Id,
					FullBottleToSell,
					EmptyBottlesToTake
				);
			}
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(FullBottleToSell <= 0)
				yield return new ValidationResult(
					"Должно быть указано планируемое количество бутылей для продажи",
					new[] { this.GetPropertyName(o => o.FullBottleToSell) }
				);

			if(EmptyBottlesToTake <= 0)
				yield return new ValidationResult(
					"Должно быть указано планируемое количество бутылей для забора",
					new[] { this.GetPropertyName(o => o.EmptyBottlesToTake) }
				);
		}

		#endregion IValidatableObject implementation
	}
}

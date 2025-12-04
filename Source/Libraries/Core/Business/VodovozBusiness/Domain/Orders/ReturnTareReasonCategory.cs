using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "категории причин забора тары",
        Nominative = "категория причины забора тары")]
	[HistoryTrace]
	[EntityPermission]
	public class ReturnTareReasonCategory : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        public virtual int Id { get; set; }

		public virtual string Title => $"Категория забора тары № {Id} - {Name}";
        
        string name;
        [Display(Name = "Название категории")]
        public virtual string Name {
            get => name;
            set => SetField(ref name, value);
        }
        
        IList<ReturnTareReason> childReasons = new List<ReturnTareReason>();
        [Display(Name = "Причины забора тары")]
        public virtual IList<ReturnTareReason> ChildReasons {
            get => childReasons;
            set => SetField(ref childReasons, value);
        }

        GenericObservableList<ReturnTareReason> observableChildReasons;
        //FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<ReturnTareReason> ObservableChildReasons {
            get {
                if(observableChildReasons == null)
                    observableChildReasons = new GenericObservableList<ReturnTareReason>(ChildReasons);
                return observableChildReasons;
            }
        }

		public virtual void AddChildReason(ReturnTareReason reason)
		{
			ObservableChildReasons.Add(reason);
		}

		public virtual void RemoveChildReason(ReturnTareReason reason)
		{
			ObservableChildReasons.Remove(reason);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name)) {
				yield return new ValidationResult(
					"Категория должна быть заполнена.",
					new[] { nameof(Name) }
				);
			}
		}
	}
}

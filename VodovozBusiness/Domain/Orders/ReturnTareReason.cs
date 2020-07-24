using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "причины забора тары",
        Nominative = "причина забора тары",
        Prepositional = "причине забора тары",
        PrepositionalPlural = "причинах забора тары"
    )]
    [HistoryTrace]
    [EntityPermission]
    public class ReturnTareReason : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        #region Cвойства

        public virtual int Id { get; set; }

		public virtual string Title => $"Причина №{Id} - {Name}";

        string name;
        [Display(Name = "Причина забора тары")]
        public virtual string Name {
            get => name;
            set => SetField(ref name, value);
        }
        
        bool isArchive;
        [Display(Name = "В архиве?")]
        public virtual bool IsArchive {
            get => isArchive;
            set => SetField(ref isArchive, value);
        }

		IList<ReturnTareReasonCategory> reasonCategories = new List<ReturnTareReasonCategory>();
		[Display(Name = "Категории причины забора тары")]
		public virtual IList<ReturnTareReasonCategory> ReasonCategories {
			get => reasonCategories;
			set => SetField(ref reasonCategories, value);
		}

		GenericObservableList<ReturnTareReasonCategory> observableReasonCategories;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ReturnTareReasonCategory> ObservableReasonCategories {
			get {
				if(observableReasonCategories == null)
					observableReasonCategories = new GenericObservableList<ReturnTareReasonCategory>(ReasonCategories);
				return observableReasonCategories;
			}
		}

		#endregion

		public virtual void AddCategory(ReturnTareReasonCategory category)
		{
			ObservableReasonCategories.Add(category);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Name))
            {
                yield return new ValidationResult(
                    "Причина должна быть заполнена.",
                    new [] {nameof(Name)}
                );
            }
        }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
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

        public virtual string Title => $"Причина забора тары №{Id} {Name}, категории {ReasonCategory.GetEnumTitle()}";

        string name;
        [Display(Name = "Причина забора тары")]
        public virtual string Name {
            get => name;
            set => SetField(ref name, value);
        }
        
        ReturnTareReasonCategory reasonCategory;
        [Display(Name = "Категория причины забора тары")]
        public virtual ReturnTareReasonCategory ReasonCategory {
            get => reasonCategory;
            set => SetField(ref reasonCategory, value);
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
            if (string.IsNullOrEmpty(Name))
            {
                yield return new ValidationResult(
                    "Причина должна быть заполнена.",
                    new [] {nameof(Name)}
                );
            }
        }
    }

    public enum ReturnTareReasonCategory
    {
        [Display(Name = "Приостановление")]
        Suspension,
        [Display(Name = "Расторжение")]
        Termination,
        [Display(Name = "Дозабор")]
        TakeAway
    }
}
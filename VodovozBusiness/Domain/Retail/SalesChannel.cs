using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Retail
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "каналы сбыта",
        Nominative = "канал сбыта"
    )]
    [HistoryTrace]
    [EntityPermission]
    public class SalesChannel : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Свойства

        public virtual int Id { get; set; }

        string Title => Name;

        string name;

        [Display(Name = "Название канала сбыта")]
        public virtual string Name
        {
            get => name;
            set => SetField(ref name, value);
        }

        #endregion

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
                yield return new ValidationResult("Название канала сбыта не может быть пустым",
                    new[] { nameof(Name) });
        }
    }
}

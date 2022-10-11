using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Employees
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "Должности",
        Nominative = "Должность")]
    [EntityPermission]
    [HistoryTrace]
    public class EmployeePost : PropertyChangedBase, IValidatableObject, IDomainObject
    {
        public virtual int Id { get; set; }

        string name;

        [Display(Name = "Название")]
        public virtual string Name
        {
            get => name;
            set => SetField(ref name, value);
        }

        public EmployeePost() => Name = String.Empty;

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (String.IsNullOrWhiteSpace(Name))
                yield return new ValidationResult("Название должно быть заполнено", new[] { nameof(Name) });
        }
    }
}
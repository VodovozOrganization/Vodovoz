using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        Nominative = "Вид наёмного автомобиля",
        NominativePlural = "Виды наёмных автомобилей"
    )]
    public class DriverCarKind : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        public virtual int Id { get; set; }

        private string name;
        [Display(Name = "Название")]
        public virtual string Name {
            get => name;
            set => SetField(ref name, value);
        }

        private string shortName;
        [Display(Name = "Сокращённое название")]
        public virtual string ShortName {
            get => shortName;
            set => SetField(ref shortName, value);
        }

        private bool isArchive;
        [Display(Name = "Архивный")]
        public virtual bool IsArchive {
            get => isArchive;
            set => SetField(ref isArchive, value);
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(String.IsNullOrWhiteSpace(Name)) {
                yield return new ValidationResult(
                    "Укажите название вида наёмного авто",
                    new[] { nameof(Name) });
            }
            if(Name?.Length > 100) {
                yield return new ValidationResult(
                    "Длина названия вида наёмного авто не должна превышать 100 символов",
                    new[] { nameof(Name) });
            }
            if(ShortName?.Length > 10) {
                yield return new ValidationResult(
                    "Длина сокращённого названия вида наёмного авто не должна превышать 10 символов",
                    new[] { nameof(Name) });
            }
        }
    }
}

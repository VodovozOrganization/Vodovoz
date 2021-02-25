using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Client
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "типы ответственных лиц",
        Nominative = "тип ответственного лица"
    )]
    [HistoryTrace]
    [EntityPermission]
    public class DeliveryPointResponsiblePersonType : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Свойства

        public virtual int Id { get; set; }

        string title;

        [Display(Name = "Название типа")]
        public virtual string Title
        {
            get => title;
            set => SetField(ref title, value);
        }

        #endregion

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Title))
                yield return new ValidationResult("Название типа ответственного за точку доставки лица не может быть пустым",
                    new[] { nameof(Title) });
        }
    }
}

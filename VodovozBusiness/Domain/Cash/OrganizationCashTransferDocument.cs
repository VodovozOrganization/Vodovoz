using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Cash
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "документы перемещения д/с между юр.лицами",
        Nominative = "документ перемещения д/с между юр.лицами")]
    [EntityPermission]
    [HistoryTrace]

    public class OrganizationCashTransferDocument : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        #region Свойства

        public virtual int Id { get; set; }

        [Display(Name = "Дата создания")]
        public virtual DateTime DocumentDate { get; set; }

        [Display(Name = "Автор")]
        public virtual Employee Author { get; set; }

        [Display(Name = "Организация откуда")]
        public virtual Organization OrganizationFrom { get; set; }

        [Display(Name = "Организация куда")]
        public virtual Organization OrganizationTo { get; set; }

        [Display(Name = "Операция на списание")]
        public virtual OrganisationCashMovementOperation OrganisationCashMovementOperationFrom { get; set; }

        [Display(Name = "Операция на зачисление")]
        public virtual OrganisationCashMovementOperation OrganisationCashMovementOperationTo { get; set; }

        [Display(Name = "Транспортируемая сумма")]
        public virtual decimal TransferedSum { get; set; }

        [Display(Name = "Комментарий")]
        public virtual string Comment { get; set; }

        #endregion

        #region IValidatableObject implementation

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OrganizationFrom == null)
                yield return new ValidationResult("Должна быть выбрана Организация откуда", new[] { nameof(OrganizationFrom) });
            if (OrganizationTo == null)
                yield return new ValidationResult("Должна быть выбрана Организация куда", new[] { nameof(OrganizationTo) });
            if (OrganizationFrom == OrganizationTo)
                yield return new ValidationResult("Должны быть выбраны разные организации", new[] { nameof(OrganizationFrom) });
            if (TransferedSum <= 0)
                yield return new ValidationResult("Некорректная сумма для перемещения", new[] { nameof(TransferedSum) });
            if (Comment?.Length > 255)
                yield return new ValidationResult("Cлишком длинный комментарий", new[] { nameof(TransferedSum) });
        }

        #endregion
    }
}

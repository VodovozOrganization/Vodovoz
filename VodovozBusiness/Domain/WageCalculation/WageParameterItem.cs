using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.WageCalculation
{
    [Appellative(
        Gender = GrammaticalGender.Masculine,
        NominativePlural = "типы параметров расчёта зарплаты",
        Nominative = "типа параметра расчёта зарплаты",
        Accusative = "тип параметра расчёта зарплаты",
        Genitive = "типу параметра расчёта зарплаты"
    )]
    [HistoryTrace]
    [EntityPermission]
    public abstract class WageParameterItem : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        public virtual int Id { get; set; }
        
        public abstract string Title { get; }
        
        public abstract WageParameterItemTypes WageParameterItemType { get; set; }

        #region IValidatableObject implementation

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }

        #endregion
    }
}
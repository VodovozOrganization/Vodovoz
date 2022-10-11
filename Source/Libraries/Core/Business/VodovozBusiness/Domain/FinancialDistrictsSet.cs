using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "версии финансовых районов",
        Nominative = "версия финансовых районов")]
    [EntityPermission]
    [HistoryTrace]
    public class FinancialDistrictsSet : PropertyChangedBase, IDomainObject, IValidatableObject, ICloneable
    {
        #region Свойства

        public virtual int Id { get; set; }

        private string name;
        [Display(Name = "Название версии финансовых районов")]
        public virtual string Name {
            get => name;
            set => SetField(ref name, value);
        }

        private Employee author;
        [Display(Name = "Автор")]
        public virtual Employee Author {
            get => author;
            set => SetField(ref author, value);
        }

        private DistrictsSetStatus status;
        [Display(Name = "Статус")]
        public virtual DistrictsSetStatus Status {
            get => status;
            set => SetField(ref status, value);
        }

        private DateTime dateCreated;
        [Display(Name = "Дата создания")]
        public virtual DateTime DateCreated {
            get => dateCreated;
            set => SetField(ref dateCreated, value);
        }

        private DateTime? dateActivated;
        [Display(Name = "Дата активации")]
        public virtual DateTime? DateActivated {
            get => dateActivated;
            set => SetField(ref dateActivated, value);
        }

        private DateTime? dateClosed;
        [Display(Name = "Дата закрытия")]
        public virtual DateTime? DateClosed {
            get => dateClosed;
            set => SetField(ref dateClosed, value);
        }

        private IList<FinancialDistrict> financialDistricts = new List<FinancialDistrict>();
        public virtual IList<FinancialDistrict> FinancialDistricts {
            get => financialDistricts;
            set => SetField(ref financialDistricts, value);
        }

        private GenericObservableList<FinancialDistrict> observableFinancialDistricts;
        //FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<FinancialDistrict> ObservableFinancialDistricts => 
            observableFinancialDistricts ?? 
            (observableFinancialDistricts = new GenericObservableList<FinancialDistrict>(FinancialDistricts));
        
        #endregion

        #region IValidatableObject implementation

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(String.IsNullOrWhiteSpace(Name)) {
                yield return new ValidationResult("Название версии должно быть обязательно заполнено",
                    new[] { nameof(this.Name) }
                );
            }
            
            if(FinancialDistricts == null || !FinancialDistricts.Any()) {
                yield return new ValidationResult("В версии районов должен присутствовать хотя бы один район",
                    new[] { nameof(this.FinancialDistricts) }
                );
            }
            
            if(FinancialDistricts != null) {
                foreach (FinancialDistrict finDistrict in FinancialDistricts) {
                    foreach (var validationResult in finDistrict.Validate(validationContext)) {
                        yield return validationResult;
                    }
                }
            }
        }

        #endregion
        
        #region ICloneable implementation

        public virtual object Clone()
        {
            var newDistrictsSet = new FinancialDistrictsSet {
                Name = Name,
                FinancialDistricts = new List<FinancialDistrict>()
            };
            foreach (var finDistrict in FinancialDistricts) {
                var newDistrict = finDistrict.Clone() as FinancialDistrict;
                newDistrict.FinancialDistrictsSet = newDistrictsSet;
                newDistrict.CopyOf = finDistrict;
                newDistrictsSet.FinancialDistricts.Add(newDistrict);
            }
            return newDistrictsSet;
        }

        #endregion
    }
}
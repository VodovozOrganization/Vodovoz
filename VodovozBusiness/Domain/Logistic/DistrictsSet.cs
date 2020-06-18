using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "наборы районов",
        Nominative = "набор районов")]
    [EntityPermission]
    public class DistrictsSet : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }

        private string name;
        [Display (Name = "Название набора районов")]
        public virtual string Name {
            get => name;
            set => SetField(ref name, value, () => Name);
        }
        
        private Employee creator;
        [Display (Name = "Создатель")]
        public virtual Employee Creator {
            get => creator;
            set => SetField(ref creator, value, () => Creator);
        }
        
        private IList<District> districts = new List<District>();
        public virtual IList<District> Districts {
            get => districts;
            set => SetField(ref districts, value, () => Districts);
        }

        private GenericObservableList<District> observableDistricts;
        //FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<District> ObservableDistricts =>
            observableDistricts ?? (observableDistricts = new GenericObservableList<District>(Districts));
    }
}
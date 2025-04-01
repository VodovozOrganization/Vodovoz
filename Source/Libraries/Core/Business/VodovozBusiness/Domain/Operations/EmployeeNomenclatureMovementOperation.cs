using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Operations 
{
    [Appellative (Gender = GrammaticalGender.Neuter,
        NominativePlural = "передвижения товаров",
        Nominative = "передвижение товаров")]
    public class EmployeeNomenclatureMovementOperation : OperationBase {

        private Employee employee;
        public virtual Employee Employee {
            get => employee;
            set => SetField (ref employee, value);
        }
        
        private Nomenclature nomenclature;
        public virtual Nomenclature Nomenclature {
            get => nomenclature;
            set => SetField (ref nomenclature, value);
        }
        
        private decimal amount;
        public virtual decimal Amount {
            get => amount;
            set => SetField (ref amount, value);
        }
    }
}
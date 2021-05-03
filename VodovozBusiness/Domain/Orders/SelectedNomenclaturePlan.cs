using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
    public class SelectedNomenclaturePlan : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }
        public virtual Nomenclature Nomenclature { get; set; }
    }
}

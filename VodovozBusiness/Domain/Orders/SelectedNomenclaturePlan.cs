using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Orders
{
    public class SelectedNomenclaturePlan : BusinessObjectBase<SelectedNomenclaturePlan>, IDomainObject
    {
        public virtual int Id { get; set; }
        public virtual int NomenclatureId { get; set; }
    }

}

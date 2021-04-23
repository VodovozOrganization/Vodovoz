using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
    public class SelectedNomenclaturePlan : BusinessObjectBase<SelectedNomenclaturePlan>, IDomainObject
    {
        public virtual int Id { get; set; }
        public virtual int NomenclatureId { get; set; }

        //private Nomenclature nomenclature;

        //[Display(Name = "Номенклатура")]
        //public virtual Nomenclature Nomenclature
        //{
        //    get => nomenclature;
        //    set => SetField(ref nomenclature, value);
        //}
    }

}

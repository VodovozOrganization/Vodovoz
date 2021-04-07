using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Permissions.Warehouse
{
    public class SubdivisionWarehousePermission: WarehousePermission, IDomainObject
    {
        public virtual int Id { get; set; }
        
        private Subdivision subdivision;
        [Display(Name = "Подразделение")]
        public virtual Subdivision Subdivision {
            get => subdivision;
            set => SetField(ref subdivision, value);
        }
    }
}
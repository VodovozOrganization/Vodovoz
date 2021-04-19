using System.Collections.Generic;
using QS.Project.Filter;
using QS.Project.Journal;
using Vodovoz.Domain.Store;

namespace Vodovoz.Filters.ViewModels
{
    public class WarehouseJournalFilterViewModel : FilterViewModelBase<WarehouseJournalFilterViewModel>, IJournalFilter
    {
        private bool restrictWithoutUnload;
        public bool RestrictWithoutUnload
        {
            get => restrictWithoutUnload;
            set => UpdateFilterField(ref restrictWithoutUnload, value, () => RestrictWithoutUnload);
        }

        private IList<Warehouse> warehouses;
        public virtual IList<Warehouse> Warehouses
        {
            get => warehouses;
            set => UpdateFilterField(ref warehouses, value, () => Warehouses);
        }

        private int warehousesAmount;
        public int WarehousesAmount
        {
            get => warehousesAmount;
            set => UpdateFilterField(ref warehousesAmount, value, () => WarehousesAmount);
        }

        private Warehouse restrictWarehouse;
        public Warehouse RestrictWarehouse
        {
            get => restrictWarehouse;
            set => UpdateFilterField(ref restrictWarehouse, value, () => RestrictWarehouse);
        }
    }
}
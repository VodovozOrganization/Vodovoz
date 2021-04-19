using QS.Project.Journal.EntitySelector;

namespace Vodovoz.TempAdapters
{
    public interface IWarehouseSelectorFactory
    {
        IEntityAutocompleteSelectorFactory CreateWarehouseAutocompleteSelectorFactory();
    }
}
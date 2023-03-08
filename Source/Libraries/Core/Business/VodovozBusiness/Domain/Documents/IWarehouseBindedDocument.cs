using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	public interface IWarehouseBindedDocument
	{
		Warehouse Warehouse { get; }
	}
}

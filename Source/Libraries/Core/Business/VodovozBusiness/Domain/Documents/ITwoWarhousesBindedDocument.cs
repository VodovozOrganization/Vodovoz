using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	public interface ITwoWarhousesBindedDocument : IDocument
	{
		Warehouse FromWarehouse { get; }
		Warehouse ToWarehouse { get; }
	}
}

using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	public interface ITwoWarhousesBindedDocument : IDocument
	{
		Warehouse WriteOffWarehouse { get; }
		Warehouse IncomingWarehouse { get; }
	}
}

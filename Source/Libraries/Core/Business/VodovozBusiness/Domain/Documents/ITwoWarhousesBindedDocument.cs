using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Domain.Documents
{
	public interface ITwoWarhousesBindedDocument : IDocument
	{
		Warehouse WriteOffWarehouse { get; }
		Warehouse IncomingWarehouse { get; }
	}
}

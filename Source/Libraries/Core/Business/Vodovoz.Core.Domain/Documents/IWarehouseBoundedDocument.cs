using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Core.Domain.Documents
{
	public interface IWarehouseBoundedDocument : IDocument
	{
		Warehouse Warehouse { get; }
	}
}

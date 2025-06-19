using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Domain.Documents
{
	public interface IWarehouseBoundedDocument : IDocument
	{
		Warehouse Warehouse { get; }
	}
}

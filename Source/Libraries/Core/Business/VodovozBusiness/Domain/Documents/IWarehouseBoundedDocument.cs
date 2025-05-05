using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	public interface IWarehouseBoundedDocument : IDocument
	{
		Warehouse Warehouse { get; }
	}
}

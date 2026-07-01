using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;

namespace Edo.Documents.Services
{
	public partial class UpdDocumentBuilder
	{
		private class UpdPendingCodeItem
		{
			public EdoUpdInventPositionCode CodeItem { get; set; }
			public GtinEntity Gtin { get; set; }
			public OrderItemEntity OrderItem { get; set; }
		}
	}
}

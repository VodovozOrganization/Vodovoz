using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;

namespace Edo.Documents.Services
{
	public partial class UpdDocumentBuilder
	{
		/// <summary>
		/// Элемент с кодом маркировки, который необходимо получить из пула кодов
		/// </summary>
		private class UpdPendingCodeItem
		{
			/// <summary>
			/// Код маркировки, который необходимо получить из пула кодов
			/// </summary>
			public EdoUpdInventPositionCode CodeItem { get; set; }

			/// <summary>
			/// GTIN номенклатуры, для которой необходимо получить код маркировки
			/// </summary>
			public GtinEntity Gtin { get; set; }

			/// <summary>
			/// Позиция заказа, для которой необходимо получить код маркировки
			/// </summary>
			public OrderItemEntity OrderItem { get; set; }
		}
	}
}

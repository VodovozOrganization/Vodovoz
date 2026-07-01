using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark;

namespace Edo.Documents.Services
{
	public partial class UpdDocumentBuilder
	{
		private class UpdDocumentCreationContext
		{
			public DocumentEdoTask DocumentEdoTask { get; set; }
			public List<EdoTaskItem> UnprocessedCodes { get; set; }
			public Dictionary<TrueMarkWaterGroupCode, IEnumerable<EdoTaskItem>> GroupCodesWithTaskItems { get; set; }
			public OrderItemEntity[] OrderItemsByPriceDesc { get; set; }
			public Dictionary<GtinEntity, int> CodesNeeded { get; set; }
			public List<UpdPendingCodeItem> PendingCodeItems { get; set; }
			public string OrganizationInn { get; set; }
		}
	}
}

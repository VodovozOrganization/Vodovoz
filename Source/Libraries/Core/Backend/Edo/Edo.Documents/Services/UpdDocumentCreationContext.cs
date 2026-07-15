using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark;

namespace Edo.Documents.Services
{
	public partial class UpdDocumentBuilder
	{
		/// <summary>
		/// Контекст создания документа УПД
		/// </summary>
		private class UpdDocumentCreationContext
		{
			/// <summary>
			/// Идентификатор ЭДО задачи
			/// </summary>
			public DocumentEdoTask DocumentEdoTask { get; set; }

			/// <summary>
			/// Коллекция кодов маркировки, которые не удалось обработать при создании документа
			/// </summary>
			public List<EdoTaskItem> UnprocessedCodes { get; set; }

			/// <summary>
			/// Словарь групповых кодов маркировки с соответствующими им задачами ЭДО
			/// </summary>
			public Dictionary<TrueMarkWaterGroupCode, IEnumerable<EdoTaskItem>> GroupCodesWithTaskItems { get; set; }

			/// <summary>
			/// Массив позиций заказа, отсортированных по убыванию цены
			/// </summary>
			public OrderItemEntity[] OrderItemsByPriceDesc { get; set; }

			/// <summary>
			/// Словарь GTIN -> количество кодов маркировки, которые необходимо получить из пула кодов
			/// </summary>
			public Dictionary<GtinEntity, int> CodesNeeded { get; set; }

			/// <summary>
			/// Коллекция элементов с кодами маркировки, которые необходимо получить из пула кодов
			/// </summary>
			public List<UpdPendingCodeItem> PendingCodeItems { get; set; }

			/// <summary>
			/// ИНН организации, для которой создается документ
			/// </summary>
			public string OrganizationInn { get; set; }
		}
	}
}

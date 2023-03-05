using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using System;
using System.Linq;
using Vodovoz.Domain.Documents;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Store
{
	public class WarehouseDocumentsItemsJournalNode<TEntity> : WarehouseDocumentsItemsJournalNode
		where TEntity : class, IDomainObject
	{
		public WarehouseDocumentsItemsJournalNode() : base(typeof(TEntity)) { }
	}

	public class WarehouseDocumentsItemsJournalNode : JournalEntityNodeBase
	{
		private Type[] _supportedDocuments = new[]
		{
			typeof(IncomingInvoiceItem),
			typeof(IncomingWaterMaterial),
			typeof(MovementDocumentItem),
			typeof(WriteoffDocumentItem),
			typeof(SelfDeliveryDocumentItem),
			typeof(CarLoadDocumentItem),
			typeof(CarUnloadDocumentItem),
			typeof(InventoryDocumentItem),
			typeof(ShiftChangeWarehouseDocumentItem),
			typeof(RegradingOfGoodsDocumentItem),
			typeof(DeliveryDocumentItem)
		};

		protected WarehouseDocumentsItemsJournalNode(Type entityType) : base(entityType)
		{
			if(!_supportedDocuments.Contains(entityType))
			{
				throw new ArgumentOutOfRangeException(nameof(entityType));
			}

			Title = GetTitle(entityType);
		}

		private string GetTitle(Type type) => type.GetAttribute<AppellativeAttribute>(true)?.Nominative;
	}
}

using System;
using Vodovoz.Core.Domain.Warehouses.Documents;

namespace Vodovoz.ViewModels.ViewModels.Warehouses
{
	public class InventoryInstanceMovementHistoryNode
	{
		public DateTime Date { get; set; }
		public string Document { get; set; }
		public int DocumentId { get; set; }
		public DocumentType DocumentType { get; set; }
		public string Sender { get; set; }
		public string Receiver { get; set; }
		public string Author { get; set; }
		public string Editor { get; set; }
		public string Comment { get; set; }
	}
}

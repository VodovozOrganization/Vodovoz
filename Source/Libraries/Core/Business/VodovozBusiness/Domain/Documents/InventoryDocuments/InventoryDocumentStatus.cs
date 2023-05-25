using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents.InventoryDocuments
{
	public enum InventoryDocumentStatus
	{
		[Display(Name = "В работе")]
		InProcess,
		[Display(Name = "Подтвержден")]
		Confirmed
	}
}

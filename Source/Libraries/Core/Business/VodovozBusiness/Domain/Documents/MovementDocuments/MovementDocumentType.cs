using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents.MovementDocuments
{
	public enum MovementDocumentType
	{
		[Display(Name = "Внутреннее перемещение")]
		InnerTransfer,
		[Display(Name = "Транспортировка")]
		Transportation
	}
}

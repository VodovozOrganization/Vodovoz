using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents.MovementDocuments
{
	public enum MovementDocumentStatus
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Отправлен")]
		Sended,
		[Display(Name = "Расхождение")]
		Discrepancy,
		[Display(Name = "Принят")]
		Accepted
	}
}

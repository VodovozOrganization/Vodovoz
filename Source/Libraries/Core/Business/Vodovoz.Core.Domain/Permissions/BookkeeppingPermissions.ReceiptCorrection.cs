using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class BookkeeppingPermissions
	{
		[Display(Name = "Доступ к реестру пояснительных записок")]
		public static string CanViewReceiptCorrectionExplanatoryNotesJournal => nameof(CanViewReceiptCorrectionExplanatoryNotesJournal);
	}
}

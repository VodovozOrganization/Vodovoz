using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum FiscalDocumentType
	{
		[Display(Name = "Приход")]
		Sale,
		[Display(Name = "Возврат прихода")]
		Return,
		[Display(Name = "Расход")]
		Buy,
		[Display(Name = "Возврат расхода")]
		BuyReturn,
		[Display(Name = "Чек коррекции прихода")]
		SaleCorrection,
		[Display(Name = "Чек коррекции возврата прихода")]
		SaleReturnCorrection
	}
}

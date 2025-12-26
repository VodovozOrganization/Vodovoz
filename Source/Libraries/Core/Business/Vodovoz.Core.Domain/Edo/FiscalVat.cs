using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Тег НДС согласно ФЗ-54
	/// </summary>
	public enum FiscalVat
	{
		[Display(Name = "НДС 0%")]
		Vat0 = 1104,

		[Display(Name = "НДС 10%")]
		Vat10 = 1103,

		[Display(Name = "НДС 20%")]
		Vat20 = 1102,

		[Display(Name = "НДС не облагается")]
		VatFree = 1105,

		[Display(Name = "НДС с рассч. ставкой 20%")]
		VatEstimatedRate20 = 1106,

		[Display(Name = "НДС с рассч. ставкой 10%")]
		VatEstimatedRate10 = 1107,

		[Display(Name = "НДС 5%")]
		Vat5 = 1109,

		[Display(Name = "НДС 7%")]
		Vat7 = 1110,

		[Display(Name = "НДС с рассч. ставкой 5%")]
		VatEstimatedRate5 = 1111,

		[Display(Name = "НДС с рассч. Ставкой 7%")]
		VatEstimatedRate7 = 1112,
		
		[Display(Name = "НДС 22%")]
		Vat22 = 1113,
		
		[Display(Name = "НДС с рассч. ставкой 20%")]
		VatEstimatedRate22 = 1114,
	}
}

using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Attributes;

namespace Vodovoz.Core.Domain.Goods
{
	public enum VAT
	{
		[Display(Name = "Без НДС")]
		[Value1c("БезНДС")]
		[Value1cComplexAutomation("БезНДС")]
		[Value1cType("БезНДС")]
		No,
		[Display(Name = "НДС 10%")]
		[Value1c("НДС10")]
		[Value1cComplexAutomation("10%")]
		[Value1cType("Пониженная")]
		Vat10,
		[Display(Name = "НДС 18%")]
		[Value1c("НДС18")]
		[Value1cComplexAutomation("18%")]
		[Value1cType("Общая")]
		Vat18,
		[Display(Name = "НДС 20%")]
		[Value1c("НДС20")]
		[Value1cComplexAutomation("20%")]
		[Value1cType("Общая")]
		Vat20
	}
}

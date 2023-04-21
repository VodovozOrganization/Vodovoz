﻿using System.ComponentModel.DataAnnotations;
using Vodovoz.Attributes;

namespace Vodovoz.Domain.Goods
{
	public enum VAT
	{
		[Display(Name = "Без НДС")]
		[Value1c("БезНДС")]
		[Value1cType("БезНДС")]
		No,
		[Display(Name = "НДС 10%")]
		[Value1c("НДС10")]
		[Value1cType("Пониженная")]
		Vat10,
		[Display(Name = "НДС 18%")]
		[Value1c("НДС18")]
		[Value1cType("Общая")]
		Vat18,
		[Display(Name = "НДС 20%")]
		[Value1c("НДС20")]
		[Value1cType("Общая")]
		Vat20
	}
}

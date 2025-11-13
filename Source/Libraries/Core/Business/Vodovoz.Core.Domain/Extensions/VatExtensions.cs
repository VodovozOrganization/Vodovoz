using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Extensions
{
	public static class VatExtensions
	{
		public static FiscalVat ToFiscalVat(this VAT vat)
		{
			switch(vat)
			{
				case VAT.No:
					return FiscalVat.VatFree;
				case VAT.Vat10:
					return FiscalVat.Vat10;
				case VAT.Vat18:
					throw new InvalidOperationException("В чеках нет возможности устанавливать НДС 18%. Скорее всего ошибка в заполнении карточки товара");
				case VAT.Vat20:
					return FiscalVat.Vat20;
				default:
					throw new InvalidOperationException("Нет соответствия между НДС товара и FiscalVat, проверьте карточку товара");
			}
		}
	}
}

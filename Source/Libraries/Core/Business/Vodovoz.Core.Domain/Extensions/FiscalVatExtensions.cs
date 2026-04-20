using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Domain.Extensions
{
	public static class FiscalVatExtensions
	{
		public static decimal? ToAddedVat(this FiscalVat source)
		{
			switch(source)
			{
				case FiscalVat.Vat0:
					return 0m;
				case FiscalVat.Vat5:
					return 0.05m;
				case FiscalVat.Vat7:
					return 0.07m;
				case FiscalVat.Vat10:
					return 0.10m;
				case FiscalVat.Vat20:
					return 0.20m;
				case FiscalVat.Vat22:
					return 0.22m;
				case FiscalVat.VatFree:
					default:
						return null;
			}
		}
	}
}

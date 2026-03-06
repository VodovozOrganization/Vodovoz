using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Core.Domain.TrueMark;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class TrueMarkWaterIdentificationCodeSpecification
	{
		public static ExpressionSpecification<TrueMarkWaterIdentificationCode> CreateForValidCode()
			=> new ExpressionSpecification<TrueMarkWaterIdentificationCode>(
				c => !c.IsInvalid);

		public static ExpressionSpecification<TrueMarkWaterIdentificationCode> CreateForGtin(string gtin)
			=> new ExpressionSpecification<TrueMarkWaterIdentificationCode>(
				c => c.Gtin == gtin);

		public static ExpressionSpecification<TrueMarkWaterIdentificationCode> CreateForSerialNumber(string serialNumber)
			=> new ExpressionSpecification<TrueMarkWaterIdentificationCode>(
				c => c.SerialNumber == serialNumber);

		public static ExpressionSpecification<TrueMarkWaterIdentificationCode> CreateForSerialNumbers(IEnumerable<string> serialNumbers)
			=> new ExpressionSpecification<TrueMarkWaterIdentificationCode>(
				c => serialNumbers.Contains(c.SerialNumber));

		public static ExpressionSpecification<TrueMarkWaterIdentificationCode> CreateForValidGtinSerialNumber(string gtin, string serialNumber)
			=> CreateForValidCode()
			& CreateForGtin(gtin)
			& CreateForSerialNumber(serialNumber);
	}
}

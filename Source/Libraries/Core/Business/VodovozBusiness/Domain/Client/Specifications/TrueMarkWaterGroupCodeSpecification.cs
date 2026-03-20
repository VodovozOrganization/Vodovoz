using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Core.Domain.TrueMark;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class TrueMarkWaterGroupCodeSpecification
	{
		public static ExpressionSpecification<TrueMarkWaterGroupCode> CreateForValidCode()
			=> new ExpressionSpecification<TrueMarkWaterGroupCode>(
				c => !c.IsInvalid);

		public static ExpressionSpecification<TrueMarkWaterGroupCode> CreateForGtin(string gtin)
			=> new ExpressionSpecification<TrueMarkWaterGroupCode>(
				c => c.GTIN == gtin);

		public static ExpressionSpecification<TrueMarkWaterGroupCode> CreateForSerialNumber(string serialNumber)
			=> new ExpressionSpecification<TrueMarkWaterGroupCode>(
				c => c.SerialNumber == serialNumber);

		public static ExpressionSpecification<TrueMarkWaterGroupCode> CreateForSerialNumbers(IEnumerable<string> serialNumbers)
			=> new ExpressionSpecification<TrueMarkWaterGroupCode>(
				c => serialNumbers.Contains(c.SerialNumber));

		public static ExpressionSpecification<TrueMarkWaterGroupCode> CreateForValidGtinSerialNumber(string gtin, string serialNumber)
			=> CreateForValidCode()
			& CreateForGtin(gtin)
			& CreateForSerialNumber(serialNumber);
	}
}

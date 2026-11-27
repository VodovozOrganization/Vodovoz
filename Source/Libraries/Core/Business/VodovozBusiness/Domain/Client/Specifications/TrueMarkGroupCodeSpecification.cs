using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Core.Domain.TrueMark;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class TrueMarkGroupCodeSpecification
	{
		public static ExpressionSpecification<TrueMarkGroupCode> CreateForValidCode()
			=> new ExpressionSpecification<TrueMarkGroupCode>(
				c => !c.IsInvalid);

		public static ExpressionSpecification<TrueMarkGroupCode> CreateForGtin(string gtin)
			=> new ExpressionSpecification<TrueMarkGroupCode>(
				c => c.GTIN == gtin);

		public static ExpressionSpecification<TrueMarkGroupCode> CreateForSerialNumber(string serialNumber)
			=> new ExpressionSpecification<TrueMarkGroupCode>(
				c => c.SerialNumber == serialNumber);

		public static ExpressionSpecification<TrueMarkGroupCode> CreateForSerialNumbers(IEnumerable<string> serialNumbers)
			=> new ExpressionSpecification<TrueMarkGroupCode>(
				c => serialNumbers.Contains(c.SerialNumber) && !c.IsInvalid);

		public static ExpressionSpecification<TrueMarkGroupCode> CreateForValidGtinSerialNumber(string gtin, string serialNumber)
			=> CreateForValidCode()
			& CreateForGtin(gtin)
			& CreateForSerialNumber(serialNumber);
	}
}

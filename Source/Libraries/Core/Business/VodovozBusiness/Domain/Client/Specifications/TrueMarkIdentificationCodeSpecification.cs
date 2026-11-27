using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Core.Domain.TrueMark;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class TrueMarkIdentificationCodeSpecification
	{
		public static ExpressionSpecification<TrueMarkIdentificationCode> CreateForValidCode()
			=> new ExpressionSpecification<TrueMarkIdentificationCode>(
				c => !c.IsInvalid);

		public static ExpressionSpecification<TrueMarkIdentificationCode> CreateForGtin(string gtin)
			=> new ExpressionSpecification<TrueMarkIdentificationCode>(
				c => c.Gtin == gtin);

		public static ExpressionSpecification<TrueMarkIdentificationCode> CreateForSerialNumber(string serialNumber)
			=> new ExpressionSpecification<TrueMarkIdentificationCode>(
				c => c.SerialNumber == serialNumber);

		public static ExpressionSpecification<TrueMarkIdentificationCode> CreateForSerialNumbers(IEnumerable<string> serialNumbers)
			=> new ExpressionSpecification<TrueMarkIdentificationCode>(
				c => serialNumbers.Contains(c.SerialNumber) && !c.IsInvalid);

		public static ExpressionSpecification<TrueMarkIdentificationCode> CreateForValidGtinSerialNumber(string gtin, string serialNumber)
			=> CreateForValidCode()
			& CreateForGtin(gtin)
			& CreateForSerialNumber(serialNumber);
	}
}

using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Core.Domain.TrueMark;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class TrueMarkTransportCodeSpecification
	{
		public static ExpressionSpecification<TrueMarkTransportCode> CreateForRawCode(string rawCode)
			=> new ExpressionSpecification<TrueMarkTransportCode>(
				c => c.RawCode == rawCode);
		public static ExpressionSpecification<TrueMarkTransportCode> CreateForRawCodes(IEnumerable<string> rawCodes)
			=> new ExpressionSpecification<TrueMarkTransportCode>(
				c => rawCodes.Contains(c.RawCode));
	}
}

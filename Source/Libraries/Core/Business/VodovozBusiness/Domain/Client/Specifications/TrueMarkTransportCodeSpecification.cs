using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Core.Domain.TrueMark;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class TrueMarkTransportCodeSpecification
	{
		public static ExpressionSpecification<TrueMarkTransportCode> CreateForRawCode(string rawCode)
			=> new ExpressionSpecification<TrueMarkTransportCode>(
				c => c.RawCode == rawCode);
	}
}

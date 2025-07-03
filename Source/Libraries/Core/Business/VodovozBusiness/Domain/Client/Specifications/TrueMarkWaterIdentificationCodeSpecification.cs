using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Specifications;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class TrueMarkWaterIdentificationCodeSpecification
	{
		public static ExpressionSpecification<TrueMarkWaterIdentificationCode> CreateForValidCode()
			=> new ExpressionSpecification<TrueMarkWaterIdentificationCode>(
				c => !c.IsInvalid);

		public static ExpressionSpecification<TrueMarkWaterIdentificationCode> CreateForGtin(string gtin)
			=> new ExpressionSpecification<TrueMarkWaterIdentificationCode>(
				c => c.GTIN == gtin);

		public static ExpressionSpecification<TrueMarkWaterIdentificationCode> CreateForSerialNumber(string serialNumber)
			=> new ExpressionSpecification<TrueMarkWaterIdentificationCode>(
				c => c.SerialNumber == serialNumber);

		public static ExpressionSpecification<TrueMarkWaterIdentificationCode> CreateForValidGtinSerialNumber(string gtin, string serialNumber)
			=> CreateForValidCode()
			& CreateForGtin(gtin)
			& CreateForSerialNumber(serialNumber);
	}
}

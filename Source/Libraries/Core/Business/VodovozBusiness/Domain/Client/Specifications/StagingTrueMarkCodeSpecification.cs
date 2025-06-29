using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Specifications;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class StagingTrueMarkCodeSpecification
	{
		public static ExpressionSpecification<StagingTrueMarkCode> CreateForCode(StagingTrueMarkCode code)
			=> new ExpressionSpecification<StagingTrueMarkCode>(
				c => ((code.IsTransport && c.RawCode == code.RawCode)
					|| (!code.IsTransport && c.GTIN == code.GTIN && c.SerialNumber == code.SerialNumber))
				&& c.Id != code.Id
				&& c.RelatedDocumentType == code.RelatedDocumentType
				&& c.RelatedDocumentId == code.RelatedDocumentId
				&& c.OrderItem.Id == code.OrderItem.Id);
	}
}

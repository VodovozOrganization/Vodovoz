using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Specifications;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class StagingTrueMarkCodeSpecification
	{
		public static ExpressionSpecification<StagingTrueMarkCode> CreateForRelatedDocument(
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId)
			=> new ExpressionSpecification<StagingTrueMarkCode>(
				c => c.RelatedDocumentType == relatedDocumentType
					&& c.RelatedDocumentId == relatedDocumentId);

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForCode(StagingTrueMarkCode code)
			=> new ExpressionSpecification<StagingTrueMarkCode>(
				c => ((code.IsTransport && c.RawCode == code.RawCode)
						|| (!code.IsTransport && c.GTIN == code.GTIN && c.SerialNumber == code.SerialNumber))
					&& c.Id != code.Id
					&& c.RelatedDocumentType == code.RelatedDocumentType
					&& c.RelatedDocumentId == code.RelatedDocumentId
					&& c.OrderItemId == code.OrderItemId);

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForRelatedDocumentOrderItemIdentificationCodesExcludeIds(
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			int orderItemId,
			IEnumerable<int> excludeIds)
			=> new ExpressionSpecification<StagingTrueMarkCode>(
				c => c.RelatedDocumentType == relatedDocumentType
					&& c.RelatedDocumentId == relatedDocumentId
					&& c.OrderItemId == orderItemId
					&& c.CodeType == StagingTrueMarkCodeType.Identification
					&& !excludeIds.Contains(c.Id));
	}
}

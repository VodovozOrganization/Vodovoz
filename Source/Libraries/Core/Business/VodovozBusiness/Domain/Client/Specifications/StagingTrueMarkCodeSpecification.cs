using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Specifications;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class StagingTrueMarkCodeSpecification
	{
		public static ExpressionSpecification<StagingTrueMarkCode> CreateForExcludeCodeId(int codeId)
			=> new ExpressionSpecification<StagingTrueMarkCode>(
				c => c.Id != codeId);

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForExcludeCodesIds(IEnumerable<int> codeIds)
			=> new ExpressionSpecification<StagingTrueMarkCode>(
				c => !codeIds.Contains(c.Id));

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForRelatedDocument(
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId)
			=> new ExpressionSpecification<StagingTrueMarkCode>(
				c => c.RelatedDocumentType == relatedDocumentType
					&& c.RelatedDocumentId == relatedDocumentId);

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForRelatedDocuments(
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			IEnumerable<int> relatedDocumentsIds)
			=> new ExpressionSpecification<StagingTrueMarkCode>(
				c => c.RelatedDocumentType == relatedDocumentType
					&& relatedDocumentsIds.Contains(c.RelatedDocumentId));

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForOrderItemId(int? orderItemId)
			=> new ExpressionSpecification<StagingTrueMarkCode>(
				c => (c.OrderItemId == null && orderItemId == null) || c.OrderItemId == orderItemId);

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForCodeType(StagingTrueMarkCodeType codeType)
			=> new ExpressionSpecification<StagingTrueMarkCode>(
				c => c.CodeType == codeType);

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForCodeData(
			bool isTransportCode,
			string rawCode,
			string gtin,
			string serialNumber)
			=> new ExpressionSpecification<StagingTrueMarkCode>(
				c => ((isTransportCode && c.RawCode == rawCode) || (!isTransportCode && c.Gtin == gtin && c.SerialNumber == serialNumber)));

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForRelatedDocumentOrderIdCodeData(
			bool isTransportCode,
			string rawCode,
			string gtin,
			string serialNumber,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			int? orderItemid)
			=> CreateForCodeData(isTransportCode, rawCode, gtin, serialNumber)
				& CreateForRelatedDocument(relatedDocumentType, relatedDocumentId)
				& CreateForOrderItemId(orderItemid);

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForStagingCodeDuplicates(StagingTrueMarkCode code)
			=> CreateForCodeData(code.IsTransport, code.RawCode, code.Gtin, code.SerialNumber)
				& CreateForRelatedDocument(code.RelatedDocumentType, code.RelatedDocumentId)
				& CreateForOrderItemId(code.OrderItemId)
				& CreateForExcludeCodeId(code.Id);

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForEqualStagingCodes(StagingTrueMarkCode code)
			=> CreateForCodeData(code.IsTransport, code.RawCode, code.Gtin, code.SerialNumber)
				& CreateForRelatedDocument(code.RelatedDocumentType, code.RelatedDocumentId)
				& CreateForOrderItemId(code.OrderItemId);

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForRelatedDocumentOrderItemIdentificationCodes(
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			int? orderItemId)
			=> CreateForRelatedDocument(relatedDocumentType, relatedDocumentId)
				& CreateForOrderItemId(orderItemId)
				& CreateForCodeType(StagingTrueMarkCodeType.Identification);

		public static ExpressionSpecification<StagingTrueMarkCode> CreateForRelatedDocumentOrderItemIdentificationCodesExcludeIds(
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			int? orderItemId,
			IEnumerable<int> excludeIds)
			=> CreateForRelatedDocumentOrderItemIdentificationCodes(relatedDocumentType, relatedDocumentId, orderItemId)
				& CreateForExcludeCodesIds(excludeIds);
	}
}

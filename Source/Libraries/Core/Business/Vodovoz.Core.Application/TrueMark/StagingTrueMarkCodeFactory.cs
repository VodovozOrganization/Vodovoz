using System;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Interfaces.TrueMark;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Application.TrueMark
{
	internal sealed class StagingTrueMarkCodeFactory : IStagingTrueMarkCodeFactory
	{
		public StagingTrueMarkCode CreateTransportCodeFromRawCode(
			string rawCode,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			int? orderItemId = null)
		{
			return new StagingTrueMarkCode
			{
				RawCode = rawCode,
				CodeType = StagingTrueMarkCodeType.Transport,
				RelatedDocumentType = relatedDocumentType,
				RelatedDocumentId = relatedDocumentId,
				OrderItemId = orderItemId
			};
		}

		public StagingTrueMarkCode CreateGroupCodeFromParsedCode(
			ITrueMarkCode parsedCode,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			int? orderItemId = null)
		{
			return CreateFromParsedCode(
				parsedCode,
				StagingTrueMarkCodeType.Group,
				relatedDocumentType,
				relatedDocumentId,
				orderItemId);
		}

		public StagingTrueMarkCode CreateGroupCodeFromProductInstanceStatus(
			ProductInstanceStatus productInstanceStatus,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			int? orderItemId = null)
		{
			return CreateFromProductInstanceStatus(
				productInstanceStatus,
				StagingTrueMarkCodeType.Group,
				relatedDocumentType,
				relatedDocumentId,
				orderItemId);
		}

		public StagingTrueMarkCode CreateIdentificationCodeFromParsedCode(
			ITrueMarkCode parsedCode,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			int? orderItemId = null)
		{
			return CreateFromParsedCode(
				parsedCode,
				StagingTrueMarkCodeType.Identification,
				relatedDocumentType,
				relatedDocumentId,
				orderItemId);
		}

		public StagingTrueMarkCode CreateIdentificationCodeFromProductInstanceStatus(
			ProductInstanceStatus productInstanceStatus,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			int? orderItemId = null)
		{
			return CreateFromProductInstanceStatus(
				productInstanceStatus,
				StagingTrueMarkCodeType.Identification,
				relatedDocumentType,
				relatedDocumentId,
				orderItemId);
		}

		private StagingTrueMarkCode CreateFromParsedCode(
			ITrueMarkCode parsedCode,
			StagingTrueMarkCodeType codeType,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			int? orderItemId = null)
		{
			return new StagingTrueMarkCode
			{
				RawCode = parsedCode.SourceCode.Substring(0, Math.Min(255, parsedCode.SourceCode.Length)),
				Gtin = parsedCode.Gtin,
				SerialNumber = parsedCode.SerialNumber,
				CheckCode = parsedCode.CheckCode,
				ProductGroup = parsedCode.ProductGroup,
				CodeType = codeType,
				RelatedDocumentType = relatedDocumentType,
				RelatedDocumentId = relatedDocumentId,
				OrderItemId = orderItemId
			};
		}

		private StagingTrueMarkCode CreateFromProductInstanceStatus(
			ProductInstanceStatus productInstanceStatus,
			StagingTrueMarkCodeType codeType,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			int? orderItemId = null)
		{
			var identificationCode = productInstanceStatus.IdentificationCode;

			var rawCode = "\\u001d" + identificationCode + "\\u001d";

			var serialNumber = TrueMarkCodeHelper.ExtractSerialNumber(identificationCode, productInstanceStatus.Gtin);

			if(!productInstanceStatus.ProductGroup.HasValue)
			{
				throw new InvalidOperationException(
					$"Не удалось определить товарную группу маркировки для кода {identificationCode}.");
			}

			return new StagingTrueMarkCode
			{
				RawCode = rawCode,
				Gtin = productInstanceStatus.Gtin,
				SerialNumber = serialNumber,
				ProductGroup = productInstanceStatus.ProductGroup.Value,
				CodeType = codeType,
				RelatedDocumentType = relatedDocumentType,
				RelatedDocumentId = relatedDocumentId,
				OrderItemId = orderItemId
			};
		}
	}
}

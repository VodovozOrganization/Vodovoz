using System;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	internal sealed class StagingTrueMarkCodeFactory : IStagingTrueMarkCodeFactory
	{
		public StagingTrueMarkCode CreateTransportCodeFromRawCode(
			string rawCode,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			OrderItemEntity orderItem)
		{
			return new StagingTrueMarkCode
			{
				RawCode = rawCode,
				CodeType = StagingTrueMarkCodeType.Transport,
				RelatedDocumentType = relatedDocumentType,
				RelatedDocumentId = relatedDocumentId,
				OrderItemId = orderItem.Id
			};
		}

		public StagingTrueMarkCode CreateGroupCodeFromParsedCode(
			TrueMarkWaterCode parsedCode,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			OrderItemEntity orderItem)
		{
			return CreateFromParsedCode(
				parsedCode,
				StagingTrueMarkCodeType.Group,
				relatedDocumentType,
				relatedDocumentId,
				orderItem);
		}

		public StagingTrueMarkCode CreateGroupCodeFromProductInstanceStatus(
			ProductInstanceStatus productInstanceStatus,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			OrderItemEntity orderItem)
		{
			return CreateFromProductInstanceStatus(
				productInstanceStatus,
				StagingTrueMarkCodeType.Group,
				relatedDocumentType,
				relatedDocumentId,
				orderItem);
		}

		public StagingTrueMarkCode CreateIdentificationCodeFromParsedCode(
			TrueMarkWaterCode parsedCode,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			OrderItemEntity orderItem)
		{
			return CreateFromParsedCode(
				parsedCode,
				StagingTrueMarkCodeType.Identification,
				relatedDocumentType,
				relatedDocumentId,
				orderItem);
		}

		public StagingTrueMarkCode CreateIdentificationCodeFromProductInstanceStatus(
			ProductInstanceStatus productInstanceStatus,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			OrderItemEntity orderItem)
		{
			return CreateFromProductInstanceStatus(
				productInstanceStatus,
				StagingTrueMarkCodeType.Identification,
				relatedDocumentType,
				relatedDocumentId,
				orderItem);
		}

		private StagingTrueMarkCode CreateFromParsedCode(
			TrueMarkWaterCode parsedCode,
			StagingTrueMarkCodeType codeType,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			OrderItemEntity orderItem)
		{
			return new StagingTrueMarkCode
			{
				RawCode = parsedCode.SourceCode.Substring(0, Math.Min(255, parsedCode.SourceCode.Length)),
				Gtin = parsedCode.GTIN,
				SerialNumber = parsedCode.SerialNumber,
				CheckCode = parsedCode.CheckCode,
				CodeType = codeType,
				RelatedDocumentType = relatedDocumentType,
				RelatedDocumentId = relatedDocumentId,
				OrderItemId = orderItem.Id
			};
		}

		private StagingTrueMarkCode CreateFromProductInstanceStatus(
			ProductInstanceStatus productInstanceStatus,
			StagingTrueMarkCodeType codeType,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			OrderItemEntity orderItem)
		{
			var identificationCode = productInstanceStatus.IdentificationCode;

			var rawCode = "\\u001d" + identificationCode + "\\u001d";

			var serialNumber = identificationCode
				.Replace(productInstanceStatus.Gtin, "")
				.Replace("0121", "");

			return new StagingTrueMarkCode
			{
				RawCode = rawCode,
				Gtin = productInstanceStatus.Gtin,
				SerialNumber = serialNumber,
				CodeType = codeType,
				RelatedDocumentType = relatedDocumentType,
				RelatedDocumentId = relatedDocumentId,
				OrderItemId = orderItem.Id
			};
		}
	}
}

using iTextSharp.text.pdf;
using System;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	internal sealed class TrueMarkWaterGroupCodeFactory : ITrueMarkWaterGroupCodeFactory
	{
		public TrueMarkWaterGroupCode CreateFromParsedCode(TrueMarkWaterCode parsedCode)
		{
			return new TrueMarkWaterGroupCode
			{
				IsInvalid = false,
				RawCode = parsedCode.SourceCode.Substring(0, Math.Min(255, parsedCode.SourceCode.Length)),
				GTIN = parsedCode.Gtin,
				SerialNumber = parsedCode.SerialNumber,
				CheckCode = parsedCode.CheckCode
			};
		}

		public TrueMarkWaterGroupCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus)
		{
			var identificationCode = productInstanceStatus.IdentificationCode;

			var rawCode = "\\u001d" + identificationCode + "\\u001d";

			var serialNumber = identificationCode
				.Replace(productInstanceStatus.Gtin, "")
				.Replace("0121", "");

			return new TrueMarkWaterGroupCode
			{
				IsInvalid = false,
				RawCode = rawCode,
				GTIN = productInstanceStatus.Gtin,
				SerialNumber = serialNumber
			};
		}

		public TrueMarkWaterGroupCode CreateFromStagingCode(StagingTrueMarkCode stagingCode)
		{
			if(stagingCode?.IsGroup == false)
			{
				throw new ArgumentException(
					$"Код {stagingCode?.IdentificationCode} не является групповым кодом",
					nameof(stagingCode));
			}

			return new TrueMarkWaterGroupCode
			{
				IsInvalid = false,
				RawCode = stagingCode.RawCode,
				GTIN = stagingCode.Gtin,
				SerialNumber = stagingCode.SerialNumber
			};
		}
	}
}

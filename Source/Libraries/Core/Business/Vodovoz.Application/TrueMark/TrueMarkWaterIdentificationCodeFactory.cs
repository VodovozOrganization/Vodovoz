using System;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	internal sealed class TrueMarkWaterIdentificationCodeFactory : ITrueMarkWaterIdentificationCodeFactory
	{
		public TrueMarkWaterIdentificationCode CreateFromParsedCode(TrueMarkWaterCode parsedCode)
		{
			return new TrueMarkWaterIdentificationCode
			{
				IsInvalid = false,
				RawCode = parsedCode.SourceCode.Substring(0, Math.Min(255, parsedCode.SourceCode.Length)),
				Gtin = parsedCode.Gtin,
				SerialNumber = parsedCode.SerialNumber,
				CheckCode = parsedCode.CheckCode
			};
		}

		public TrueMarkWaterIdentificationCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus)
		{
			var identificationCode = productInstanceStatus.IdentificationCode;

			var rawCode = "\\u001d" + identificationCode + "\\u001d";

			var serialNumber = identificationCode
				.Replace(productInstanceStatus.Gtin, "")
				.Replace("0121", "");

			return new TrueMarkWaterIdentificationCode
			{
				IsInvalid = false,
				RawCode = rawCode,
				Gtin = productInstanceStatus.Gtin,
				SerialNumber = serialNumber
			};
		}
	}
}

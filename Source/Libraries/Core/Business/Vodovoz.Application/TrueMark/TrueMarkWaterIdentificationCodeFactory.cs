﻿using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Application.TrueMark
{
	internal sealed class TrueMarkWaterIdentificationCodeFactory : ITrueMarkWaterIdentificationCodeFactory
	{
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
				GTIN = productInstanceStatus.Gtin,
				SerialNumber = serialNumber
			};
		}
	}
}

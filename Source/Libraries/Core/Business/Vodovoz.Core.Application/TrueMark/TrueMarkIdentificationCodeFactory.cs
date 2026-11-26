using System;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Interfaces.TrueMark;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Application.TrueMark
{
	internal sealed class TrueMarkIdentificationCodeFactory : ITrueMarkIdentificationCodeFactory
	{
		/// <inheritdoc/>
		public TrueMarkIdentificationCode CreateFromParsedCode(ITrueMarkCode parsedCode)
		{
			return new TrueMarkIdentificationCode
			{
				IsInvalid = false,
				RawCode = parsedCode.SourceCode.Substring(0, Math.Min(255, parsedCode.SourceCode.Length)),
				Gtin = parsedCode.Gtin,
				SerialNumber = parsedCode.SerialNumber,
				CheckCode = parsedCode.CheckCode,
				ProductGroup = parsedCode.ProductGroup
			};
		}

		/// <inheritdoc/>
		public TrueMarkIdentificationCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus)
		{
			var identificationCode = productInstanceStatus.IdentificationCode;

			var rawCode = "\\u001d" + identificationCode + "\\u001d";

			var serialNumber = TrueMarkCodeHelper.ExtractSerialNumber(identificationCode, productInstanceStatus.Gtin);

			if(!productInstanceStatus.ProductGroup.HasValue)
			{
				throw new InvalidOperationException(
					$"Не удалось определить товарную группу маркировки для кода {identificationCode}.");
			}

			return new TrueMarkIdentificationCode
			{
				IsInvalid = false,
				RawCode = rawCode,
				Gtin = productInstanceStatus.Gtin,
				SerialNumber = serialNumber,
				ProductGroup = productInstanceStatus.ProductGroup.Value
			};
		}

		public TrueMarkIdentificationCode CreateFromStagingCode(StagingTrueMarkCode stagingCode)
		{
			if(stagingCode?.IsIdentification == false)
			{
				throw new ArgumentException(
					$"Код {stagingCode?.IdentificationCode} не является кодом экземпляра",
					nameof(stagingCode));
			}

			return new TrueMarkIdentificationCode
			{
				IsInvalid = false,
				RawCode = stagingCode.RawCode,
				Gtin = stagingCode.Gtin,
				SerialNumber = stagingCode.SerialNumber,
				CheckCode = stagingCode.CheckCode,
				ProductGroup = stagingCode.ProductGroup
					?? throw new InvalidOperationException(
						$"У кода {stagingCode.IdentificationCode} не указана товарная группа.")
			};
		}
	}
}

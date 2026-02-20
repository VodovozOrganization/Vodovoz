using System;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	internal sealed class TrueMarkTransportCodeFactory : ITrueMarkTransportCodeFactory
	{
		public TrueMarkTransportCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus)
		{
			return new TrueMarkTransportCode
			{
				IsInvalid = false,
				RawCode = productInstanceStatus.IdentificationCode,
			};
		}

		public TrueMarkTransportCode CreateFromRawCode(string scannedCode)
		{
			return new TrueMarkTransportCode
			{
				IsInvalid = false,
				RawCode = scannedCode,
			};
		}

		public TrueMarkTransportCode CreateFromStagingCode(StagingTrueMarkCode stagingCode)
		{
			if(stagingCode?.IsTransport == false)
			{
				throw new ArgumentException(
					$"Код {stagingCode?.IdentificationCode} не является транспортным кодом",
					nameof(stagingCode));
			}

			return new TrueMarkTransportCode
			{
				IsInvalid = false,
				RawCode = stagingCode.RawCode,
			};
		}
	}
}

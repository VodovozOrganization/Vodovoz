using TrueMark.Contracts;
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
	}
}

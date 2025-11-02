using TrueMark.Contracts;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	public interface ITrueMarkTransportCodeFactory
	{
		TrueMarkTransportCode CreateFromRawCode(string scannedCode);
		TrueMarkTransportCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus);
	}
}

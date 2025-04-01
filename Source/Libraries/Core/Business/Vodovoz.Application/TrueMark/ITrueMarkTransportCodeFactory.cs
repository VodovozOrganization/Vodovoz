using TrueMark.Contracts;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	public interface ITrueMarkTransportCodeFactory
	{
		TrueMarkTransportCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus);
	}
}

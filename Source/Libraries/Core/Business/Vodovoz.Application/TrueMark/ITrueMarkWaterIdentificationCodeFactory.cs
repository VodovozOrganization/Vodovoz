using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Application.TrueMark
{
	public interface ITrueMarkWaterIdentificationCodeFactory
	{
		TrueMarkWaterIdentificationCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus);
	}
}

using TrueMark.Contracts;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	public interface ITrueMarkWaterGroupCodeFactory
	{
		TrueMarkWaterGroupCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus);
	}
}
